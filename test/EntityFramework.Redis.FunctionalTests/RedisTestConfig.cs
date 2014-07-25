// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.Framework.ConfigurationModel;

namespace Microsoft.Data.Entity.Redis
{
    public static class RedisTestConfig
    {
        private const string RedisServerExeName = "redis-server";
        private const string RedisTestConfigFilename = "RedisTest.config";

        private static volatile Process _redisServerProcess;
        private static bool _startedRedisServer;
        private static object _redisServerProcessLock = new object();
        private static int _serverTimeoutInSecs = 10; // default timeout in secs

        public static bool StartServer()
        {
            if (TryAssignExistingRedisServer())
            {
                return true;
            }

            var configFilePath = Environment.GetEnvironmentVariable("USERPROFILE");
            string serverPath = null;
            if (File.Exists(Path.Combine(configFilePath, RedisTestConfigFilename)))
            {
                try
                {
                    var configuration = new Configuration();
                    configuration.AddXmlFile(Path.Combine(configFilePath, RedisTestConfigFilename));
                    if (configuration.TryGet("RedisServer:Path", out serverPath))
                    {
                        serverPath = serverPath.Trim();
                    }

                    string timeoutInSecs = null;
                    if (configuration.TryGet("RedisServer:TimeoutInSecs", out timeoutInSecs))
                    {
                        int.TryParse(timeoutInSecs.Trim(), out _serverTimeoutInSecs);
                    }
                }
                catch (XmlException)
                {
                    // do nothing - could not read XML file
                }
            }

            if (serverPath == null
                || !File.Exists(Path.Combine(serverPath, RedisServerExeName + ".exe")))
            {
                return false;
            }
            else
            {
                return RunServer(Path.Combine(serverPath, RedisServerExeName));
            }
        }

        public static void StopServer()
        {
            if (_redisServerProcess != null)
            {
                lock (_redisServerProcessLock)
                {
                    if (_redisServerProcess != null && _startedRedisServer)
                    {
                        _redisServerProcess.Kill();
                        _redisServerProcess = null;
                    }
                }
            }
        }

        public static int ServerTimeoutInSecs
        {
            get { return _serverTimeoutInSecs; }
        }

        private static bool TryAssignExistingRedisServer()
        {
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

        private static bool RunServer(string serverExePath)
        {
            if (_redisServerProcess == null)
            {
                lock (_redisServerProcessLock)
                {
                    if (_redisServerProcess == null)
                    {
                        _redisServerProcess = Process.Start(serverExePath);
                        _startedRedisServer = true;
                    }
                }
            }

            return true;
        }
    }
}
