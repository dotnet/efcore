// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.SqlClient;
using Microsoft.Data.Entity.FunctionalTests.TestUtilities;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public static class SqlConnectionStringBuilderExtensions
    {
        public const string DefaultDataSource = @"(localdb)\MSSQLLocalDB";
        public const string DataSourceKeyword = "DataSource";
        private const int DefaultConnectTimeout = 30;
        private const bool DefaultIntegratedSecurity = true;

        public static SqlConnectionStringBuilder ApplyConfiguration(this SqlConnectionStringBuilder builder)
        {
            var config = TestEnvironment.Config;

            int connectTimeout;
            if (!int.TryParse(config["ConnectTimeout"], out connectTimeout))
            {
                connectTimeout = DefaultConnectTimeout;
            }

            builder.ConnectTimeout = connectTimeout;

            bool integratedSecurity;
            if (!bool.TryParse(config["IntegratedSecurity"], out integratedSecurity))
            {
                integratedSecurity = DefaultIntegratedSecurity;
            }

            builder.IntegratedSecurity = integratedSecurity;

            builder.DataSource = string.IsNullOrEmpty(config[DataSourceKeyword]) ? DefaultDataSource : config[DataSourceKeyword];

            if (!string.IsNullOrEmpty(config["UserID"]))
            {
                builder.UserID = config["UserID"];
            }

            if (!string.IsNullOrEmpty(config["Password"]))
            {
                builder.Password = config["Password"];
                builder.IntegratedSecurity = false;

                if (!TestPlatformHelper.IsMono)
                {
                    builder.PersistSecurityInfo = true;
                }
            }

            return builder;
        }
    }
}
