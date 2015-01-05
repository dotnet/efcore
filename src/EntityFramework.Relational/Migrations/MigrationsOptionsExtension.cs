// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Migrations
{
    public class MigrationsOptionsExtension : DbContextOptionsExtension
    {
        private Assembly _migrationAssembly;
        private string _migrationNamespace;

        public virtual Assembly MigrationAssembly
        {
            get { return _migrationAssembly; }

            [param: NotNull] set { _migrationAssembly = Check.NotNull(value, "value"); }
        }

        public virtual string MigrationNamespace
        {
            get { return _migrationNamespace; }

            [param: NotNull] set { _migrationNamespace = Check.NotEmpty(value, "value"); }
        }

        protected override void ApplyServices(EntityServicesBuilder builder)
        {
        }

        public static MigrationsOptionsExtension Extract([NotNull] IDbContextOptions options)
        {
            Check.NotNull(options, "options");

            return
                options.Extensions
                    .OfType<MigrationsOptionsExtension>()
                    .SingleOrDefault();
        }
    }
}
