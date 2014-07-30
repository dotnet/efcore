// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using StackExchange.Redis;

namespace Microsoft.Data.Entity.Redis
{
    public static class RedisTestConfig
    {
        internal const string RedisServerExeName = "redis-server.exe";
        internal const string UserProfileRedisNugetPackageServerPath = @".kpm\packages\Redis-64\2.8.9";
        internal const string CIMachineRedisNugetPackageServerPath = @"Redis-64\2.8.9";

        private static volatile Process _redisServerProcess; // null implies if server exists it was not started by this code
        private static object _redisServerProcessLock = new object();
        public static int RedisPort = 6375; // override default so that do not interfere with anyone else's server

        public static void GetOrStartServer()
        {
            if (AlreadyOwnRunningRedisServer()) 
            {
                return;
            }

            TryConnectToOrStartServer();
        }

        private static bool AlreadyOwnRunningRedisServer()
        {
            // Does RedisTestConfig already know about a running server?
            if (_redisServerProcess != null
                && !_redisServerProcess.HasExited)
            {
                return true;
            }

            return false;
        }

        private static bool TryConnectToOrStartServer()
        {
            if (CanConnectToExistingRedisServer(3, 100))
            {
                lock (_redisServerProcessLock)
                {
                    _redisServerProcess = null;
                }
                return true;
            }

            return TryStartRedisServer();
        }

        private static bool CanConnectToExistingRedisServer(int numRetries, int sleepMillisecsBetweenRetries = 0)
        {
            var canConnectToServer = false;
            for (var retryCount = 0; retryCount < numRetries; retryCount++)
            {
                try
                {
                    using (var connectionMultiplexer =
                        ConnectionMultiplexer.Connect("127.0.0.1:" + RedisPort))
                    {
                        if (connectionMultiplexer.IsConnected)
                        {
                            canConnectToServer = true;
                            break;
                        }
                    }
                }
                catch (RedisConnectionException)
                {
                    // exception connecting to server - try again
                }

                if (sleepMillisecsBetweenRetries > 0)
                {
                    Thread.Sleep(sleepMillisecsBetweenRetries);
                }
            }

            return canConnectToServer;
        }

        private static bool TryStartRedisServer()
        {
            string serverPath = GetUserProfileServerPath();
            if (!File.Exists(serverPath))
            {
                serverPath = GetCIMachineServerPath();
                if (!File.Exists(serverPath))
                {
                    throw new Exception("Could not find " + RedisServerExeName +
                        " at path " + GetUserProfileServerPath() + " nor at " + GetCIMachineServerPath());
                }
            }

            return RunServer(serverPath);
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

        public static string GetTMPPath()
        {
            var tempPath = Environment.GetEnvironmentVariable("TMP");
            if (tempPath == null)
            {
                tempPath = Environment.GetEnvironmentVariable("TEMP");
                if (tempPath == null)
                {
                    throw new Exception("User does not have a TMP or TEMP environment variable defined.");
                }
            }

            tempPath = Path.Combine(tempPath, "RedisFunctionalTestsServer");
            if (!Directory.Exists(tempPath))
            {
                Directory.CreateDirectory(tempPath);
            }

            return tempPath;
        }

        private static bool RunServer(string serverExePath)
        {
            if (_redisServerProcess == null)
            {
                lock (_redisServerProcessLock)
                {
                    // copy the redis-server.exe to a directory under the user's TMP path. The server
                    // will be left running - so needs not to be under git's working directory as CI
                    // machines run cleanup on those paths before starting tests.
                    var tempPath = GetTMPPath();
                    var tempRedisServerFullPath = Path.Combine(tempPath, RedisServerExeName);
                    if (!File.Exists(tempRedisServerFullPath))
                    {
                        File.Copy(serverExePath, tempRedisServerFullPath);
                    }

                    if (_redisServerProcess == null)
                    {
                        var serverArgs = "--port " + RedisPort;
                        var processInfo = new ProcessStartInfo
                            {
                                // start the process in users TMP dir (a .dat file will be created but will be removed when the server dies)
                                Arguments = serverArgs,
                                WorkingDirectory = tempPath,
                                CreateNoWindow = true,
                                FileName = tempRedisServerFullPath,
                                RedirectStandardError = true,
                                RedirectStandardOutput = true,
                                UseShellExecute = false,
                            };
                        try
                        {
                            _redisServerProcess = Process.Start(processInfo);
                        }
                        catch (Exception e)
                        {
                            throw new Exception("Could not start Redis Server at path "
                                + tempRedisServerFullPath + " with Arguments '" + serverArgs + "', working dir = " + tempPath, e);
                        }

                        if (_redisServerProcess == null)
                        {
                            throw new Exception("Got null process trying to  start Redis Server at path "
                                + tempRedisServerFullPath + " with Arguments '" + serverArgs + "', working dir = " + tempPath);
                        }
                        else if (!CanConnectToExistingRedisServer(5, 1000))
                        {
                            throw new Exception("Cannot connect to started Redis server process PID " + _redisServerProcess.Id);
                        }
                    }
                }
            }

            return true;
        }
    }
}
