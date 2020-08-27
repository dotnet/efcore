// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;

namespace Microsoft.EntityFrameworkCore
{
    public class PortArranger
    {
        private static int _nextPort = 11000;

        public static int Reserve()
        {
            var attempts = 0;
            while (attempts++ < 10)
            {
                var port = Interlocked.Increment(ref _nextPort);
                if (port >= 65535)
                {
                    throw new OverflowException("Cannot get an available port, port value overflowed");
                }

                if (IsFree(port))
                {
                    return port;
                }
            }

            throw new TimeoutException(string.Format("Cannot get an available port in {0} attempts.", attempts));
        }

        private static bool IsFree(int port)
        {
            var properties = IPGlobalProperties.GetIPGlobalProperties();
            var connections = properties.GetActiveTcpConnections();

            return !connections.Any(c => c.LocalEndPoint.Port == port || c.RemoteEndPoint.Port == port);
        }
    }
}
