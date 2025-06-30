using System;
using System.Net.Sockets;
using Godot;

public static class NetUtils
{
  public static bool CheckIPv4AddressValidity(string ip)
  {
    if (string.IsNullOrEmpty(ip))
      return false;

    string[] parts = ip.Split('.');
    if (parts.Length != 4)
      return false;

    foreach (string part in parts)
    {
      if (!int.TryParse(part, out int num) || num < 0 || num > 255)
        return false;
    }

    // Link Local Addresses
    if (ip.StartsWith("169.254.")) return false;

    // Loopback Address
    if (ip.StartsWith("127.")) return false;

    // Bogon Addresses
    if (int.TryParse(parts[0], out int firstOctet))
    {
      if (firstOctet >= 224) return false;
    }
    if (ip.StartsWith("192.0.2.")) return false;
    if (ip.StartsWith("198.51.100.")) return false;
    if (ip.StartsWith("203.0.113.")) return false;
    if (ip.StartsWith("198.18.")) return false;

    return true;
  }

  public static string[] GetValidIPv4Addresses()
  {
    string[] ips = IP.GetLocalAddresses();
    string[] validIPs = Array.FindAll(ips, CheckIPv4AddressValidity);
    return validIPs;
  }
}