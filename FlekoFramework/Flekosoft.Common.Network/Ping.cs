using System;
using Flekosoft.Common.Logging;

namespace Flekosoft.Common.Network
{
    public static class Ping
    {
        public static bool Send(string url)
        {
            System.Net.NetworkInformation.PingReply reply;
            return Send(url, out reply);
        }

        public static bool Send(string url, out System.Net.NetworkInformation.PingReply reply)
        {
            reply = null;
            using (System.Net.NetworkInformation.Ping pingSender = new System.Net.NetworkInformation.Ping())
            {
                try
                {
                    reply = pingSender.Send(url);

                    if (reply?.Status != System.Net.NetworkInformation.IPStatus.Success)
                    {
                        return false;
                        //Console.WriteLine("Address: {0}", reply.Address.ToString());
                        //Console.WriteLine("RoundTrip time: {0}", reply.RoundtripTime);
                        //Console.WriteLine("Time to live: {0}", reply.Options.Ttl);
                        //Console.WriteLine("Don't fragment: {0}", reply.Options.DontFragment);
                        //Console.WriteLine("Buffer size: {0}", reply.Buffer.Length);
                    }
                    return true;
                }
                // ReSharper disable once UnusedVariable
                catch (Exception ex)
                {
                    Logger.Instance.AppendException(ex);
                    // ReSharper disable once RedundantJumpStatement
                    return false;
                }
            }
        }
    }
}
