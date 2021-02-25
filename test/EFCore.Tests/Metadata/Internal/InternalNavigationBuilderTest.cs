// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public class InternalNavigationBuilderTest
    {
        [ConditionalFact]
        public void Can_only_override_lower_or_equal_source_HasField()
        {
            var builder = CreateInternalNavigationBuilder();
            var metadata = builder.Metadata;

            Assert.Equal(Order.DetailsField, metadata.FieldInfo);
            Assert.Equal(ConfigurationSource.Convention, metadata.GetFieldInfoConfigurationSource());

            Assert.True(builder.CanSetField(Order.DetailsField, ConfigurationSource.DataAnnotation));
            Assert.NotNull(builder.HasField(Order.DetailsField, ConfigurationSource.DataAnnotation));

            Assert.Equal(Order.DetailsField, metadata.FieldInfo);
            Assert.Equal(ConfigurationSource.DataAnnotation, metadata.GetFieldInfoConfigurationSource());

            Assert.True(builder.CanSetField(Order.DetailsField, ConfigurationSource.Convention));
            Assert.False(builder.CanSetField(Order.OtherDetailsField, ConfigurationSource.Convention));
            Assert.NotNull(builder.HasField(Order.DetailsField, ConfigurationSource.Convention));
            Assert.Null(builder.HasField(Order.OtherDetailsField, ConfigurationSource.Convention));

            Assert.Equal(Order.DetailsField, metadata.FieldInfo);
            Assert.Equal(ConfigurationSource.DataAnnotation, metadata.GetFieldInfoConfigurationSource());

            Assert.True(builder.CanSetField(Order.OtherDetailsField, ConfigurationSource.DataAnnotation));
            Assert.NotNull(builder.HasField(Order.OtherDetailsField, ConfigurationSource.DataAnnotation));

            Assert.Equal(Order.OtherDetailsField, metadata.FieldInfo);
            Assert.Equal(ConfigurationSource.DataAnnotation, metadata.GetFieldInfoConfigurationSource());

            Assert.True(builder.CanSetField((string)null, ConfigurationSource.DataAnnotation));
            Assert.NotNull(builder.HasField((string)null, ConfigurationSource.DataAnnotation));

            Assert.Null(metadata.FieldInfo);
            Assert.Null(metadata.GetFieldInfoConfigurationSource());
        }

        [ConditionalFact]
        public void Can_only_override_lower_or_equal_source_HasField_string()
        {
            var builder = CreateInternalNavigationBuilder();
            var metadata = builder.Metadata;

            Assert.NotNull(metadata.FieldInfo);
            Assert.Equal(ConfigurationSource.Convention, metadata.GetFieldInfoConfigurationSource());

            Assert.True(builder.CanSetField("_details", ConfigurationSource.DataAnnotation));
            Assert.NotNull(builder.HasField("_details", ConfigurationSource.DataAnnotation));

            Assert.Equal("_details", metadata.FieldInfo?.Name);
            Assert.Equal(ConfigurationSource.DataAnnotation, metadata.GetFieldInfoConfigurationSource());

            Assert.True(builder.CanSetField("_details", ConfigurationSource.Convention));
            Assert.False(builder.CanSetField("_otherDetails", ConfigurationSource.Convention));
            Assert.NotNull(builder.HasField("_details", ConfigurationSource.Convention));
            Assert.Null(builder.HasField("_otherDetails", ConfigurationSource.Convention));

            Assert.Equal("_details", metadata.FieldInfo?.Name);
            Assert.Equal(ConfigurationSource.DataAnnotation, metadata.GetFieldInfoConfigurationSource());

            Assert.True(builder.CanSetField("_otherDetails", ConfigurationSource.DataAnnotation));
            Assert.NotNull(builder.HasField("_otherDetails", ConfigurationSource.DataAnnotation));

            Assert.Equal("_otherDetails", metadata.FieldInfo?.Name);
            Assert.Equal(ConfigurationSource.DataAnnotation, metadata.GetFieldInfoConfigurationSource());

            Assert.True(builder.CanSetField((string)null, ConfigurationSource.DataAnnotation));
            Assert.NotNull(builder.HasField((string)null, ConfigurationSource.DataAnnotation));

            Assert.Null(metadata.FieldInfo);
            Assert.Null(metadata.GetFieldInfoConfigurationSource());
        }

        [ConditionalFact]
        public void Can_only_override_lower_or_equal_source_PropertyAccessMode()
        {
            var builder = CreateInternalNavigationBuilder();
            IConventionNavigation metadata = builder.Metadata;

            Assert.Equal(PropertyAccessMode.PreferField, metadata.GetPropertyAccessMode());
            Assert.Null(metadata.GetPropertyAccessModeConfigurationSource());

            Assert.True(builder.CanSetPropertyAccessMode(PropertyAccessMode.PreferProperty, ConfigurationSource.DataAnnotation));
            Assert.NotNull(builder.UsePropertyAccessMode(PropertyAccessMode.PreferProperty, ConfigurationSource.DataAnnotation));

            Assert.Equal(PropertyAccessMode.PreferProperty, metadata.GetPropertyAccessMode());
            Assert.Equal(ConfigurationSource.DataAnnotation, metadata.GetPropertyAccessModeConfigurationSource());

            Assert.True(builder.CanSetPropertyAccessMode(PropertyAccessMode.PreferProperty, ConfigurationSource.Convention));
            Assert.False(
                builder.CanSetPropertyAccessMode(PropertyAccessMode.PreferFieldDuringConstruction, ConfigurationSource.Convention));
            Assert.NotNull(builder.UsePropertyAccessMode(PropertyAccessMode.PreferProperty, ConfigurationSource.Convention));
            Assert.Null(builder.UsePropertyAccessMode(PropertyAccessMode.PreferFieldDuringConstruction, ConfigurationSource.Convention));

            Assert.Equal(PropertyAccessMode.PreferProperty, metadata.GetPropertyAccessMode());
            Assert.Equal(ConfigurationSource.DataAnnotation, metadata.GetPropertyAccessModeConfigurationSource());

            Assert.True(
                builder.CanSetPropertyAccessMode(PropertyAccessMode.PreferFieldDuringConstruction, ConfigurationSource.DataAnnotation));
            Assert.NotNull(
                builder.UsePropertyAccessMode(PropertyAccessMode.PreferFieldDuringConstruction, ConfigurationSource.DataAnnotation));

            Assert.Equal(PropertyAccessMode.PreferFieldDuringConstruction, metadata.GetPropertyAccessMode());
            Assert.Equal(ConfigurationSource.DataAnnotation, metadata.GetPropertyAccessModeConfigurationSource());

            Assert.True(builder.CanSetPropertyAccessMode(null, ConfigurationSource.DataAnnotation));
            Assert.NotNull(builder.UsePropertyAccessMode(null, ConfigurationSource.DataAnnotation));

            Assert.Equal(PropertyAccessMode.PreferField, metadata.GetPropertyAccessMode());
            Assert.Null(metadata.GetPropertyAccessModeConfigurationSource());
        }

        [ConditionalFact]
        public void Can_only_override_lower_or_equal_source_IsEagerLoaded()
        {
            var builder = CreateInternalNavigationBuilder();
            IConventionNavigation metadata = builder.Metadata;

            Assert.False(metadata.IsEagerLoaded);
            Assert.Null(metadata.GetIsEagerLoadedConfigurationSource());

            Assert.True(builder.CanSetAutoInclude(autoInclude: true, ConfigurationSource.DataAnnotation));
            Assert.NotNull(builder.AutoInclude(autoInclude: true, ConfigurationSource.DataAnnotation));

            Assert.True(metadata.IsEagerLoaded);
            Assert.Equal(ConfigurationSource.DataAnnotation, metadata.GetIsEagerLoadedConfigurationSource());

            Assert.True(builder.CanSetAutoInclude(autoInclude: true, ConfigurationSource.Convention));
            Assert.False(builder.CanSetAutoInclude(autoInclude: false, ConfigurationSource.Convention));
            Assert.NotNull(builder.AutoInclude(autoInclude: true, ConfigurationSource.Convention));
            Assert.Null(builder.AutoInclude(autoInclude: false, ConfigurationSource.Convention));

            Assert.True(metadata.IsEagerLoaded);
            Assert.Equal(ConfigurationSource.DataAnnotation, metadata.GetIsEagerLoadedConfigurationSource());

            Assert.True(builder.CanSetAutoInclude(autoInclude: false, ConfigurationSource.DataAnnotation));
            Assert.NotNull(builder.AutoInclude(autoInclude: false, ConfigurationSource.DataAnnotation));

            Assert.False(metadata.IsEagerLoaded);
            Assert.Equal(ConfigurationSource.DataAnnotation, metadata.GetIsEagerLoadedConfigurationSource());

            Assert.True(builder.CanSetAutoInclude(null, ConfigurationSource.DataAnnotation));
            Assert.NotNull(builder.AutoInclude(null, ConfigurationSource.DataAnnotation));

            Assert.False(metadata.IsEagerLoaded);
            Assert.Null(metadata.GetIsEagerLoadedConfigurationSource());
        }

        [ConditionalFact]
        public void Configuring_IsRequired_on_to_dependent_nonUnique_throws()
        {
            var builder = CreateInternalNavigationBuilder();

            Assert.Equal(
                CoreStrings.NonUniqueRequiredDependentNavigation(nameof(Order), nameof(Order.Details)),
                Assert.Throws<InvalidOperationException>(() => builder.IsRequired(true, ConfigurationSource.Explicit)).Message);
        }

        [ConditionalFact]
        public void Can_configure_IsRequired_on_to_principal_nonUnique()
        {
            var builder = CreateInternalNavigationBuilder()
                .Metadata.ForeignKey.Builder.HasNavigation(
                    nameof(OrderDetails.Order), pointsToPrincipal: true, ConfigurationSource.Explicit)
                .Metadata.DependentToPrincipal.Builder;
            builder.IsRequired(true, ConfigurationSource.Explicit);

            Assert.True(builder.Metadata.ForeignKey.IsRequired);
        }

        [ConditionalFact]
        public void Can_configure_IsRequired_on_to_dependent_unique()
        {
            var foreignKey = CreateInternalNavigationBuilder()
                .Metadata.ForeignKey;
            foreignKey = foreignKey.Builder.HasNavigations(
                    nameof(OrderDetails.Order), nameof(Order.SingleDetails), ConfigurationSource.Explicit)
                .Metadata;

            foreignKey.PrincipalToDependent.Builder.IsRequired(true, ConfigurationSource.Explicit);

            Assert.True(foreignKey.IsRequiredDependent);
        }

        [ConditionalFact]
        public void Can_configure_IsRequired_on_to_principal_unique()
        {
            var foreignKey = CreateInternalNavigationBuilder()
                .Metadata.ForeignKey;
            foreignKey = foreignKey.Builder.HasNavigations(
                    nameof(OrderDetails.Order), nameof(Order.SingleDetails), ConfigurationSource.Explicit)
                .Metadata;

            foreignKey.PrincipalToDependent.Builder.IsRequired(true, ConfigurationSource.Explicit);

            Assert.True(foreignKey.IsRequiredDependent);
        }

        private InternalNavigationBuilder CreateInternalNavigationBuilder()
        {
            var modelBuilder = (InternalModelBuilder)
                InMemoryTestHelpers.Instance.CreateConventionBuilder().GetInfrastructure();
            var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Convention);
            var detailsEntityBuilder = modelBuilder.Entity(typeof(OrderDetails), ConfigurationSource.Convention);
            orderEntityBuilder
                .HasRelationship(
                    detailsEntityBuilder.Metadata, nameof(Order.Details), ConfigurationSource.DataAnnotation, targetIsPrincipal: false)
                .IsUnique(false, ConfigurationSource.Convention);
            var navigation = (Navigation)orderEntityBuilder.Navigation(nameof(Order.Details));

            return new InternalNavigationBuilder(navigation, modelBuilder);
        }

        protected class Order
        {
            public static readonly FieldInfo DetailsField = typeof(Order)
                .GetField(nameof(_details), BindingFlags.Instance | BindingFlags.NonPublic);

            public static readonly FieldInfo OtherDetailsField = typeof(Order)
                .GetField(nameof(_otherDetails), BindingFlags.Instance | BindingFlags.NonPublic);

            public int OrderId { get; set; }

            private ICollection<OrderDetails> _details;
            private readonly ICollection<OrderDetails> _otherDetails = new List<OrderDetails>();
            public OrderDetails SingleDetails { get; set; }
            public ICollection<OrderDetails> Details { get => _details; set => _details = value; }
        }

        protected class OrderDetails
        {
            public int Id { get; set; }
            public Order Order { get; set; }
        }
    }
}
