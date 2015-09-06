// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.SqlClient;
using Microsoft.Framework.Configuration;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public static class SqlConnectionStringBuilderExtensions
    {
        private const string DefaultDataSource = @"(localdb)\MSSQLLocalDB";
        private const int DefaultConnectTimeout = 30;
        private const bool DefaultIntegratedSecurity = true;

        private static readonly IConfiguration _config;

        static SqlConnectionStringBuilderExtensions()
        {
            var configBuilder = new ConfigurationBuilder(".")
                .AddJsonFile("config.json", optional: true)
                .AddJsonFile("config.test.json", optional: true)
                .AddEnvironmentVariables();

            _config = configBuilder.Build()
                        .GetSection("Test:SqlServer");
        }

        public static SqlConnectionStringBuilder ApplyConfiguration(this SqlConnectionStringBuilder builder)
        {
            int connectTimeout;
            if (!int.TryParse(_config["ConnectTimeout"], out connectTimeout))
            {
                connectTimeout = DefaultConnectTimeout;
            }

            builder.ConnectTimeout = connectTimeout;

            bool integratedSecurity;
            if (!bool.TryParse(_config["IntegratedSecurity"], out integratedSecurity))
            {
                integratedSecurity = DefaultIntegratedSecurity;
            }

            builder.IntegratedSecurity = integratedSecurity;

            builder.DataSource = string.IsNullOrEmpty(_config["DataSource"]) ? DefaultDataSource : _config["DataSource"];

            if (!string.IsNullOrEmpty(_config["UserID"]))
            {
                builder.UserID = _config["UserID"];
            }

            if (!string.IsNullOrEmpty(_config["Password"]))
            {
                builder.Password = _config["Password"];
            }

            return builder;
        }
    }
}
