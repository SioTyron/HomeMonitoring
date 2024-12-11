using System;
using HM.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.NetworkInformation;
using System.Linq;

namespace HM.Controllers
{
    public class NetworkController : Controller
    {
        private readonly NetworkScanner _scanner;

        public NetworkController()
        {
            _scanner = new NetworkScanner();
        }

        [HttpPost]
        public IActionResult Scan()
        {
            string subnet = GetSubnet();
            if (string.IsNullOrEmpty(subnet))
            {
                return new JsonResult(new { error = "Impossible de déterminer le sous-réseau." });
            }

            // Lancer le scan avec le sous-réseau détecté
            var devices = _scanner.ScanNetwork(subnet);
            return new JsonResult(devices); // Retourner les appareils trouvés
        }

        private string GetSubnet()
        {
            try
            {
                var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(ni => ni.OperationalStatus == OperationalStatus.Up &&
                                 ni.NetworkInterfaceType != NetworkInterfaceType.Loopback);

                foreach (var networkInterface in networkInterfaces)
                {
                    var properties = networkInterface.GetIPProperties();
                    var unicastAddresses = properties.UnicastAddresses
                        .Where(ua => ua.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);

                    foreach (var unicastAddress in unicastAddresses)
                    {
                        var ipAddress = unicastAddress.Address;
                        var mask = unicastAddress.IPv4Mask;

                        if (ipAddress != null && mask != null)
                        {
                            // Calculer le sous-réseau à partir de l'adresse IP et du masque
                            var subnetBytes = ipAddress.GetAddressBytes()
                                .Zip(mask.GetAddressBytes(), (ip, maskByte) => (byte)(ip & maskByte))
                                .ToArray();

                            // Construire le sous-réseau en notation décimale pointée (ex. : 192.168.1)
                            var subnet = string.Join('.', subnetBytes.Take(3));
                            return subnet;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Gérer les exceptions et éventuellement logger l'erreur
                Console.WriteLine($"Erreur lors de la détection du sous-réseau : {ex.Message}");
            }

            return null;
        }
    }
}
