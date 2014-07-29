// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace Microsoft.Data.Entity.Redis
{
    public static class RedisTestConfig
    {
        public const int ServerStartupTimeMillisecs = 3000;

        internal const string RedisServerExeName = "redis-server.exe";
        internal const string UserProfileRedisNugetPackageServerPath = @".kpm\packages\Redis-64\2.8.9";
        internal const string CIMachineRedisNugetPackageServerPath = @"Redis-64\2.8.9";

        private static volatile Process _redisServerProcess;
        private static object _redisServerProcessLock = new object();

        public static bool GetOrStartServer()
        {
            if (TryUseExistingRedisServer())
            {
                return true;
            }

            string serverPath = GetUserProfileServerPath();
            if (!File.Exists(serverPath))
            {
                serverPath = GetCIMachineServerPath();
                if (!File.Exists(serverPath))
                {
                    return false;
                }
            }

            return RunServer(serverPath);
        }

        private static bool TryUseExistingRedisServer()
        {
            // Does RedisTestConfig already know about a running server?
            if (_redisServerProcess != null
                && !_redisServerProcess.HasExited)
            {
                return true;
            }

            // Otherwise is there a running Redis server which RedisTestConfig can use?
            var existingRedisServer =
                Process.GetProcessesByName(RedisServerExeName).FirstOrDefault();
            if (existingRedisServer != null)
            {
                lock (_redisServerProcessLock)
                {
                    _redisServerProcess = existingRedisServer;
                }
                return true;
            }

            return false;
        }

        public static string GetUserProfileServerPath()
        {
            var configFilePath = Environment.GetEnvironmentVariable("USERPROFILE");
            return Path.Combine(configFilePath, UserProfileRedisNugetPackageServerPath, RedisServerExeName);
        }

        public static string GetCIMachineServerPath()
        {
            var configFilePath = Environment.GetEnvironmentVariable("KRE_PACKAGES");
            return Path.Combine(configFilePath, CIMachineRedisNugetPackageServerPath, RedisServerExeName);
        }

        private static bool RunServer(string serverExePath)
        {
            if (_redisServerProcess == null)
            {
                lock (_redisServerProcessLock)
                {
                    if (_redisServerProcess == null)
                    {
                        _redisServerProcess = Process.Start(serverExePath);
                        // wait for server to complete start-up
                        Thread.Sleep(ServerStartupTimeMillisecs);
                    }
                }
            }

            return true;
        }
    }
}
