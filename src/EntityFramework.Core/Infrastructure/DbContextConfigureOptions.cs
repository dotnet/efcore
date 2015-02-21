// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.ConfigurationModel;
using Microsoft.Framework.OptionsModel;

namespace Microsoft.Data.Entity.Infrastructure
{
    public class DbContextConfigureOptions<TContext> : ConfigureOptions<DbContextOptions<TContext>>
        where TContext : DbContext
    {
        public DbContextConfigureOptions([NotNull] IConfiguration configuration, [NotNull] DbContextOptionsParser parser)
            : base(o =>
                {
                    var extensions = ((IDbContextOptions)o);
                    extensions.RawOptions = parser.ReadRawOptions<TContext>(configuration, extensions.RawOptions);
                })
        {
            Check.NotNull(configuration, nameof(configuration));
            Check.NotNull(parser, nameof(parser));
        }
    }
}
