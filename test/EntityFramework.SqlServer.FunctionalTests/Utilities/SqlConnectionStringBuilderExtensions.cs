// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.SqlClient;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public static class SqlConnectionStringBuilderExtensions
    {
        private const string DefaultDataSource = @"(localdb)\MSSQLLocalDB";
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

            builder.DataSource = string.IsNullOrEmpty(config["DataSource"]) ? DefaultDataSource : config["DataSource"];

            if (!string.IsNullOrEmpty(config["UserID"]))
            {
                builder.UserID = config["UserID"];
            }

            if (!string.IsNullOrEmpty(config["Password"]))
            {
                builder.Password = config["Password"];
            }

            return builder;
        }
    }
}
