using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace GOKCafe.Web.Helpers;

public static class MobileAccessHelper
{
    public static void DisplayMobileAccessInfo()
    {
        var addresses = NetworkInterface.GetAllNetworkInterfaces()
            .Where(ni => ni.OperationalStatus == OperationalStatus.Up &&
                         ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
            .SelectMany(ni => ni.GetIPProperties().UnicastAddresses)
            .Where(addr => addr.Address.AddressFamily == AddressFamily.InterNetwork)
            .Select(addr => addr.Address.ToString())
            .ToList();

        Console.WriteLine("\n========================================");
        Console.WriteLine("GOK Cafe is running!");
        Console.WriteLine("========================================");
        Console.WriteLine("\nAccess from mobile device:");

        foreach (var ip in addresses)
        {
            Console.WriteLine($"   HTTP:  http://{ip}:25718");
            Console.WriteLine($"   HTTPS: https://{ip}:44317");
        }

        Console.WriteLine("\nLocal access:");
        Console.WriteLine("   HTTP:  http://localhost:25718");
        Console.WriteLine("   HTTPS: https://localhost:44317");
        Console.WriteLine("\nMake sure your phone is on the SAME WiFi network!");
        Console.WriteLine("\nFirst time setup? Run as Administrator:");
        Console.WriteLine("   netsh advfirewall firewall add rule name=\"GOK Cafe HTTP\" dir=in action=allow protocol=TCP localport=25718");
        Console.WriteLine("   netsh advfirewall firewall add rule name=\"GOK Cafe HTTPS\" dir=in action=allow protocol=TCP localport=44317");
        Console.WriteLine("========================================\n");
    }
}
