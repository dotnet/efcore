// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Framework.ConfigurationModel;
using Microsoft.Framework.OptionsModel;

namespace Microsoft.Data.Entity
{
    public class DbContextConfigureOptions<TContext> : ConfigureOptions<DbContextOptions<TContext>>
        where TContext : DbContext
    {
        public DbContextConfigureOptions([NotNull] IConfiguration configuration)
            : base(options => options.ReadRawOptions(configuration))
        {
        }
    }
}
