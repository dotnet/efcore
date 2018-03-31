// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;
using EntityFrameworkServiceCollectionExtensions = Microsoft.Extensions.DependencyInjection.EntityFrameworkServiceCollectionExtensions;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public class ApiConsistencyTest : ApiConsistencyTestBase
    {
        private static readonly Type[] _fluentApiTypes =
        {
            typeof(ModelBuilder),
            typeof(CollectionNavigationBuilder),
            typeof(CollectionNavigationBuilder<SampleEntity, SampleEntity>),
            typeof(EntityTypeBuilder),
            typeof(EntityTypeBuilder<>),
            typeof(IndexBuilder),
            typeof(KeyBuilder),
            typeof(PropertyBuilder),
            typeof(PropertyBuilder<>),
            typeof(ReferenceCollectionBuilder),
            typeof(ReferenceCollectionBuilder<SampleEntity, SampleEntity>),
            typeof(ReferenceNavigationBuilder),
            typeof(ReferenceNavigationBuilder<SampleEntity, SampleEntity>),
            typeof(ReferenceReferenceBuilder),
            typeof(ReferenceReferenceBuilder<SampleEntity, SampleEntity>),
            typeof(DbContextOptionsBuilder),
            typeof(DbContextOptionsBuilder<DbContext>),
            typeof(EntityFrameworkServiceCollectionExtensions)
        };

        protected override void AddServices(ServiceCollection serviceCollection)
        {
            new EntityFrameworkServicesBuilder(serviceCollection).TryAddCoreServices();
        }

        public class SampleEntity
        {
        }

        protected override bool ShouldHaveNotNullAnnotation(MethodBase method, Type type)
            => base.ShouldHaveNotNullAnnotation(method, type)
               && method.Name != nameof(DbContext.OnConfiguring)
               && method.Name != nameof(DbContext.OnModelCreating)
               && !(type == typeof(IEntityTypeConfiguration<>)
                    && method.Name == nameof(IEntityTypeConfiguration<object>.Configure));

        protected override bool ShouldHaveVirtualMethods(Type type) => type != typeof(InternalEntityEntry);

        protected override IEnumerable<Type> FluentApiTypes => _fluentApiTypes;

        protected override Assembly TargetAssembly => typeof(EntityType).GetTypeInfo().Assembly;
    }
}
