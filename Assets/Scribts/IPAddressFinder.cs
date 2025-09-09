using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Linq;

public static class IPAddressFinder
{
    public static string GetLocalIPv4Address()
    {
        if (!NetworkInterface.GetIsNetworkAvailable())
        {
            return "No network available";
        }
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        return "Local IP not found";
    }
}
