// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Migrations.Design;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.Design.Internal
{
    public static class DesignTimeServices
    {
        public static IServiceProvider Build([NotNull] DbContext context)
        {
            Check.NotNull(context, nameof(context));

            var serviceCollection = new ServiceCollection();
            Configure(serviceCollection);

            return new AggregateServiceProvider(
                ((IAccessor<IServiceProvider>)context).Service,
                serviceCollection.BuildServiceProvider());
        }

        private static void Configure(ServiceCollection serviceCollection)
            => serviceCollection
                .AddScoped<MigrationsScaffolder>()
                .AddSingleton<MigrationsCodeGenerator, CSharpMigrationsGenerator>()
                .AddSingleton<CSharpHelper>()
                .AddSingleton<CSharpMigrationOperationGenerator>()
                .AddSingleton<CSharpSnapshotGenerator>();
    }
}
