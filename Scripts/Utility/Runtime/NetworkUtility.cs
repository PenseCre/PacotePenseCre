using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace PacotePenseCre.Utility
{
    /// <summary>
    ///     Singleton class providing networks utilities to get MAC and Ip addresses, to check if internet is active, etc.
    /// </summary>
    public class NetworkUtility
    {
        #region public methods

        /// <summary>
        ///     Returns the MAC address of the computer.
        /// </summary>
        /// <returns>Returns the MAC address if found in the NetworkInterafaces. Returns null if nothing is found </returns>
        public static string GetMacAddress()
        {
            foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (networkInterface.NetworkInterfaceType != NetworkInterfaceType.Ethernet)
                    continue;

                return networkInterface.GetPhysicalAddress().ToString();
            }
            return null;
        }

        /// <summary>
        ///     Returns the local ip4 address.
        /// </summary>
        /// <returns>Returns the local ip4 address.</returns>
        public static string GetLocalIpAddress()
        {
            IPHostEntry ipHostEntry = Dns.GetHostEntry(Dns.GetHostName());
            return Convert.ToString(
                ipHostEntry.AddressList.FirstOrDefault(address => address.AddressFamily == AddressFamily.InterNetwork));
        }

        #endregion
    }
}