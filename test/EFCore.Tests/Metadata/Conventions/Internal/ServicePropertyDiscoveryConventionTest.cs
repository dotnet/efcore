// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.InMemory.Storage.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    public class ServicePropertyDiscoveryConventionTest
    {
        [Fact]
        public void Finds_one_service_property()
        {
            var entityType = ApplyConvention<BlogOneService>();

            var serviceProperty = entityType.FindServiceProperty(nameof(BlogOneService.Loader));
            var binding = serviceProperty.ParameterBinding;

            Assert.Equal(typeof(ILazyLoader), binding.ParameterType);
            Assert.Equal(typeof(ILazyLoader), binding.ServiceType);
        }

        [Fact]
        public void Does_not_find_service_property_configured_as_property()
        {
            var entityType = new Model().AddEntityType(typeof(BlogOneService), ConfigurationSource.Explicit);
            entityType.Builder.Property(typeof(ILazyLoader), nameof(BlogOneService.Loader), ConfigurationSource.Explicit)
                .HasConversion(typeof(string), ConfigurationSource.Explicit);

            ApplyConvention(entityType);

            Assert.NotNull(entityType.FindProperty(nameof(BlogOneService.Loader)));
            Assert.Null(entityType.FindServiceProperty(nameof(BlogOneService.Loader)));
        }

        [Fact]
        public void Does_not_find_service_property_configured_as_navigation()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(BlogOneService), ConfigurationSource.Explicit);
            entityType.Builder.HasRelationship(
                model.AddEntityType(typeof(LazyLoader), ConfigurationSource.Explicit),
                nameof(BlogOneService.Loader), ConfigurationSource.Explicit);

            ApplyConvention(entityType);

            Assert.NotNull(entityType.FindNavigation(nameof(BlogOneService.Loader)));
            Assert.Null(entityType.FindServiceProperty(nameof(BlogOneService.Loader)));
        }

        [Fact]
        public void Does_not_find_duplicate_service_properties()
        {
            var typeMappingSource = TestServiceFactory.Instance.Create<InMemoryTypeMappingSource>();
            var convention = TestServiceFactory.Instance.Create<ServicePropertyDiscoveryConvention>(
                (typeof(ITypeMappingSource), typeMappingSource));

            var entityType = new Model().AddEntityType(typeof(BlogDuplicateService), ConfigurationSource.Explicit);

            convention.Apply(entityType.Builder);

            Assert.Empty(entityType.GetServiceProperties());

            Assert.Equal(
                CoreStrings.AmbiguousServiceProperty(
                    nameof(BlogDuplicateService.ContextTwo), nameof(DbContext), nameof(BlogDuplicateService)),
                Assert.Throws<InvalidOperationException>(
                    () =>
                        TestServiceFactory.Instance.Create<ServicePropertyDiscoveryConvention>().Apply(entityType.Model.Builder)).Message);
        }

        [Fact]
        public void Finds_service_property_duplicate_ignored()
        {
            var typeMappingSource = TestServiceFactory.Instance.Create<InMemoryTypeMappingSource>();
            var convention = TestServiceFactory.Instance.Create<ServicePropertyDiscoveryConvention>(
                (typeof(ITypeMappingSource), typeMappingSource));

            var entityType = new Model().AddEntityType(typeof(BlogDuplicateService), ConfigurationSource.Explicit);

            convention.Apply(entityType.Builder);

            Assert.Empty(entityType.GetServiceProperties());

            entityType.Builder.Ignore(nameof(BlogDuplicateService.ContextTwo), ConfigurationSource.Convention);

            convention.Apply(entityType.Builder, nameof(BlogDuplicateService.ContextTwo));

            Assert.NotNull(entityType.FindServiceProperty(nameof(BlogDuplicateService.ContextOne)));

            convention.Apply(entityType.Model.Builder);
        }

        private static EntityType ApplyConvention<TEntity>()
            => ApplyConvention(new Model().AddEntityType(typeof(TEntity), ConfigurationSource.Explicit));

        private static EntityType ApplyConvention(EntityType entityType)
        {
            entityType.AddProperty(nameof(Blog.Id), typeof(int), ConfigurationSource.Explicit, ConfigurationSource.Explicit);

            var typeMappingSource = TestServiceFactory.Instance.Create<InMemoryTypeMappingSource>();
            TestServiceFactory.Instance.Create<ServicePropertyDiscoveryConvention>(
                    (typeof(ITypeMappingSource), typeMappingSource))
                .Apply(entityType.Builder);

            return entityType;
        }

        private class BlogOneService : Blog
        {
            public ILazyLoader Loader { get; set; }
        }

        private class BlogDuplicateService : Blog
        {
            public DbContext ContextOne { get; set; }
            public DbContext ContextTwo { get; set; }
        }

        private abstract class Blog
        {
            public int Id { get; set; }
        }
    }
}
