using System;
using System.Globalization;
using System.Net;

namespace StackExchange.Redis
{
    internal static class Format
    {
        public static int ParseInt32(string s)
        {
            return int.Parse(s, NumberStyles.Integer, NumberFormatInfo.InvariantInfo);
        }

        public static long ParseInt64(string s)
        {
            return long.Parse(s, NumberStyles.Integer, NumberFormatInfo.InvariantInfo);
        }

        public static string ToString(int value)
        {
            return value.ToString(NumberFormatInfo.InvariantInfo);
        }

        public static bool TryParseBoolean(string s, out bool value)
        {
            if (bool.TryParse(s, out value)) return true;

            if (s == "1" || string.Equals(s, "yes", StringComparison.OrdinalIgnoreCase) || string.Equals(s, "on", StringComparison.OrdinalIgnoreCase))
            {
                value = true;
                return true;
            }
            if (s == "0" || string.Equals(s, "no", StringComparison.OrdinalIgnoreCase) || string.Equals(s, "off", StringComparison.OrdinalIgnoreCase))
            {
                value = false;
                return true;
            }
            value = false;
            return false;
        }

        public static bool TryParseInt32(string s, out int value)
        {
            return int.TryParse(s, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out value);
        }
        internal static EndPoint ParseEndPoint(string host, int port)
        {
            IPAddress ip;
            if (IPAddress.TryParse(host, out ip)) return new IPEndPoint(ip, port);
            return new DnsEndPoint(host, port);
        }
        internal static EndPoint TryParseEndPoint(string host, string port)
        {
            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(port)) return null;
            int i;
            return TryParseInt32(port, out i) ? ParseEndPoint(host, i) : null;
        }

        internal static string ToString(long value)
        {
            return value.ToString(NumberFormatInfo.InvariantInfo);
        }

        internal static string ToString(double value)
        {
            if (double.IsInfinity(value))
            {
                if (double.IsPositiveInfinity(value)) return "+inf";
                if (double.IsNegativeInfinity(value)) return "-inf";
            }
            return value.ToString("G17", NumberFormatInfo.InvariantInfo);
        }

        internal static string ToString(object value)
        {
            return Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        internal static string ToString(EndPoint endpoint)
        {   
            var dns = endpoint as DnsEndPoint;
            if (dns != null)
            {
                if (dns.Port == 0) return dns.Host;
                return dns.Host + ":" + Format.ToString(dns.Port);
            }
            var ip = endpoint as IPEndPoint;
            if (ip != null)
            {
                if (ip.Port == 0) return ip.Address.ToString();
                return ip.Address.ToString() + ":" + Format.ToString(ip.Port);
            }
            return endpoint == null ? "" : endpoint.ToString();
        }
        internal static string ToStringHostOnly(EndPoint endpoint)
        {
            var dns = endpoint as DnsEndPoint;
            if (dns != null)
            {
                return dns.Host;
            }
            var ip = endpoint as IPEndPoint;
            if(ip != null)
            {
                return ip.Address.ToString();
            }
            return "";
        }

        internal static bool TryGetHostPort(EndPoint endpoint, out string host, out int port)
        {
            if (endpoint != null)
            {
                if (endpoint is IPEndPoint)
                {
                    IPEndPoint ip = (IPEndPoint)endpoint;
                    host = ip.Address.ToString();
                    port = ip.Port;
                    return true;
                }
                else if (endpoint is DnsEndPoint)
                {
                    DnsEndPoint dns = (DnsEndPoint)endpoint;
                    host = dns.Host;
                    port = dns.Port;
                    return true;
                }
            }
            host = null;
            port = 0;
            return false;
        }

        internal static bool TryParseDouble(string s, out double value)
        {
            if(s == null || s.Length == 0)
            {
                value = 0;
                return false;
            }
            if(s.Length==1 && s[0] >= '0' && s[0] <= '9')
            {
                value = (int)(s[0] - '0');
                return true;
            }
            // need to handle these
            if(string.Equals("+inf", s, StringComparison.OrdinalIgnoreCase))
            {
                value = double.PositiveInfinity;
                return true;
            }
            if(string.Equals("-inf", s, StringComparison.OrdinalIgnoreCase))
            {
                value = double.NegativeInfinity;
                return true;
            }
            return double.TryParse(s, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out value);
        }
        internal static EndPoint TryParseEndPoint(string endpoint)
        {
            if (string.IsNullOrWhiteSpace(endpoint)) return null;
            string host;
            int port;
            int i = endpoint.IndexOf(':');
            if (i < 0)
            {
                host = endpoint;
                port = 0;
            }
            else
            {
                host = endpoint.Substring(0, i);
                var portAsString = endpoint.Substring(i + 1);
                if (string.IsNullOrEmpty(portAsString)) return null;
                if (!Format.TryParseInt32(portAsString, out port)) return null;
            }
            if (string.IsNullOrWhiteSpace(host)) return null;

            return Format.ParseEndPoint(host, port);
        }
    }
}
