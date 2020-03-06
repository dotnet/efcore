// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.ModelBuilding
{
    public class NavigationBuilderTest
    {
        [ConditionalFact]
        public virtual void Navigation_properties_can_have_access_mode_set()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;

            modelBuilder.Entity<NavPrincipal>()
                .HasMany(
                    e => e.Dependents,
                    nb => nb.UsePropertyAccessMode(PropertyAccessMode.Field))
                .WithOne(
                    e => e.Principal,
                    nb => nb.UsePropertyAccessMode(PropertyAccessMode.Property));

            var principal = (IEntityType)model.FindEntityType(typeof(NavPrincipal));
            var dependent = (IEntityType)model.FindEntityType(typeof(NavDependent));

            Assert.Equal(PropertyAccessMode.Field, principal.FindNavigation("Dependents").GetPropertyAccessMode());
            Assert.Equal(PropertyAccessMode.Property, dependent.FindNavigation("Principal").GetPropertyAccessMode());
        }
        protected virtual ModelBuilder CreateModelBuilder()
            => InMemoryTestHelpers.Instance.CreateConventionBuilder();

        private class NavPrincipal
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public List<NavDependent> Dependents { get; set; }
        }

        private class NavDependent
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public NavPrincipal Principal { get; set; }
        }
    }
}
