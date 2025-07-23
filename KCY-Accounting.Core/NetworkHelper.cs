using System.Net.NetworkInformation;

namespace KCY_Accounting.Core;
public class NetworkHelper
{
    public bool HasInternetConnection()
    {
        try
        {
            using var ping = new Ping();
            var reply = ping.Send("8.8.8.8", 2000);
            return reply.Status == IPStatus.Success;
        }
        catch
        {
            return false;
        }
    }
}