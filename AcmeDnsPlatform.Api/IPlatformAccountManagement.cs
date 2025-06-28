using System.Collections;
using System.Net;
using System.Net.Sockets;

namespace AcmeDnsPlatform.Api;

public interface IPlatformAccountManagement
{
    public Account RegisterAccount(List<string> allowFrom);

    public Account GetAccount(string username);
    
    public bool CheckCredentials(string username, string password, string ip);
    
    /* Special thanks to Ryan (https://www.ryadel.com/en/author/ryan/) and Vivien Sonntag (https://stackoverflow.com/users/3085985/vivien-sonntag) */
    /* From https://www.ryadel.com/en/check-ip-address-within-given-subnet-mask-c-sharp/ */
    /// <summary>
    /// Returns TRUE if the given IP address is contained in the given subnetmask, FALSE otherwise.
    /// Examples:
    /// - IsInSubnet("192.168.5.1", "192.168.5.85/24") -> TRUE
    /// - IsInSubnet("192.168.5.1", "192.168.5.85/32") -> FALSE
    /// ref.: https://stackoverflow.com/a/56461160
    /// </summary>
    /// <param name="address">The IP Address to check</param>
    /// <param name="subnetMask">The SubnetMask</param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public static bool IsInSubnetMask(string ipAddress, string subnetMask)
    {
        var address = IPAddress.Parse(ipAddress);
        var slashIdx = subnetMask.IndexOf("/");
        if (slashIdx == -1)
            // We only handle netmasks in format "IP/PrefixLength".
            throw new NotSupportedException("Only SubNetMasks with a given prefix length are supported.");
     
        // First parse the address of the netmask before the prefix length.
        var maskAddress = IPAddress.Parse(subnetMask.Substring(0, slashIdx));
     
        if (maskAddress.AddressFamily != address.AddressFamily)
            // We got something like an IPV4-Address for an IPv6-Mask. This is not valid.
            return false;
     
        // Now find out how long the prefix is.
        int maskLength = int.Parse(subnetMask.Substring(slashIdx + 1));
     
        if (maskLength == 0)
            return true;
     
        if (maskLength < 0)
            throw new NotSupportedException("A Subnetmask should not be less than 0.");
     
        if (maskAddress.AddressFamily == AddressFamily.InterNetwork)
        {
            var maskAddressBits = BitConverter.ToUInt32(maskAddress.GetAddressBytes().Reverse().ToArray(), 0);
            var ipAddressBits = BitConverter.ToUInt32(address.GetAddressBytes().Reverse().ToArray(), 0);
            uint mask = uint.MaxValue << (32 - maskLength);
     
            // https://stackoverflow.com/a/1499284/3085985
            // Bitwise AND mask and MaskAddress, this should be the same as mask and IpAddress
            // as the end of the mask is 0000 which leads to both addresses to end with 0000
            // and to start with the prefix.
            return (maskAddressBits & mask) == (ipAddressBits & mask);
        }
     
        if (maskAddress.AddressFamily == AddressFamily.InterNetworkV6)
        {
            // Convert the mask address to a BitArray. Reverse the BitArray to compare the bits of each byte in the right order.
            var maskAddressBits = new BitArray(maskAddress.GetAddressBytes().Reverse().ToArray());
     
            // And convert the IpAddress to a BitArray. Reverse the BitArray to compare the bits of each byte in the right order.
            var ipAddressBits = new BitArray(address.GetAddressBytes().Reverse().ToArray());
            var ipAddressLength = ipAddressBits.Length;
     
            if (maskAddressBits.Length != ipAddressBits.Length)
                throw new ArgumentException("Length of IP Address and Subnet Mask do not match.");
     
            // Compare the prefix bits.
            for (var i = ipAddressLength - 1; i >= ipAddressLength - maskLength; i--)
                if (ipAddressBits[i] != maskAddressBits[i])
                    return false;
     
            return true;
        }
     
        return false;
    }
}