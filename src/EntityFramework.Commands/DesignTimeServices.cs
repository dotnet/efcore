// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Commands.Migrations;
using Microsoft.Data.Entity.Commands.Utilities;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.Commands
{
    public class DesignTimeServices : IServiceProvider
    {
        private readonly IServiceProvider _runtimeServices;
        private readonly IServiceProvider _designTimeServices;

        public DesignTimeServices([NotNull] IServiceProvider runtimeServices)
        {
            Check.NotNull(runtimeServices, nameof(runtimeServices));

            _runtimeServices = runtimeServices;
            _designTimeServices = new ServiceCollection()
                .AddScoped<MigrationScaffolder>()
                .AddSingleton<MigrationCodeGenerator, CSharpMigrationGenerator>()
                .AddSingleton<CSharpHelper>()
                .AddSingleton<CSharpMigrationOperationGenerator>()
                .AddSingleton<CSharpModelGenerator>()
                .BuildServiceProvider();
        }

        public virtual object GetService(Type serviceType)
        {
            return _runtimeServices.GetService(serviceType) ?? _designTimeServices.GetService(serviceType);
        }
    }
}
