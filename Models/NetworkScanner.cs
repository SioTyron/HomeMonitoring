using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace HM.Models
{
    public class NetworkScanner
    {
        public List<NetworkDevice> ScanNetwork(string subnet)
        {
            var devices = new List<NetworkDevice>();
            var lockObject = new object(); // Utilisé pour synchroniser l'accès à la liste

            Parallel.For(1, 255, i =>
            {
                string ip = $"{subnet}.{i}";
                Ping ping = new Ping();
                try
                {
                    PingReply reply = ping.Send(ip, 100);
                    if (reply.Status == IPStatus.Success)
                    {
                        var device = new NetworkDevice
                        {
                            IPAddress = ip,
                            Status = "online",
                            RoundTripTime = reply.RoundtripTime
                        };

                        // Synchroniser l'accès à la liste
                        lock (lockObject)
                        {
                            devices.Add(device);
                        }
                    }
                }
                catch
                {
                    // Ignorer les erreurs de ping
                }
            });

            return devices; // Retourner la liste des appareils
        }
    }
}
