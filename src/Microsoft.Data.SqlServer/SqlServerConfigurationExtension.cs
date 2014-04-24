// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.AspNet.DependencyInjection;
using Microsoft.Data.Entity;
using Microsoft.Data.SqlServer.Utilities;

namespace Microsoft.Data.SqlServer
{
    public class SqlServerConfigurationExtension : EntityConfigurationExtension
    {
        public virtual string ConnectionString { get; internal set; }

        protected override void ApplyServices(EntityServicesBuilder builder)
        {
            Check.NotNull(builder, "builder");

            builder.AddSqlServer();
        }
    }
}
