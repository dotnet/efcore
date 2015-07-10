// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.SqlClient;
using Microsoft.Framework.Configuration;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class SqlServerTestConfig
    {
        private const string prefix = "Test:SqlServer:";

        private static readonly Lazy<SqlServerTestConfig> _instance = new Lazy<SqlServerTestConfig>(() =>
            {
                var config = new ConfigurationBuilder(".")
                    .AddJsonFile("config.json")
                    .AddJsonFile("config.test.json", optional: true)
                    .AddEnvironmentVariables()
                    .Build();

                int connectTimeout;
                if (!int.TryParse(config.Get(prefix + "ConnectTimeout"), out connectTimeout))
                {
                    connectTimeout = 30;
                }

                return new SqlServerTestConfig
                {
                    DataSource = config.Get(prefix + "DataSource"),
                    IntegratedSecurity = bool.Parse(config.Get(prefix + "IntegratedSecurity")),
                    ConnectTimeout = connectTimeout,
                    UserId = config.Get(prefix + "UserId"),
                    Password = config.Get(prefix + "Password")
                };
            });

        private SqlServerTestConfig()
        {
        }

        public static SqlServerTestConfig Instance => _instance.Value;

        public string DataSource { get; private set; }
        public bool IntegratedSecurity { get; private set; }
        public int ConnectTimeout { get; private set; }
        public string UserId { get; private set; }
        public string Password { get; private set; }

        public SqlConnectionStringBuilder ConnectionStringBuilder
            => new SqlConnectionStringBuilder
            {
                DataSource = DataSource,
                IntegratedSecurity = IntegratedSecurity,
                ConnectTimeout = ConnectTimeout,
                UserID = UserId,
                Password = Password
            };
    }
}
