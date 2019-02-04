// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore
{
    public class ApiConsistencyTest : ApiConsistencyTestBase
    {
        private static readonly Type[] _fluentApiTypes =
        {
            typeof(RelationalQueryTypeBuilderExtensions),
            typeof(RelationalReferenceReferenceBuilderExtensions),
            typeof(RelationalReferenceCollectionBuilderExtensions),
            typeof(RelationalOwnershipBuilderExtensions),
            typeof(RelationalOwnedNavigationBuilderExtensions),
            typeof(RelationalPropertyBuilderExtensions),
            typeof(RelationalModelBuilderExtensions),
            typeof(RelationalIndexBuilderExtensions),
            typeof(RelationalKeyBuilderExtensions),
            typeof(RelationalEntityTypeBuilderExtensions),
            typeof(MigrationBuilder),
            typeof(AlterOperationBuilder<>),
            typeof(ColumnsBuilder),
            typeof(CreateTableBuilder<>),
            typeof(OperationBuilder<>)
        };

        protected override IEnumerable<Type> FluentApiTypes => _fluentApiTypes;

        protected override bool ShouldHaveVirtualMethods(Type type)
            => type.Name != "EntityShaper";

        protected override void AddServices(ServiceCollection serviceCollection)
        {
            new EntityFrameworkRelationalServicesBuilder(serviceCollection).TryAddCoreServices();
        }

        protected override Assembly TargetAssembly => typeof(RelationalDatabase).GetTypeInfo().Assembly;
    }
}
