// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public abstract class PropertyValuesTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : PropertyValuesTestBase<TFixture>.PropertyValuesFixtureBase, new()
    {
        protected PropertyValuesTestBase(TFixture fixture)
        {
            Fixture = fixture;
        }

        protected TFixture Fixture { get; }

        [Fact]
        public virtual Task Scalar_current_values_can_be_accessed_as_a_property_dictionary()
        {
            return TestPropertyValuesScalars(e => Task.FromResult(e.CurrentValues), expectOriginalValues: false);
        }

        [Fact]
        public virtual Task Scalar_original_values_can_be_accessed_as_a_property_dictionary()
        {
            return TestPropertyValuesScalars(e => Task.FromResult(e.OriginalValues), expectOriginalValues: true);
        }

        [Fact]
        public virtual Task Scalar_store_values_can_be_accessed_as_a_property_dictionary()
        {
            return TestPropertyValuesScalars(e => Task.FromResult(e.GetDatabaseValues()), expectOriginalValues: true);
        }

        [Fact]
        public virtual Task Scalar_store_values_can_be_accessed_asynchronously_as_a_property_dictionary()
        {
            return TestPropertyValuesScalars(e => e.GetDatabaseValuesAsync(), expectOriginalValues: true);
        }

        private async Task TestPropertyValuesScalars(
            Func<EntityEntry<Building>, Task<PropertyValues>> getPropertyValues,
            bool expectOriginalValues)
        {
            using (var context = CreateContext())
            {
                var building = context.Set<Building>().Single(b => b.Name == "Building One");

                building.Name = "Building One Prime";
                building.Value = 1500001m;
                context.Entry(building).Property("Shadow1").CurrentValue = 12;
                context.Entry(building).Property("Shadow2").CurrentValue = "Pine Walk";

                var values = await getPropertyValues(context.Entry(building));

                if (expectOriginalValues)
                {
                    Assert.Equal("Building One", values["Name"]);
                    Assert.Equal(1500000m, values["Value"]);
                    Assert.Equal(11, values["Shadow1"]);
                    Assert.Equal("Meadow Drive", values["Shadow2"]);
                }
                else
                {
                    Assert.Equal("Building One Prime", values["Name"]);
                    Assert.Equal(1500001m, values["Value"]);
                    Assert.Equal(12, values["Shadow1"]);
                    Assert.Equal("Pine Walk", values["Shadow2"]);
                }
            }
        }

        [Fact]
        public virtual Task Scalar_current_values_can_be_accessed_as_a_property_dictionary_using_IProperty()
        {
            return TestPropertyValuesScalarsIProperty(e => Task.FromResult(e.CurrentValues), expectOriginalValues: false);
        }

        [Fact]
        public virtual Task Scalar_original_values_can_be_accessed_as_a_property_dictionary_using_IProperty()
        {
            return TestPropertyValuesScalarsIProperty(e => Task.FromResult(e.OriginalValues), expectOriginalValues: true);
        }

        [Fact]
        public virtual Task Scalar_store_values_can_be_accessed_as_a_property_dictionary_using_IProperty()
        {
            return TestPropertyValuesScalarsIProperty(e => Task.FromResult(e.GetDatabaseValues()), expectOriginalValues: true);
        }

        [Fact]
        public virtual Task Scalar_store_values_can_be_accessed_asynchronously_as_a_property_dictionary_using_IProperty()
        {
            return TestPropertyValuesScalarsIProperty(e => e.GetDatabaseValuesAsync(), expectOriginalValues: true);
        }

        private async Task TestPropertyValuesScalarsIProperty(
            Func<EntityEntry<Building>, Task<PropertyValues>> getPropertyValues,
            bool expectOriginalValues)
        {
            using (var context = CreateContext())
            {
                var building = context.Set<Building>().Single(b => b.Name == "Building One");

                building.Name = "Building One Prime";
                building.Value = 1500001m;
                context.Entry(building).Property("Shadow1").CurrentValue = 12;
                context.Entry(building).Property("Shadow2").CurrentValue = "Pine Walk";

                var entry = context.Entry(building);
                var values = await getPropertyValues(entry);

                if (expectOriginalValues)
                {
                    Assert.Equal("Building One", values[entry.Property(e => e.Name).Metadata]);
                    Assert.Equal(1500000m, values[entry.Property(e => e.Value).Metadata]);
                    Assert.Equal(11, values[entry.Property("Shadow1").Metadata]);
                    Assert.Equal("Meadow Drive", values[entry.Property("Shadow2").Metadata]);
                }
                else
                {
                    Assert.Equal("Building One Prime", values[entry.Property(e => e.Name).Metadata]);
                    Assert.Equal(1500001m, values[entry.Property(e => e.Value).Metadata]);
                    Assert.Equal(12, values[entry.Property("Shadow1").Metadata]);
                    Assert.Equal("Pine Walk", values[entry.Property("Shadow2").Metadata]);
                }
            }
        }

        [Fact]
        public virtual Task Scalar_current_values_of_a_derived_object_can_be_accessed_as_a_property_dictionary()
        {
            return TestPropertyValuesDerivedScalars(e => Task.FromResult(e.CurrentValues), expectOriginalValues: false);
        }

        [Fact]
        public virtual Task Scalar_original_values_of_a_derived_object_can_be_accessed_as_a_property_dictionary()
        {
            return TestPropertyValuesDerivedScalars(e => Task.FromResult(e.OriginalValues), expectOriginalValues: true);
        }

        [Fact]
        public virtual Task Scalar_store_values_of_a_derived_object_can_be_accessed_as_a_property_dictionary()
        {
            return TestPropertyValuesDerivedScalars(e => Task.FromResult(e.GetDatabaseValues()), expectOriginalValues: true);
        }

        [Fact]
        public virtual Task Scalar_store_values_of_a_derived_object_can_be_accessed_asynchronously_as_a_property_dictionary()
        {
            return TestPropertyValuesDerivedScalars(e => e.GetDatabaseValuesAsync(), expectOriginalValues: true);
        }

        private async Task TestPropertyValuesDerivedScalars(
            Func<EntityEntry<CurrentEmployee>, Task<PropertyValues>> getPropertyValues,
            bool expectOriginalValues)
        {
            using (var context = CreateContext())
            {
                var employee = context.Set<Employee>().OfType<CurrentEmployee>().Single(b => b.FirstName == "Rowan");

                employee.LastName = "Milner";
                employee.LeaveBalance = 55m;
                context.Entry(employee).Property("Shadow1").CurrentValue = 222;
                context.Entry(employee).Property("Shadow2").CurrentValue = "Dev";
                context.Entry(employee).Property("Shadow3").CurrentValue = 2222;

                var values = await getPropertyValues(context.Entry(employee));

                if (expectOriginalValues)
                {
                    Assert.Equal("Miller", values["LastName"]);
                    Assert.Equal(45m, values["LeaveBalance"]);
                    Assert.Equal(111, values["Shadow1"]);
                    Assert.Equal("PM", values["Shadow2"]);
                    Assert.Equal(1111, values["Shadow3"]);
                }
                else
                {
                    Assert.Equal("Milner", values["LastName"]);
                    Assert.Equal(55m, values["LeaveBalance"]);
                    Assert.Equal(222, values["Shadow1"]);
                    Assert.Equal("Dev", values["Shadow2"]);
                    Assert.Equal(2222, values["Shadow3"]);
                }
            }
        }

        [Fact]
        public virtual Task Scalar_current_values_can_be_accessed_as_a_non_generic_property_dictionary()
        {
            return TestNonGenericPropertyValuesScalars(e => Task.FromResult(e.CurrentValues), expectOriginalValues: false);
        }

        [Fact]
        public virtual Task Scalar_original_values_can_be_accessed_as_a_non_generic_property_dictionary()
        {
            return TestNonGenericPropertyValuesScalars(e => Task.FromResult(e.OriginalValues), expectOriginalValues: true);
        }

        [Fact]
        public virtual Task Scalar_store_values_can_be_accessed_as_a_non_generic_property_dictionary()
        {
            return TestNonGenericPropertyValuesScalars(e => Task.FromResult(e.GetDatabaseValues()), expectOriginalValues: true);
        }

        [Fact]
        public virtual Task Scalar_store_values_can_be_accessed_asynchronously_as_a_non_generic_property_dictionary()
        {
            return TestNonGenericPropertyValuesScalars(e => e.GetDatabaseValuesAsync(), expectOriginalValues: true);
        }

        private async Task TestNonGenericPropertyValuesScalars(
            Func<EntityEntry, Task<PropertyValues>> getPropertyValues,
            bool expectOriginalValues)
        {
            using (var context = CreateContext())
            {
                object building = context.Set<Building>().Single(b => b.Name == "Building One");

                context.Entry(building).Property("Name").CurrentValue = "Building One Prime";
                context.Entry(building).Property("Value").CurrentValue = 1500001m;
                context.Entry(building).Property("Shadow1").CurrentValue = 12;
                context.Entry(building).Property("Shadow2").CurrentValue = "Pine Walk";

                var values = await getPropertyValues(context.Entry(building));

                if (expectOriginalValues)
                {
                    Assert.Equal("Building One", values["Name"]);
                    Assert.Equal(1500000m, values["Value"]);
                    Assert.Equal(11, values["Shadow1"]);
                    Assert.Equal("Meadow Drive", values["Shadow2"]);

                    Assert.Equal("Building One", values.GetValue<string>("Name"));
                    Assert.Equal(1500000m, values.GetValue<decimal>("Value"));
                    Assert.Equal(11, values.GetValue<int>("Shadow1"));
                    Assert.Equal("Meadow Drive", values.GetValue<string>("Shadow2"));
                }
                else
                {
                    Assert.Equal("Building One Prime", values["Name"]);
                    Assert.Equal(1500001m, values["Value"]);
                    Assert.Equal(12, values["Shadow1"]);
                    Assert.Equal("Pine Walk", values["Shadow2"]);

                    Assert.Equal("Building One Prime", values.GetValue<string>("Name"));
                    Assert.Equal(1500001m, values.GetValue<decimal>("Value"));
                    Assert.Equal(12, values.GetValue<int>("Shadow1"));
                    Assert.Equal("Pine Walk", values.GetValue<string>("Shadow2"));
                }
            }
        }

        [Fact]
        public virtual Task Scalar_current_values_can_be_accessed_as_a_non_generic_property_dictionary_using_IProperty()
        {
            return TestNonGenericPropertyValuesScalarsIProperty(e => Task.FromResult(e.CurrentValues), expectOriginalValues: false);
        }

        [Fact]
        public virtual Task Scalar_original_values_can_be_accessed_as_a_non_generic_property_dictionary_using_IProperty()
        {
            return TestNonGenericPropertyValuesScalarsIProperty(e => Task.FromResult(e.OriginalValues), expectOriginalValues: true);
        }

        [Fact]
        public virtual Task Scalar_store_values_can_be_accessed_as_a_non_generic_property_dictionary_using_IProperty()
        {
            return TestNonGenericPropertyValuesScalarsIProperty(e => Task.FromResult(e.GetDatabaseValues()), expectOriginalValues: true);
        }

        [Fact]
        public virtual Task Scalar_store_values_can_be_accessed_asynchronously_as_a_non_generic_property_dictionary_using_IProperty()
        {
            return TestNonGenericPropertyValuesScalarsIProperty(e => e.GetDatabaseValuesAsync(), expectOriginalValues: true);
        }

        private async Task TestNonGenericPropertyValuesScalarsIProperty(
            Func<EntityEntry, Task<PropertyValues>> getPropertyValues,
            bool expectOriginalValues)
        {
            using (var context = CreateContext())
            {
                object building = context.Set<Building>().Single(b => b.Name == "Building One");

                context.Entry(building).Property("Name").CurrentValue = "Building One Prime";
                context.Entry(building).Property("Value").CurrentValue = 1500001m;
                context.Entry(building).Property("Shadow1").CurrentValue = 12;
                context.Entry(building).Property("Shadow2").CurrentValue = "Pine Walk";

                var entry = context.Entry(building);
                var values = await getPropertyValues(entry);

                if (expectOriginalValues)
                {
                    Assert.Equal("Building One", values["Name"]);
                    Assert.Equal(1500000m, values["Value"]);

                    Assert.Equal("Building One", values.GetValue<string>(entry.Property("Name").Metadata));
                    Assert.Equal(1500000m, values.GetValue<decimal>(entry.Property("Value").Metadata));
                    Assert.Equal(11, values.GetValue<int>(entry.Property("Shadow1").Metadata));
                    Assert.Equal("Meadow Drive", values.GetValue<string>(entry.Property("Shadow2").Metadata));
                }
                else
                {
                    Assert.Equal("Building One Prime", values["Name"]);
                    Assert.Equal(1500001m, values["Value"]);

                    Assert.Equal("Building One Prime", values.GetValue<string>(entry.Property("Name").Metadata));
                    Assert.Equal(1500001m, values.GetValue<decimal>(entry.Property("Value").Metadata));
                    Assert.Equal(12, values.GetValue<int>(entry.Property("Shadow1").Metadata));
                    Assert.Equal("Pine Walk", values.GetValue<string>(entry.Property("Shadow2").Metadata));
                }
            }
        }

        [Fact]
        public virtual Task Scalar_current_values_of_a_derived_object_can_be_accessed_as_a_non_generic_property_dictionary()
        {
            return TestNonGenericPropertyValuesDerivedScalars(e => Task.FromResult(e.CurrentValues), expectOriginalValues: false);
        }

        [Fact]
        public virtual Task Scalar_original_values_of_a_derived_object_can_be_accessed_as_a_non_generic_property_dictionary()
        {
            return TestNonGenericPropertyValuesDerivedScalars(e => Task.FromResult(e.OriginalValues), expectOriginalValues: true);
        }

        [Fact]
        public virtual Task Scalar_store_values_of_a_derived_object_can_be_accessed_as_a_non_generic_property_dictionary()
        {
            return TestNonGenericPropertyValuesDerivedScalars(e => Task.FromResult(e.GetDatabaseValues()), expectOriginalValues: true);
        }

        [Fact]
        public virtual Task Scalar_store_values_of_a_derived_object_can_be_accessed_asynchronously_as_a_non_generic_property_dictionary()
        {
            return TestNonGenericPropertyValuesDerivedScalars(e => e.GetDatabaseValuesAsync(), expectOriginalValues: true);
        }

        private async Task TestNonGenericPropertyValuesDerivedScalars(
            Func<EntityEntry, Task<PropertyValues>> getPropertyValues,
            bool expectOriginalValues)
        {
            using (var context = CreateContext())
            {
                object employee = context.Set<Employee>().OfType<CurrentEmployee>().Single(b => b.FirstName == "Rowan");

                ((CurrentEmployee)employee).LastName = "Milner";
                ((CurrentEmployee)employee).LeaveBalance = 55m;
                context.Entry(employee).Property("Shadow1").CurrentValue = 222;
                context.Entry(employee).Property("Shadow2").CurrentValue = "Dev";
                context.Entry(employee).Property("Shadow3").CurrentValue = 2222;

                var values = await getPropertyValues(context.Entry(employee));

                if (expectOriginalValues)
                {
                    Assert.Equal("Miller", values["LastName"]);
                    Assert.Equal(45m, values["LeaveBalance"]);
                    Assert.Equal(111, values["Shadow1"]);
                    Assert.Equal("PM", values["Shadow2"]);
                    Assert.Equal(1111, values["Shadow3"]);
                }
                else
                {
                    Assert.Equal("Milner", values["LastName"]);
                    Assert.Equal(55m, values["LeaveBalance"]);
                    Assert.Equal(222, values["Shadow1"]);
                    Assert.Equal("Dev", values["Shadow2"]);
                    Assert.Equal(2222, values["Shadow3"]);
                }
            }
        }

        [Fact]
        public virtual void Scalar_current_values_can_be_set_using_a_property_dictionary()
        {
            TestSetPropertyValuesScalars(e => e.CurrentValues, (e, n) => e.Property(n).CurrentValue);
        }

        [Fact]
        public virtual void Scalar_original_values_can_be_set_using_a_property_dictionary()
        {
            TestSetPropertyValuesScalars(e => e.OriginalValues, (e, n) => e.Property(n).OriginalValue);
        }

        private void TestSetPropertyValuesScalars(
            Func<EntityEntry<Building>, PropertyValues> getPropertyValues,
            Func<EntityEntry, string, object> getValue)
        {
            using (var context = CreateContext())
            {
                var building = context.Set<Building>().Single(b => b.Name == "Building One");
                var values = getPropertyValues(context.Entry(building));

                values["Name"] = "Building 18";
                values["Value"] = -1000m;
                values["Shadow1"] = 13;
                values["Shadow2"] = "Pine Walk";

                Assert.Equal("Building 18", values["Name"]);
                Assert.Equal(-1000m, values["Value"]);
                Assert.Equal(13, values["Shadow1"]);
                Assert.Equal("Pine Walk", values["Shadow2"]);

                var entry = context.Entry(building);
                Assert.Equal("Building 18", getValue(entry, "Name"));
                Assert.Equal(-1000m, getValue(entry, "Value"));
                Assert.Equal(13, getValue(entry, "Shadow1"));
                Assert.Equal("Pine Walk", getValue(entry, "Shadow2"));
            }
        }

        [Fact]
        public virtual void Scalar_current_values_can_be_set_using_a_property_dictionary_with_IProperty()
        {
            TestSetPropertyValuesScalarsIProperty(e => e.CurrentValues, (e, n) => e.Property(n).CurrentValue);
        }

        [Fact]
        public virtual void Scalar_original_values_can_be_set_using_a_property_dictionary_with_IProperty()
        {
            TestSetPropertyValuesScalarsIProperty(e => e.OriginalValues, (e, n) => e.Property(n).OriginalValue);
        }

        private void TestSetPropertyValuesScalarsIProperty(
            Func<EntityEntry<Building>, PropertyValues> getPropertyValues,
            Func<EntityEntry, string, object> getValue)
        {
            using (var context = CreateContext())
            {
                var building = context.Set<Building>().Single(b => b.Name == "Building One");
                var entry = context.Entry(building);
                var values = getPropertyValues(entry);

                values[entry.Property(e => e.Name).Metadata] = "Building 18";
                values[entry.Property(e => e.Value).Metadata] = -1000m;
                values[entry.Property("Shadow1").Metadata] = 13;
                values[entry.Property("Shadow2").Metadata] = "Pine Walk";

                Assert.Equal("Building 18", values["Name"]);
                Assert.Equal(-1000m, values["Value"]);
                Assert.Equal(13, values["Shadow1"]);
                Assert.Equal("Pine Walk", values["Shadow2"]);

                Assert.Equal("Building 18", getValue(entry, "Name"));
                Assert.Equal(-1000m, getValue(entry, "Value"));
                Assert.Equal(13, getValue(entry, "Shadow1"));
                Assert.Equal("Pine Walk", getValue(entry, "Shadow2"));
            }
        }

        [Fact]
        public virtual void Scalar_current_values_can_be_set_using_a_non_generic_property_dictionary()
        {
            TestSetNonGenericPropertyValuesScalars(e => e.CurrentValues, (e, n) => e.Property(n).CurrentValue);
        }

        [Fact]
        public virtual void Scalar_original_values_can_be_set_using_a_non_generic_property_dictionary()
        {
            TestSetNonGenericPropertyValuesScalars(e => e.OriginalValues, (e, n) => e.Property(n).OriginalValue);
        }

        private void TestSetNonGenericPropertyValuesScalars(
            Func<EntityEntry, PropertyValues> getPropertyValues,
            Func<EntityEntry, string, object> getValue)
        {
            using (var context = CreateContext())
            {
                var building = context.Set<Building>().Single(b => b.Name == "Building One");
                var values = getPropertyValues(context.Entry(building));

                values["Name"] = "Building 18";
                values["Value"] = -1000m;
                values["Shadow1"] = 13;
                values["Shadow2"] = "Pine Walk";

                Assert.Equal("Building 18", values["Name"]);
                Assert.Equal(-1000m, values["Value"]);
                Assert.Equal(13, values["Shadow1"]);
                Assert.Equal("Pine Walk", values["Shadow2"]);

                var entry = context.Entry(building);
                Assert.Equal("Building 18", getValue(entry, "Name"));
                Assert.Equal(-1000m, getValue(entry, "Value"));
                Assert.Equal(13, getValue(entry, "Shadow1"));
                Assert.Equal("Pine Walk", getValue(entry, "Shadow2"));
            }
        }

        [Fact]
        public virtual Task Current_values_can_be_copied_into_an_object()
        {
            return TestPropertyValuesClone(e => Task.FromResult(e.CurrentValues), expectOriginalValues: false);
        }

        [Fact]
        public virtual Task Original_values_can_be_copied_into_an_object()
        {
            return TestPropertyValuesClone(e => Task.FromResult(e.OriginalValues), expectOriginalValues: true);
        }

        [Fact]
        public virtual Task Store_values_can_be_copied_into_an_object()
        {
            return TestPropertyValuesClone(e => Task.FromResult(e.GetDatabaseValues()), expectOriginalValues: true);
        }

        [Fact]
        public virtual Task Store_values_can_be_copied_into_an_object_asynchronously()
        {
            return TestPropertyValuesClone(e => e.GetDatabaseValuesAsync(), expectOriginalValues: true);
        }

        private async Task TestPropertyValuesClone(
            Func<EntityEntry<Building>, Task<PropertyValues>> getPropertyValues,
            bool expectOriginalValues)
        {
            using (var context = CreateContext())
            {
                var building = context.Set<Building>().Single(b => b.Name == "Building One");

                building.Name = "Building One Prime";
                building.Value = 1500001m;
                context.Entry(building).Property("Shadow1").CurrentValue = 12;
                context.Entry(building).Property("Shadow2").CurrentValue = "Pine Walk";

                var buildingClone = (Building)(await getPropertyValues(context.Entry(building))).ToObject();

                if (expectOriginalValues)
                {
                    Assert.Equal("Building One", buildingClone.Name);
                    Assert.Equal(1500000m, buildingClone.Value);
                }
                else
                {
                    Assert.Equal("Building One Prime", buildingClone.Name);
                    Assert.Equal(1500001m, buildingClone.Value);
                }
            }
        }

        [Fact]
        public virtual Task Current_values_for_derived_object_can_be_copied_into_an_object()
        {
            return TestPropertyValuesDerivedClone(e => Task.FromResult(e.CurrentValues), expectOriginalValues: false);
        }

        [Fact]
        public virtual Task Original_values_for_derived_object_can_be_copied_into_an_object()
        {
            return TestPropertyValuesDerivedClone(e => Task.FromResult(e.OriginalValues), expectOriginalValues: true);
        }

        [Fact]
        public virtual Task Store_values_for_derived_object_can_be_copied_into_an_object()
        {
            return TestPropertyValuesDerivedClone(e => Task.FromResult(e.GetDatabaseValues()), expectOriginalValues: true);
        }

        [Fact]
        public virtual Task Store_values_for_derived_object_can_be_copied_into_an_object_asynchronously()
        {
            return TestPropertyValuesDerivedClone(e => e.GetDatabaseValuesAsync(), expectOriginalValues: true);
        }

        private async Task TestPropertyValuesDerivedClone(
            Func<EntityEntry<CurrentEmployee>, Task<PropertyValues>> getPropertyValues,
            bool expectOriginalValues)
        {
            using (var context = CreateContext())
            {
                var employee = context.Set<Employee>().OfType<CurrentEmployee>().Single(b => b.FirstName == "Rowan");

                employee.LastName = "Milner";
                employee.LeaveBalance = 55m;
                context.Entry(employee).Property("Shadow1").CurrentValue = 222;
                context.Entry(employee).Property("Shadow2").CurrentValue = "Dev";
                context.Entry(employee).Property("Shadow3").CurrentValue = 2222;

                var clone = (CurrentEmployee)(await getPropertyValues(context.Entry(employee))).ToObject();

                if (expectOriginalValues)
                {
                    Assert.Equal("Rowan", clone.FirstName);
                    Assert.Equal("Miller", clone.LastName);
                    Assert.Equal(45m, clone.LeaveBalance);
                }
                else
                {
                    Assert.Equal("Rowan", clone.FirstName);
                    Assert.Equal("Milner", clone.LastName);
                    Assert.Equal(55m, clone.LeaveBalance);
                }
            }
        }

        [Fact]
        public virtual Task Current_values_can_be_copied_from_a_non_generic_property_dictionary_into_an_object()
        {
            return TestNonGenericPropertyValuesClone(e => Task.FromResult(e.CurrentValues), expectOriginalValues: false);
        }

        [Fact]
        public virtual Task Original_values_can_be_copied_non_generic_property_dictionary_into_an_object()
        {
            return TestNonGenericPropertyValuesClone(e => Task.FromResult(e.OriginalValues), expectOriginalValues: true);
        }

        [Fact]
        public virtual Task Store_values_can_be_copied_non_generic_property_dictionary_into_an_object()
        {
            return TestNonGenericPropertyValuesClone(e => Task.FromResult(e.GetDatabaseValues()), expectOriginalValues: true);
        }

        [Fact]
        public virtual Task Store_values_can_be_copied_asynchronously_non_generic_property_dictionary_into_an_object()
        {
            return TestNonGenericPropertyValuesClone(e => e.GetDatabaseValuesAsync(), expectOriginalValues: true);
        }

        private async Task TestNonGenericPropertyValuesClone(
            Func<EntityEntry, Task<PropertyValues>> getPropertyValues,
            bool expectOriginalValues)
        {
            using (var context = CreateContext())
            {
                object building = context.Set<Building>().Single(b => b.Name == "Building One");

                ((Building)building).Name = "Building One Prime";
                ((Building)building).Value = 1500001m;
                context.Entry(building).Property("Shadow1").CurrentValue = 12;
                context.Entry(building).Property("Shadow2").CurrentValue = "Pine Walk";

                var buildingClone = (Building)(await getPropertyValues(context.Entry(building))).ToObject();

                if (expectOriginalValues)
                {
                    Assert.Equal("Building One", buildingClone.Name);
                    Assert.Equal(1500000m, buildingClone.Value);
                }
                else
                {
                    Assert.Equal("Building One Prime", buildingClone.Name);
                    Assert.Equal(1500001m, buildingClone.Value);
                }
            }
        }

        [Fact]
        public virtual Task Current_values_can_be_copied_into_a_cloned_dictionary()
        {
            return TestPropertyValuesCloneToValues(e => Task.FromResult(e.CurrentValues), expectOriginalValues: false);
        }

        [Fact]
        public virtual Task Original_values_can_be_copied_into_a_cloned_dictionary()
        {
            return TestPropertyValuesCloneToValues(e => Task.FromResult(e.OriginalValues), expectOriginalValues: true);
        }

        [Fact]
        public virtual Task Store_values_can_be_copied_into_a_cloned_dictionary()
        {
            return TestPropertyValuesCloneToValues(e => Task.FromResult(e.GetDatabaseValues()), expectOriginalValues: true);
        }

        [Fact]
        public virtual Task Store_values_can_be_copied_into_a_cloned_dictionary_asynchronously()
        {
            return TestPropertyValuesCloneToValues(e => e.GetDatabaseValuesAsync(), expectOriginalValues: true);
        }

        private async Task TestPropertyValuesCloneToValues(
            Func<EntityEntry<Building>, Task<PropertyValues>> getPropertyValues,
            bool expectOriginalValues)
        {
            using (var context = CreateContext())
            {
                var building = context.Set<Building>().Single(b => b.Name == "Building One");

                building.Name = "Building One Prime";
                building.Value = 1500001m;
                context.Entry(building).Property("Shadow1").CurrentValue = 12;
                context.Entry(building).Property("Shadow2").CurrentValue = "The Avenue";

                var buildingValues = await getPropertyValues(context.Entry(building));
                var clonedBuildingValues = buildingValues.Clone();

                if (expectOriginalValues)
                {
                    Assert.Equal("Building One", clonedBuildingValues["Name"]);
                    Assert.Equal(1500000m, clonedBuildingValues["Value"]);
                    Assert.Equal(11, clonedBuildingValues["Shadow1"]);
                    Assert.Equal("Meadow Drive", clonedBuildingValues["Shadow2"]);
                }
                else
                {
                    Assert.Equal("Building One Prime", clonedBuildingValues["Name"]);
                    Assert.Equal(1500001m, clonedBuildingValues["Value"]);
                    Assert.Equal(12, clonedBuildingValues["Shadow1"]);
                    Assert.Equal("The Avenue", clonedBuildingValues["Shadow2"]);
                }

                // Test modification of cloned property values does not impact original property values

                var newKey = new Guid();
                clonedBuildingValues["BuildingId"] = newKey; // Can change primary key on clone
                clonedBuildingValues["Name"] = "Building 18";
                clonedBuildingValues["Shadow1"] = 13;
                clonedBuildingValues["Shadow2"] = "Pine Walk";

                if (expectOriginalValues)
                {
                    Assert.Equal(newKey, clonedBuildingValues["BuildingId"]);
                    Assert.Equal("Building 18", clonedBuildingValues["Name"]);
                    Assert.Equal(13, clonedBuildingValues["Shadow1"]);
                    Assert.Equal("Pine Walk", clonedBuildingValues["Shadow2"]);

                    Assert.Equal("Building One", buildingValues["Name"]);
                    Assert.Equal(11, buildingValues["Shadow1"]);
                    Assert.Equal("Meadow Drive", buildingValues["Shadow2"]);
                }
                else
                {
                    Assert.Equal(newKey, clonedBuildingValues["BuildingId"]);
                    Assert.Equal("Building 18", clonedBuildingValues["Name"]);
                    Assert.Equal(13, clonedBuildingValues["Shadow1"]);
                    Assert.Equal("Pine Walk", clonedBuildingValues["Shadow2"]);

                    Assert.Equal("Building One Prime", buildingValues["Name"]);
                    Assert.Equal(12, buildingValues["Shadow1"]);
                    Assert.Equal("The Avenue", buildingValues["Shadow2"]);
                }
            }
        }

        [Fact]
        public virtual void Values_in_cloned_dictionary_can_be_set_with_IProperty()
        {
            using (var context = CreateContext())
            {
                var building = context.Set<Building>().Single(b => b.Name == "Building One");
                var entry = context.Entry(building);

                var buildingValues = entry.CurrentValues;
                var clonedBuildingValues = buildingValues.Clone();

                Assert.Equal("Building One", clonedBuildingValues["Name"]);
                Assert.Equal(1500000m, clonedBuildingValues["Value"]);
                Assert.Equal(11, clonedBuildingValues["Shadow1"]);
                Assert.Equal("Meadow Drive", clonedBuildingValues["Shadow2"]);

                // Test modification of cloned property values does not impact original property values

                var newKey = new Guid();
                clonedBuildingValues[entry.Property(e => e.BuildingId).Metadata] = newKey; // Can change primary key on clone
                clonedBuildingValues[entry.Property(e => e.Name).Metadata] = "Building 18";
                clonedBuildingValues[entry.Property("Shadow1").Metadata] = 13;
                clonedBuildingValues[entry.Property("Shadow2").Metadata] = "Pine Walk";

                Assert.Equal(newKey, clonedBuildingValues["BuildingId"]);
                Assert.Equal("Building 18", clonedBuildingValues["Name"]);
                Assert.Equal(13, clonedBuildingValues["Shadow1"]);
                Assert.Equal("Pine Walk", clonedBuildingValues["Shadow2"]);

                Assert.Equal("Building One", buildingValues["Name"]);
                Assert.Equal(11, buildingValues["Shadow1"]);
                Assert.Equal("Meadow Drive", buildingValues["Shadow2"]);
            }
        }

        [Fact]
        public virtual void Using_bad_property_names_throws()
        {
            using (var context = CreateContext())
            {
                var building = context.Set<Building>().Single(b => b.Name == "Building One");
                var entry = context.Entry(building);

                var buildingValues = entry.CurrentValues;
                var clonedBuildingValues = buildingValues.Clone();

                Assert.Equal(
                    CoreStrings.PropertyNotFound("Foo", nameof(Building)),
                    Assert.Throws<InvalidOperationException>(() => buildingValues["Foo"]).Message);

                Assert.Equal(
                    CoreStrings.PropertyNotFound("Foo", nameof(Building)),
                    Assert.Throws<InvalidOperationException>(() => clonedBuildingValues["Foo"]).Message);

                Assert.Equal(
                    CoreStrings.PropertyNotFound("Foo", nameof(Building)),
                    Assert.Throws<InvalidOperationException>(() => buildingValues["Foo"] = "foo").Message);

                Assert.Equal(
                    CoreStrings.PropertyNotFound("Foo", nameof(Building)),
                    Assert.Throws<InvalidOperationException>(() => clonedBuildingValues["Foo"] = "foo").Message);

                Assert.Equal(
                    CoreStrings.PropertyNotFound("Foo", nameof(Building)),
                    Assert.Throws<InvalidOperationException>(() => clonedBuildingValues.GetValue<string>("Foo")).Message);
            }
        }

        [Fact]
        public virtual void Using_bad_IProperty_instances_throws()
        {
            using (var context = CreateContext())
            {
                var building = context.Set<Building>().Single(b => b.Name == "Building One");
                var entry = context.Entry(building);

                var buildingValues = entry.CurrentValues;
                var clonedBuildingValues = buildingValues.Clone();

                var property = context.Model.FindEntityType(typeof(Whiteboard)).FindProperty(nameof(Whiteboard.AssetTag));

                Assert.Equal(
                    CoreStrings.PropertyDoesNotBelong("AssetTag", nameof(Whiteboard), nameof(Building)),
                    Assert.Throws<InvalidOperationException>(() => buildingValues[property]).Message);

                Assert.Equal(
                    CoreStrings.PropertyDoesNotBelong("AssetTag", nameof(Whiteboard), nameof(Building)),
                    Assert.Throws<InvalidOperationException>(() => clonedBuildingValues[property]).Message);

                Assert.Equal(
                    CoreStrings.PropertyDoesNotBelong("AssetTag", nameof(Whiteboard), nameof(Building)),
                    Assert.Throws<InvalidOperationException>(() => buildingValues[property] = "foo").Message);

                Assert.Equal(
                    CoreStrings.PropertyDoesNotBelong("AssetTag", nameof(Whiteboard), nameof(Building)),
                    Assert.Throws<InvalidOperationException>(() => clonedBuildingValues[property] = "foo").Message);

                Assert.Equal(
                    CoreStrings.PropertyDoesNotBelong("AssetTag", nameof(Whiteboard), nameof(Building)),
                    Assert.Throws<InvalidOperationException>(() => clonedBuildingValues.GetValue<string>(property)).Message);
            }
        }

        [Fact]
        public virtual void Using_bad_property_names_throws_derived()
        {
            using (var context = CreateContext())
            {
                var employee = context.Set<Employee>().OfType<CurrentEmployee>().Single(b => b.FirstName == "Rowan");
                var entry = context.Entry(employee);

                var values = entry.CurrentValues;
                var clonedValues = values.Clone();

                Assert.Equal(
                    CoreStrings.PropertyNotFound("Shadow4", nameof(CurrentEmployee)),
                    Assert.Throws<InvalidOperationException>(() => values["Shadow4"]).Message);

                Assert.Equal(
                    CoreStrings.PropertyNotFound("Shadow4", nameof(CurrentEmployee)),
                    Assert.Throws<InvalidOperationException>(() => clonedValues["Shadow4"]).Message);

                Assert.Equal(
                    CoreStrings.PropertyNotFound("Shadow4", nameof(CurrentEmployee)),
                    Assert.Throws<InvalidOperationException>(() => values["Shadow4"] = "foo").Message);

                Assert.Equal(
                    CoreStrings.PropertyNotFound("Shadow4", nameof(CurrentEmployee)),
                    Assert.Throws<InvalidOperationException>(() => clonedValues["Shadow4"] = "foo").Message);

                Assert.Equal(
                    CoreStrings.PropertyNotFound("TerminationDate", nameof(CurrentEmployee)),
                    Assert.Throws<InvalidOperationException>(() => values["TerminationDate"]).Message);

                Assert.Equal(
                    CoreStrings.PropertyNotFound("TerminationDate", nameof(CurrentEmployee)),
                    Assert.Throws<InvalidOperationException>(() => clonedValues["TerminationDate"]).Message);

                Assert.Equal(
                    CoreStrings.PropertyNotFound("TerminationDate", nameof(CurrentEmployee)),
                    Assert.Throws<InvalidOperationException>(() => values["TerminationDate"] = "foo").Message);

                Assert.Equal(
                    CoreStrings.PropertyNotFound("TerminationDate", nameof(CurrentEmployee)),
                    Assert.Throws<InvalidOperationException>(() => clonedValues["TerminationDate"] = "foo").Message);

                Assert.Equal(
                    CoreStrings.PropertyNotFound("Shadow4", nameof(CurrentEmployee)),
                    Assert.Throws<InvalidOperationException>(() => clonedValues.GetValue<string>("Shadow4")).Message);

                Assert.Equal(
                    CoreStrings.PropertyNotFound("TerminationDate", nameof(CurrentEmployee)),
                    Assert.Throws<InvalidOperationException>(() => clonedValues.GetValue<string>("TerminationDate")).Message);
            }
        }

        [Fact]
        public virtual void Using_bad_IProperty_instances_throws_derived()
        {
            using (var context = CreateContext())
            {
                var employee = context.Set<Employee>().OfType<CurrentEmployee>().Single(b => b.FirstName == "Rowan");
                var entry = context.Entry(employee);

                var values = entry.CurrentValues;
                var clonedValues = values.Clone();

                var shadowProperty = context.Model.FindEntityType(typeof(PastEmployee)).FindProperty("Shadow4");
                var termProperty = context.Model.FindEntityType(typeof(PastEmployee)).FindProperty(nameof(PastEmployee.TerminationDate));

                Assert.Equal(
                    CoreStrings.PropertyDoesNotBelong("Shadow4", nameof(PastEmployee), nameof(CurrentEmployee)),
                    Assert.Throws<InvalidOperationException>(() => values[shadowProperty]).Message);

                Assert.Equal(
                    CoreStrings.PropertyDoesNotBelong("Shadow4", nameof(PastEmployee), nameof(CurrentEmployee)),
                    Assert.Throws<InvalidOperationException>(() => clonedValues[shadowProperty]).Message);

                Assert.Equal(
                    CoreStrings.PropertyDoesNotBelong("Shadow4", nameof(PastEmployee), nameof(CurrentEmployee)),
                    Assert.Throws<InvalidOperationException>(() => values[shadowProperty] = "foo").Message);

                Assert.Equal(
                    CoreStrings.PropertyDoesNotBelong("Shadow4", nameof(PastEmployee), nameof(CurrentEmployee)),
                    Assert.Throws<InvalidOperationException>(() => clonedValues[shadowProperty] = "foo").Message);

                Assert.Equal(
                    CoreStrings.PropertyDoesNotBelong("Shadow4", nameof(PastEmployee), nameof(CurrentEmployee)),
                    Assert.Throws<InvalidOperationException>(() => clonedValues.GetValue<string>(shadowProperty)).Message);

                Assert.Equal(
                    CoreStrings.PropertyDoesNotBelong("TerminationDate", nameof(PastEmployee), nameof(CurrentEmployee)),
                    Assert.Throws<InvalidOperationException>(() => values[termProperty]).Message);

                Assert.Equal(
                    CoreStrings.PropertyDoesNotBelong("TerminationDate", nameof(PastEmployee), nameof(CurrentEmployee)),
                    Assert.Throws<InvalidOperationException>(() => clonedValues[termProperty]).Message);

                Assert.Equal(
                    CoreStrings.PropertyDoesNotBelong("TerminationDate", nameof(PastEmployee), nameof(CurrentEmployee)),
                    Assert.Throws<InvalidOperationException>(() => values[termProperty] = "foo").Message);

                Assert.Equal(
                    CoreStrings.PropertyDoesNotBelong("TerminationDate", nameof(PastEmployee), nameof(CurrentEmployee)),
                    Assert.Throws<InvalidOperationException>(() => clonedValues[termProperty] = "foo").Message);

                Assert.Equal(
                    CoreStrings.PropertyDoesNotBelong("TerminationDate", nameof(PastEmployee), nameof(CurrentEmployee)),
                    Assert.Throws<InvalidOperationException>(() => clonedValues.GetValue<string>(termProperty)).Message);
            }
        }

        [Fact]
        public virtual Task Current_values_can_be_copied_into_a_non_generic_cloned_dictionary()
        {
            return TestNonGenericPropertyValuesCloneToValues(e => Task.FromResult(e.CurrentValues), expectOriginalValues: false);
        }

        [Fact]
        public virtual Task Original_values_can_be_copied_into_a_non_generic_cloned_dictionary()
        {
            return TestNonGenericPropertyValuesCloneToValues(e => Task.FromResult(e.OriginalValues), expectOriginalValues: true);
        }

        [Fact]
        public virtual Task Store_values_can_be_copied_into_a_non_generic_cloned_dictionary()
        {
            return TestNonGenericPropertyValuesCloneToValues(e => Task.FromResult(e.GetDatabaseValues()), expectOriginalValues: true);
        }

        [Fact]
        public virtual Task Store_values_can_be_copied_asynchronously_into_a_non_generic_cloned_dictionary()
        {
            return TestNonGenericPropertyValuesCloneToValues(e => e.GetDatabaseValuesAsync(), expectOriginalValues: true);
        }

        private async Task TestNonGenericPropertyValuesCloneToValues(
            Func<EntityEntry, Task<PropertyValues>> getPropertyValues,
            bool expectOriginalValues)
        {
            using (var context = CreateContext())
            {
                var building = context.Set<Building>().Single(b => b.Name == "Building One");

                building.Name = "Building One Prime";
                building.Value = 1500001m;
                context.Entry(building).Property("Shadow1").CurrentValue = 12;
                context.Entry(building).Property("Shadow2").CurrentValue = "The Avenue";

                var buildingValues = await getPropertyValues(context.Entry(building));

                var clonedBuildingValues = buildingValues.Clone();

                if (expectOriginalValues)
                {
                    Assert.Equal("Building One", clonedBuildingValues["Name"]);
                    Assert.Equal(1500000m, clonedBuildingValues["Value"]);
                    Assert.Equal(11, clonedBuildingValues["Shadow1"]);
                    Assert.Equal("Meadow Drive", clonedBuildingValues["Shadow2"]);
                }
                else
                {
                    Assert.Equal("Building One Prime", clonedBuildingValues["Name"]);
                    Assert.Equal(1500001m, clonedBuildingValues["Value"]);
                    Assert.Equal(12, clonedBuildingValues["Shadow1"]);
                    Assert.Equal("The Avenue", clonedBuildingValues["Shadow2"]);
                }

                // Test modification of cloned dictionaries does not impact original property values

                var newKey = new Guid();
                clonedBuildingValues["BuildingId"] = newKey; // Can change primary key on clone
                clonedBuildingValues["Name"] = "Building 18";
                clonedBuildingValues["Shadow1"] = 13;
                clonedBuildingValues["Shadow2"] = "Pine Walk";

                if (expectOriginalValues)
                {
                    Assert.Equal(newKey, clonedBuildingValues["BuildingId"]);
                    Assert.Equal("Building 18", clonedBuildingValues["Name"]);
                    Assert.Equal(13, clonedBuildingValues["Shadow1"]);
                    Assert.Equal("Pine Walk", clonedBuildingValues["Shadow2"]);

                    Assert.Equal("Building One", buildingValues["Name"]);
                    Assert.Equal(11, buildingValues["Shadow1"]);
                    Assert.Equal("Meadow Drive", buildingValues["Shadow2"]);
                }
                else
                {
                    Assert.Equal(newKey, clonedBuildingValues["BuildingId"]);
                    Assert.Equal("Building 18", clonedBuildingValues["Name"]);
                    Assert.Equal(13, clonedBuildingValues["Shadow1"]);
                    Assert.Equal("Pine Walk", clonedBuildingValues["Shadow2"]);

                    Assert.Equal("Building One Prime", buildingValues["Name"]);
                    Assert.Equal(12, buildingValues["Shadow1"]);
                    Assert.Equal("The Avenue", buildingValues["Shadow2"]);
                }
            }
        }

        [Fact]
        public virtual Task Current_values_can_be_read_or_set_for_an_object_in_the_Deleted_state()
        {
            return TestPropertyValuesPositiveForState(e => Task.FromResult(e.CurrentValues), EntityState.Deleted, expectOriginalValues: false);
        }

        [Fact]
        public virtual Task Original_values_can_be_read_and_set_for_an_object_in_the_Deleted_state()
        {
            return TestPropertyValuesPositiveForState(e => Task.FromResult(e.OriginalValues), EntityState.Deleted, expectOriginalValues: true);
        }

        [Fact]
        public virtual Task Store_values_can_be_read_and_set_for_an_object_in_the_Deleted_state()
        {
            return TestPropertyValuesPositiveForState(e => Task.FromResult(e.GetDatabaseValues()), EntityState.Deleted, expectOriginalValues: true);
        }

        [Fact]
        public virtual Task Store_values_can_be_read_and_set_for_an_object_in_the_Deleted_state_asynchronously()
        {
            return TestPropertyValuesPositiveForState(e => e.GetDatabaseValuesAsync(), EntityState.Deleted, expectOriginalValues: true);
        }

        [Fact]
        public virtual Task Current_values_can_be_read_and_set_for_an_object_in_the_Unchanged_state()
        {
            return TestPropertyValuesPositiveForState(e => Task.FromResult(e.CurrentValues), EntityState.Unchanged, expectOriginalValues: false);
        }

        [Fact]
        public virtual Task Original_values_can_be_read_and_set_for_an_object_in_the_Unchanged_state()
        {
            return TestPropertyValuesPositiveForState(e => Task.FromResult(e.OriginalValues), EntityState.Unchanged, expectOriginalValues: true);
        }

        [Fact]
        public virtual Task Store_values_can_be_read_and_set_for_an_object_in_the_Unchanged_state()
        {
            return TestPropertyValuesPositiveForState(e => Task.FromResult(e.GetDatabaseValues()), EntityState.Unchanged, expectOriginalValues: true);
        }

        [Fact]
        public virtual Task Store_values_can_be_read_and_set_for_an_object_in_the_Unchanged_state_asynchronously()
        {
            return TestPropertyValuesPositiveForState(e => e.GetDatabaseValuesAsync(), EntityState.Unchanged, expectOriginalValues: true);
        }

        [Fact]
        public virtual Task Current_values_can_be_read_and_set_for_an_object_in_the_Modified_state()
        {
            return TestPropertyValuesPositiveForState(e => Task.FromResult(e.CurrentValues), EntityState.Modified, expectOriginalValues: false);
        }

        [Fact]
        public virtual Task Original_values_can_be_read_and_set_for_an_object_in_the_Modified_state()
        {
            return TestPropertyValuesPositiveForState(e => Task.FromResult(e.OriginalValues), EntityState.Modified, expectOriginalValues: true);
        }

        [Fact]
        public virtual Task Store_values_can_be_read_and_set_for_an_object_in_the_Modified_state()
        {
            return TestPropertyValuesPositiveForState(e => Task.FromResult(e.GetDatabaseValues()), EntityState.Modified, expectOriginalValues: true);
        }

        [Fact]
        public virtual Task Store_values_can_be_read_and_set_for_an_object_in_the_Modified_state_asynchronously()
        {
            return TestPropertyValuesPositiveForState(e => e.GetDatabaseValuesAsync(), EntityState.Modified, expectOriginalValues: true);
        }

        [Fact]
        public virtual Task Current_values_can_be_read_and_set_for_an_object_in_the_Added_state()
        {
            return TestPropertyValuesPositiveForState(e => Task.FromResult(e.CurrentValues), EntityState.Added, expectOriginalValues: false);
        }

        [Fact]
        public virtual Task Original_values_can_be_read_or_set_for_an_object_in_the_Added_state()
        {
            return TestPropertyValuesPositiveForState(e => Task.FromResult(e.OriginalValues), EntityState.Added, expectOriginalValues: true);
        }

        [Fact]
        public virtual Task Store_values_can_be_read_or_set_for_an_object_in_the_Added_state()
        {
            return TestPropertyValuesPositiveForState(e => Task.FromResult(e.GetDatabaseValues()), EntityState.Detached, expectOriginalValues: true);
        }

        [Fact]
        public virtual Task Store_values_can_be_read_or_set_for_an_object_in_the_Added_state_asynchronously()
        {
            return TestPropertyValuesPositiveForState(e => e.GetDatabaseValuesAsync(), EntityState.Detached, expectOriginalValues: true);
        }

        [Fact]
        public virtual Task Current_values_can_be_read_or_set_for_a_Detached_object()
        {
            return TestPropertyValuesPositiveForState(e => Task.FromResult(e.CurrentValues), EntityState.Detached, expectOriginalValues: false);
        }

        [Fact]
        public virtual Task Original_values_can_be_read_or_set_for_a_Detached_object()
        {
            return TestPropertyValuesPositiveForState(e => Task.FromResult(e.OriginalValues), EntityState.Detached, expectOriginalValues: true);
        }

        [Fact]
        public virtual Task Store_values_can_be_read_or_set_for_a_Detached_object()
        {
            return TestPropertyValuesPositiveForState(e => Task.FromResult(e.GetDatabaseValues()), EntityState.Detached, expectOriginalValues: true);
        }

        [Fact]
        public virtual Task Store_values_can_be_read_or_set_for_a_Detached_object_asynchronously()
        {
            return TestPropertyValuesPositiveForState(e => e.GetDatabaseValuesAsync(), EntityState.Detached, expectOriginalValues: true);
        }

        private async Task TestPropertyValuesPositiveForState(
            Func<EntityEntry<Building>, Task<PropertyValues>> getPropertyValues,
            EntityState state,
            bool expectOriginalValues)
        {
            using (var context = CreateContext())
            {
                var building = context.Set<Building>().Single(b => b.Name == "Building One");
                var entry = context.Entry(building);
                entry.State = state;

                building.Name = "Building One Prime";

                var values = await getPropertyValues(entry);

                Assert.Equal(expectOriginalValues ? "Building One" : "Building One Prime", values["Name"]);

                values["Name"] = "Building One Optimal";

                Assert.Equal("Building One Optimal", values["Name"]);
            }
        }

        [Theory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Added, true)]
        [InlineData(EntityState.Added, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        [InlineData(EntityState.Detached, true)]
        [InlineData(EntityState.Detached, false)]
        public async Task Values_can_be_reloaded_from_database_for_entity_in_any_state(EntityState state, bool async)
        {
            using (var context = CreateContext())
            {
                var building = context.Set<Building>().Single(b => b.Name == "Building One");
                var entry = context.Entry(building);

                entry.Property(e => e.Name).OriginalValue = "Original Building";
                building.Name = "Building One Prime";

                entry.State = state;

                if (async)
                {
                    await entry.ReloadAsync();
                }
                else
                {
                    entry.Reload();
                }

                Assert.Equal("Building One", entry.Property(e => e.Name).OriginalValue);
                Assert.Equal("Building One", entry.Property(e => e.Name).CurrentValue);
                Assert.Equal("Building One", building.Name);

                Assert.Equal(EntityState.Unchanged, entry.State);
            }
        }

        [Theory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Added, true)]
        [InlineData(EntityState.Added, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        [InlineData(EntityState.Detached, true)]
        [InlineData(EntityState.Detached, false)]
        public async Task Reload_when_entity_deleted_in_store_can_happen_for_any_state(EntityState state, bool async)
        {
            using (var context = CreateContext())
            {
                var building = Building.Create(Guid.NewGuid(), "Bag End", 77);
                var entry = context.Entry(building);

                entry.State = state;

                if (async)
                {
                    await entry.ReloadAsync();
                }
                else
                {
                    entry.Reload();
                }

                Assert.Equal("Bag End", entry.Property(e => e.Name).OriginalValue);
                Assert.Equal("Bag End", entry.Property(e => e.Name).CurrentValue);
                Assert.Equal("Bag End", building.Name);

                Assert.Equal(state == EntityState.Added ? EntityState.Added : EntityState.Detached, entry.State);
            }
        }

        [Fact]
        public virtual void Current_values_can_be_set_from_an_object_using_generic_dictionary()
        {
            TestGenericObjectSetValues(e => e.CurrentValues, (e, n) => e.Property(n).CurrentValue);
        }

        [Fact]
        public virtual void Original_values_can_be_set_from_an_object_using_generic_dictionary()
        {
            TestGenericObjectSetValues(e => e.OriginalValues, (e, n) => e.Property(n).OriginalValue);
        }

        private void TestGenericObjectSetValues(
            Func<EntityEntry<Building>, PropertyValues> getPropertyValues,
            Func<EntityEntry, string, object> getValue)
        {
            using (var context = CreateContext())
            {
                var building = context.Set<Building>().Single(b => b.Name == "Building One");
                var buildingValues = getPropertyValues(context.Entry(building));

                var newBuilding = Building.Create(
                    new Guid(building.BuildingId.ToString()),
                    "Values End",
                    building.Value);

                buildingValues.SetValues(newBuilding);

                // Check Values

                Assert.Equal("Values End", buildingValues["Name"]);
                Assert.Equal(1500000m, buildingValues["Value"]);
                Assert.Equal(11, buildingValues["Shadow1"]);
                Assert.Equal("Meadow Drive", buildingValues["Shadow2"]);

                ValidateBuildingPropereties(context.Entry(building), getValue, 11, "Meadow Drive");
            }
        }

        private static void ValidateBuildingPropereties(
            EntityEntry buildingEntry,
            Func<EntityEntry, string, object> getValue,
            int shadow1,
            string shadow2)
        {
            Assert.Equal("Values End", getValue(buildingEntry, "Name"));
            Assert.Equal(1500000m, getValue(buildingEntry, "Value"));
            Assert.Equal(shadow1, getValue(buildingEntry, "Shadow1"));
            Assert.Equal(shadow2, getValue(buildingEntry, "Shadow2"));

            Assert.True(buildingEntry.Property("Name").IsModified);
            Assert.False(buildingEntry.Property("BuildingId").IsModified);
            Assert.False(buildingEntry.Property("Value").IsModified);
            Assert.Equal(shadow1 != 11, buildingEntry.Property("Shadow1").IsModified);
            Assert.Equal(shadow2 != "Meadow Drive", buildingEntry.Property("Shadow2").IsModified);
        }

        [Fact]
        public virtual void Current_values_can_be_set_from_an_object_using_non_generic_dictionary()
        {
            TestNonGenericObjectSetValues(e => e.CurrentValues, (e, n) => e.Property(n).CurrentValue);
        }

        [Fact]
        public virtual void Original_values_can_be_set_from_an_object_using_non_generic_dictionary()
        {
            TestNonGenericObjectSetValues(e => e.OriginalValues, (e, n) => e.Property(n).OriginalValue);
        }

        private void TestNonGenericObjectSetValues(
            Func<EntityEntry, PropertyValues> getPropertyValues,
            Func<EntityEntry, string, object> getValue)
        {
            using (var context = CreateContext())
            {
                var building = context.Set<Building>().Single(b => b.Name == "Building One");
                var buildingValues = getPropertyValues(context.Entry(building));

                var newBuilding = Building.Create(
                    new Guid(building.BuildingId.ToString()),
                    "Values End",
                    building.Value);

                buildingValues.SetValues(newBuilding);

                // Check Values

                Assert.Equal("Values End", buildingValues["Name"]);
                Assert.Equal(1500000m, buildingValues["Value"]);
                Assert.Equal(11, buildingValues["Shadow1"]);
                Assert.Equal("Meadow Drive", buildingValues["Shadow2"]);

                ValidateBuildingPropereties(context.Entry(building), getValue, 11, "Meadow Drive");
            }
        }

        [Fact]
        public virtual void Current_values_can_be_set_from_DTO_object_using_non_generic_dictionary()
        {
            TestNonGenericDtoSetValues(e => e.CurrentValues, (e, n) => e.Property(n).CurrentValue);
        }

        [Fact]
        public virtual void Original_values_can_be_set_from_DTO_object_using_non_generic_dictionary()
        {
            TestNonGenericDtoSetValues(e => e.OriginalValues, (e, n) => e.Property(n).OriginalValue);
        }

        private void TestNonGenericDtoSetValues(
            Func<EntityEntry, PropertyValues> getPropertyValues,
            Func<EntityEntry, string, object> getValue)
        {
            using (var context = CreateContext())
            {
                var building = context.Set<Building>().Single(b => b.Name == "Building One");
                var buildingValues = getPropertyValues(context.Entry(building));

                var newBuilding = new BuildingDto
                {
                    BuildingId = new Guid(building.BuildingId.ToString()),
                    Name = "Values End",
                    Value = building.Value,
                    Shadow1 = 777
                };

                buildingValues.SetValues(newBuilding);

                // Check Values

                Assert.Equal("Values End", buildingValues["Name"]);
                Assert.Equal(1500000m, buildingValues["Value"]);
                Assert.Equal(777, buildingValues["Shadow1"]);
                Assert.Equal("Meadow Drive", buildingValues["Shadow2"]);

                ValidateBuildingPropereties(context.Entry(building), getValue, 777, "Meadow Drive");
            }
        }

        [Fact]
        public virtual void Current_values_can_be_set_from_DTO_object_missing_key_using_non_generic_dictionary()
        {
            TestNonGenericDtoNoKeySetValues(e => e.CurrentValues, (e, n) => e.Property(n).CurrentValue);
        }

        [Fact]
        public virtual void Original_values_can_be_set_from_DTO_object_missing_key_using_non_generic_dictionary()
        {
            TestNonGenericDtoNoKeySetValues(e => e.OriginalValues, (e, n) => e.Property(n).OriginalValue);
        }

        private void TestNonGenericDtoNoKeySetValues(
            Func<EntityEntry, PropertyValues> getPropertyValues,
            Func<EntityEntry, string, object> getValue)
        {
            using (var context = CreateContext())
            {
                var building = context.Set<Building>().Single(b => b.Name == "Building One");
                var buildingValues = getPropertyValues(context.Entry(building));

                var newBuilding = new BuildingDtoNoKey
                {
                    Name = "Values End",
                    Value = building.Value,
                    Shadow2 = "Cheese"
                };

                buildingValues.SetValues(newBuilding);

                // Check Values

                Assert.Equal("Values End", buildingValues["Name"]);
                Assert.Equal(1500000m, buildingValues["Value"]);
                Assert.Equal("Cheese", buildingValues["Shadow2"]);

                ValidateBuildingPropereties(context.Entry(building), getValue, 11, "Cheese");
            }
        }

        [Fact]
        public virtual void Current_values_can_be_set_from_dictionary()
        {
            TestDictionarySetValues(e => e.CurrentValues, (e, n) => e.Property(n).CurrentValue);
        }

        [Fact]
        public virtual void Original_values_can_be_set_from_dictionary()
        {
            TestDictionarySetValues(e => e.OriginalValues, (e, n) => e.Property(n).OriginalValue);
        }

        private void TestDictionarySetValues(
            Func<EntityEntry, PropertyValues> getPropertyValues,
            Func<EntityEntry, string, object> getValue)
        {
            using (var context = CreateContext())
            {
                var building = context.Set<Building>().Single(b => b.Name == "Building One");
                var buildingValues = getPropertyValues(context.Entry(building));

                var dictionary = new Dictionary<string, object>
                {
                    { "BuildingId", new Guid(building.BuildingId.ToString()) },
                    { "Name", "Values End" },
                    { "Value", building.Value },
                    { "Shadow1", 13 },
                    { "Shadow2", "Pine Walk" },
                    { "PrincipalMailRoomId", 0 }
                };

                buildingValues.SetValues(dictionary);

                // Check Values

                Assert.Equal("Values End", buildingValues["Name"]);
                Assert.Equal(1500000m, buildingValues["Value"]);
                Assert.Equal(13, buildingValues["Shadow1"]);
                Assert.Equal("Pine Walk", buildingValues["Shadow2"]);

                ValidateBuildingPropereties(context.Entry(building), getValue, 13, "Pine Walk");
            }
        }

        [Fact]
        public virtual void Current_values_can_be_set_from_dictionary_some_missing()
        {
            TestPartialDictionarySetValues(e => e.CurrentValues, (e, n) => e.Property(n).CurrentValue);
        }

        [Fact]
        public virtual void Original_values_can_be_set_from_dictionary_some_missing()
        {
            TestPartialDictionarySetValues(e => e.OriginalValues, (e, n) => e.Property(n).OriginalValue);
        }

        private void TestPartialDictionarySetValues(
            Func<EntityEntry, PropertyValues> getPropertyValues,
            Func<EntityEntry, string, object> getValue)
        {
            using (var context = CreateContext())
            {
                var building = context.Set<Building>().Single(b => b.Name == "Building One");
                var buildingValues = getPropertyValues(context.Entry(building));

                var dictionary = new Dictionary<string, object>
                {
                    { "BuildingId", new Guid(building.BuildingId.ToString()) },
                    { "Name", "Values End" },
                    { "Value", building.Value },
                    { "Shadow1", 777 }
                };

                buildingValues.SetValues(dictionary);

                // Check Values

                Assert.Equal("Values End", buildingValues["Name"]);
                Assert.Equal(1500000m, buildingValues["Value"]);
                Assert.Equal(777, buildingValues["Shadow1"]);
                Assert.Equal("Meadow Drive", buildingValues["Shadow2"]);

                ValidateBuildingPropereties(context.Entry(building), getValue, 777, "Meadow Drive");
            }
        }

        [Fact]
        public virtual void Current_values_can_be_set_from_one_generic_dictionary_to_another_generic_dictionary()
        {
            TestGenericValuesSetValues(e => e.CurrentValues, (e, n) => e.Property(n).CurrentValue);
        }

        [Fact]
        public virtual void Original_values_can_be_set_from_one_generic_dictionary_to_another_generic_dictionary()
        {
            TestGenericValuesSetValues(e => e.OriginalValues, (e, n) => e.Property(n).OriginalValue);
        }

        private void TestGenericValuesSetValues(
            Func<EntityEntry<Building>, PropertyValues> getPropertyValues,
            Func<EntityEntry, string, object> getValue)
        {
            using (var context = CreateContext())
            {
                var building = context.Set<Building>().Single(b => b.Name == "Building One");
                var buildingValues = getPropertyValues(context.Entry(building));

                var clonedBuildingValues = buildingValues.Clone();

                clonedBuildingValues["BuildingId"] = new Guid(building.BuildingId.ToString());
                clonedBuildingValues["Name"] = "Values End";
                clonedBuildingValues["Value"] = building.Value;
                clonedBuildingValues["Shadow1"] = 13;
                clonedBuildingValues["Shadow2"] = "Pine Walk";

                buildingValues.SetValues(clonedBuildingValues);

                // Check Values

                Assert.Equal("Values End", buildingValues["Name"]);
                Assert.Equal(1500000m, buildingValues["Value"]);
                Assert.Equal(13, clonedBuildingValues["Shadow1"]);
                Assert.Equal("Pine Walk", clonedBuildingValues["Shadow2"]);

                ValidateBuildingPropereties(context.Entry(building), getValue, 13, "Pine Walk");
            }
        }

        [Fact]
        public virtual void Current_values_can_be_set_from_one_non_generic_dictionary_to_another_generic_dictionary()
        {
            TestNonGenericValuesSetValues(e => e.CurrentValues, (e, n) => e.Property(n).CurrentValue);
        }

        [Fact]
        public virtual void Original_values_can_be_set_from_one_non_generic_dictionary_to_another_generic_dictionary()
        {
            TestNonGenericValuesSetValues(e => e.OriginalValues, (e, n) => e.Property(n).OriginalValue);
        }

        private void TestNonGenericValuesSetValues(
            Func<EntityEntry, PropertyValues> getPropertyValues,
            Func<EntityEntry, string, object> getValue)
        {
            using (var context = CreateContext())
            {
                var building = context.Set<Building>().Single(b => b.Name == "Building One");
                var buildingValues = getPropertyValues(context.Entry(building));

                var clonedBuildingValues = buildingValues.Clone();

                clonedBuildingValues["BuildingId"] = new Guid(building.BuildingId.ToString());
                clonedBuildingValues["Name"] = "Values End";
                clonedBuildingValues["Value"] = building.Value;
                clonedBuildingValues["Shadow1"] = 13;
                clonedBuildingValues["Shadow2"] = "Pine Walk";

                buildingValues.SetValues(clonedBuildingValues);

                // Check Values

                Assert.Equal("Values End", buildingValues["Name"]);
                Assert.Equal(1500000m, buildingValues["Value"]);
                Assert.Equal(13, buildingValues["Shadow1"]);
                Assert.Equal("Pine Walk", buildingValues["Shadow2"]);

                ValidateBuildingPropereties(context.Entry(building), getValue, 13, "Pine Walk");
            }
        }

        [Fact]
        public virtual void Primary_key_in_current_values_cannot_be_changed_in_property_dictionary()
        {
            TestKeyChange(e => e.CurrentValues);
        }

        [Fact]
        public virtual void Primary_key_in_original_values_cannot_be_changed_in_property_dictionary()
        {
            TestKeyChange(e => e.OriginalValues);
        }

        private void TestKeyChange(Func<EntityEntry<Building>, PropertyValues> getPropertyValues)
        {
            using (var context = CreateContext())
            {
                var building = context.Set<Building>().Single(b => b.Name == "Building One");
                var values = getPropertyValues(context.Entry(building));

                Assert.Equal(
                    CoreStrings.KeyReadOnly(nameof(Building.BuildingId), nameof(Building)),
                    Assert.Throws<InvalidOperationException>(() => values["BuildingId"] = new Guid()).Message);
            }
        }

        [Fact]
        public virtual void Non_nullable_property_in_current_values_results_in_conceptual_null()
        {
            using (var context = CreateContext())
            {
                var building = context.Set<Building>().Single(b => b.Name == "Building One");
                var entry = context.Entry(building);
                var values = entry.CurrentValues;

                Assert.False(entry.GetInfrastructure().HasConceptualNull);

                values["Value"] = null;

                Assert.True(entry.GetInfrastructure().HasConceptualNull);

                Assert.Equal(1500000m, values["Value"]);
                Assert.Equal(1500000m, building.Value);
            }
        }

        [Fact]
        public virtual void Non_nullable_shadow_property_in_current_values_results_in_conceptual_null()
        {
            using (var context = CreateContext())
            {
                var building = context.Set<Building>().Single(b => b.Name == "Building One");
                var entry = context.Entry(building);
                var values = entry.CurrentValues;

                Assert.False(entry.GetInfrastructure().HasConceptualNull);

                values["Shadow1"] = null;

                Assert.True(entry.GetInfrastructure().HasConceptualNull);

                Assert.Equal(11, values["Shadow1"]);
            }
        }

        [Fact]
        public virtual void Non_nullable_property_in_original_values_cannot_be_set_to_null_in_property_dictionary()
        {
            using (var context = CreateContext())
            {
                var building = context.Set<Building>().Single(b => b.Name == "Building One");
                var values = context.Entry(building).OriginalValues;

                Assert.Equal(
                    CoreStrings.ValueCannotBeNull(nameof(Building.Value), nameof(Building), "decimal"),
                    Assert.Throws<InvalidOperationException>(() => values["Value"] = null).Message);

                Assert.Equal(1500000m, values["Value"]);
            }
        }

        [Fact]
        public virtual void Non_nullable_shadow_property_in_original_values_cannot_be_set_to_null_in_property_dictionary()
        {
            using (var context = CreateContext())
            {
                var building = context.Set<Building>().Single(b => b.Name == "Building One");
                var values = context.Entry(building).OriginalValues;

                Assert.Equal(
                    CoreStrings.ValueCannotBeNull("Shadow1", nameof(Building), "int"),
                    Assert.Throws<InvalidOperationException>(() => values["Shadow1"] = null).Message);

                Assert.Equal(11, values["Shadow1"]);
            }
        }

        [Fact]
        public virtual void Non_nullable_property_in_cloned_dictionary_cannot_be_set_to_null()
        {
            using (var context = CreateContext())
            {
                var building = context.Set<Building>().Single(b => b.Name == "Building One");
                var values = context.Entry(building).CurrentValues.Clone();

                Assert.Equal(
                    CoreStrings.ValueCannotBeNull(nameof(Building.Value), nameof(Building), "decimal"),
                    Assert.Throws<InvalidOperationException>(() => values["Value"] = null).Message);
            }
        }

        [Fact]
        public virtual void Property_in_current_values_cannot_be_set_to_instance_of_wrong_type()
        {
            TestSetWrongType(e => e.CurrentValues);
        }

        [Fact]
        public virtual void Property_in_original_values_cannot_be_set_to_instance_of_wrong_type()
        {
            TestSetWrongType(e => e.OriginalValues);
        }

        private void TestSetWrongType(Func<EntityEntry<Building>, PropertyValues> getPropertyValues)
        {
            using (var context = CreateContext())
            {
                var building = context.Set<Building>().Single(b => b.Name == "Building One");
                var values = getPropertyValues(context.Entry(building));

                Assert.Throws<InvalidCastException>(() => values["Name"] = 1);

                Assert.Equal("Building One", values["Name"]);
                Assert.Equal("Building One", building.Name);
            }
        }

        [Fact]
        public virtual void Shadow_property_in_current_values_cannot_be_set_to_instance_of_wrong_type()
        {
            TestSetWrongTypeShadow(e => e.CurrentValues);
        }

        [Fact]
        public virtual void Shadow_property_in_original_values_cannot_be_set_to_instance_of_wrong_type()
        {
            TestSetWrongTypeShadow(e => e.OriginalValues);
        }

        private void TestSetWrongTypeShadow(Func<EntityEntry<Building>, PropertyValues> getPropertyValues)
        {
            using (var context = CreateContext())
            {
                var building = context.Set<Building>().Single(b => b.Name == "Building One");
                var values = getPropertyValues(context.Entry(building));

                Assert.Throws<InvalidCastException>(() => values["Shadow1"] = "foo");

                Assert.Equal(11, values["Shadow1"]);
            }
        }

        [Fact]
        public virtual void Property_in_cloned_dictionary_cannot_be_set_to_instance_of_wrong_type()
        {
            using (var context = CreateContext())
            {
                var building = context.Set<Building>().Single(b => b.Name == "Building One");
                var values = context.Entry(building).CurrentValues.Clone();

                Assert.Equal(
                    CoreStrings.InvalidType(nameof(Building.Name), nameof(Building), "int", "string"),
                    Assert.Throws<InvalidCastException>(() => values["Name"] = 1).Message);

                Assert.Equal("Building One", values["Name"]);
                Assert.Equal("Building One", building.Name);
            }
        }

        [Fact]
        public virtual void Primary_key_in_current_values_cannot_be_changed_by_setting_values_from_object()
        {
            TestKeyChangeByObject(e => e.CurrentValues);
        }

        [Fact]
        public virtual void Primary_key_in_original_values_cannot_be_changed_by_setting_values_from_object()
        {
            TestKeyChangeByObject(e => e.OriginalValues);
        }

        private void TestKeyChangeByObject(Func<EntityEntry<Building>, PropertyValues> getPropertyValues)
        {
            using (var context = CreateContext())
            {
                var building = context.Set<Building>().Single(b => b.Name == "Building One");
                var values = getPropertyValues(context.Entry(building));

                var newBuilding = (Building)values.ToObject();
                newBuilding.BuildingId = new Guid();

                Assert.Equal(
                    CoreStrings.KeyReadOnly(nameof(Building.BuildingId), nameof(Building)),
                    Assert.Throws<InvalidOperationException>(() => values.SetValues(newBuilding)).Message);
            }
        }

        [Fact]
        public virtual void Primary_key_in_current_values_cannot_be_changed_by_setting_values_from_another_dictionary()
        {
            TestKeyChangeByValues(e => e.CurrentValues);
        }

        [Fact]
        public virtual void Primary_key_in_original_values_cannot_be_changed_by_setting_values_from_another_dictionary()
        {
            TestKeyChangeByValues(e => e.OriginalValues);
        }

        private void TestKeyChangeByValues(Func<EntityEntry<Building>, PropertyValues> getPropertyValues)
        {
            using (var context = CreateContext())
            {
                var building = context.Set<Building>().Single(b => b.Name == "Building One");
                var values = getPropertyValues(context.Entry(building));

                var clone = values.Clone();
                clone["BuildingId"] = new Guid();

                Assert.Equal(
                    CoreStrings.KeyReadOnly(nameof(Building.BuildingId), nameof(Building)),
                    Assert.Throws<InvalidOperationException>(() => values.SetValues(clone)).Message);
            }
        }

        [Fact]
        public virtual Task Properties_for_current_values_returns_properties()
        {
            return TestProperties(e => Task.FromResult(e.CurrentValues));
        }

        [Fact]
        public virtual Task Properties_for_original_values_returns_properties()
        {
            return TestProperties(e => Task.FromResult(e.OriginalValues));
        }

        [Fact]
        public virtual Task Properties_for_store_values_returns_properties()
        {
            return TestProperties(e => Task.FromResult(e.GetDatabaseValues()));
        }

        [Fact]
        public virtual Task Properties_for_store_values_returns_properties_asynchronously()
        {
            return TestProperties(e => e.GetDatabaseValuesAsync());
        }

        [Fact]
        public virtual Task Properties_for_cloned_dictionary_returns_properties()
        {
            return TestProperties(e => Task.FromResult(e.CurrentValues.Clone()));
        }

        private async Task TestProperties(Func<EntityEntry<Building>, Task<PropertyValues>> getPropertyValues)
        {
            using (var context = CreateContext())
            {
                var building = context.Set<Building>().Single(b => b.Name == "Building One");
                var buildingValues = await getPropertyValues(context.Entry(building));

                Assert.Equal(
                    new List<string>
                    {
                        "BuildingId",
                        "Name",
                        "PrincipalMailRoomId",
                        "Shadow1",
                        "Shadow2",
                        "Value"
                    },
                    buildingValues.Properties.Select(p => p.Name).ToList());
            }
        }

        [Fact]
        public virtual Task GetDatabaseValues_for_entity_not_in_the_store_returns_null()
        {
            return GetDatabaseValues_for_entity_not_in_the_store_returns_null_implementation(e => Task.FromResult(e.GetDatabaseValues()));
        }

        [Fact]
        public virtual Task GetDatabaseValuesAsync_for_entity_not_in_the_store_returns_null()
        {
            return GetDatabaseValues_for_entity_not_in_the_store_returns_null_implementation(e => e.GetDatabaseValuesAsync());
        }

        private async Task GetDatabaseValues_for_entity_not_in_the_store_returns_null_implementation(
            Func<EntityEntry, Task<PropertyValues>> getPropertyValues)
        {
            using (var context = CreateContext())
            {
                var building = (Building)context.Entry(
                    context.Set<Building>().Single(b => b.Name == "Building One")).CurrentValues.ToObject();

                building.BuildingId = new Guid();

                context.Set<Building>().Attach(building);

                Assert.Null(await getPropertyValues(context.Entry(building)));
            }
        }

        [Fact]
        public virtual Task NonGeneric_GetDatabaseValues_for_entity_not_in_the_store_returns_null()
        {
            return NonGeneric_GetDatabaseValues_for_entity_not_in_the_store_returns_null_implementation(e => Task.FromResult(e.GetDatabaseValues()));
        }

        [Fact]
        public virtual Task NonGeneric_GetDatabaseValuesAsync_for_entity_not_in_the_store_returns_null()
        {
            return NonGeneric_GetDatabaseValues_for_entity_not_in_the_store_returns_null_implementation(e => e.GetDatabaseValuesAsync());
        }

        private async Task NonGeneric_GetDatabaseValues_for_entity_not_in_the_store_returns_null_implementation(
            Func<EntityEntry, Task<PropertyValues>> getPropertyValues)
        {
            using (var context = CreateContext())
            {
                var building =
                    (Building)
                    context.Entry(context.Set<Building>().Single(b => b.Name == "Building One")).CurrentValues.ToObject();
                building.BuildingId = new Guid();

                context.Set<Building>().Attach(building);

                Assert.Null(await getPropertyValues(context.Entry((object)building)));
            }
        }

        [Fact]
        public virtual Task GetDatabaseValues_for_derived_entity_not_in_the_store_returns_null()
        {
            return GetDatabaseValues_for_derived_entity_not_in_the_store_returns_null_implementation(e => Task.FromResult(e.GetDatabaseValues()));
        }

        [Fact]
        public virtual Task GetDatabaseValuesAsync_for_derived_entity_not_in_the_store_returns_null()
        {
            return GetDatabaseValues_for_derived_entity_not_in_the_store_returns_null_implementation(e => e.GetDatabaseValuesAsync());
        }

        private async Task GetDatabaseValues_for_derived_entity_not_in_the_store_returns_null_implementation(
            Func<EntityEntry, Task<PropertyValues>> getPropertyValues)
        {
            using (var context = CreateContext())
            {
                var employee = (CurrentEmployee)context.Entry(
                        context.Set<Employee>()
                            .OfType<CurrentEmployee>()
                            .Single(b => b.FirstName == "Rowan"))
                    .CurrentValues
                    .ToObject();
                employee.EmployeeId = -77;

                context.Set<Employee>().Attach(employee);

                Assert.Null(await getPropertyValues(context.Entry(employee)));
            }
        }

        [Fact]
        public virtual Task NonGeneric_GetDatabaseValues_for_derived_entity_not_in_the_store_returns_null()
        {
            return NonGeneric_GetDatabaseValues_for_derived_entity_not_in_the_store_returns_null_implementation(e => Task.FromResult(e.GetDatabaseValues()));
        }

        [Fact]
        public virtual Task NonGeneric_GetDatabaseValuesAsync_for_derived_entity_not_in_the_store_returns_null()
        {
            return NonGeneric_GetDatabaseValues_for_derived_entity_not_in_the_store_returns_null_implementation(
                e => e.GetDatabaseValuesAsync());
        }

        private async Task NonGeneric_GetDatabaseValues_for_derived_entity_not_in_the_store_returns_null_implementation(
            Func<EntityEntry, Task<PropertyValues>> getPropertyValues)
        {
            using (var context = CreateContext())
            {
                var employee = (CurrentEmployee)context.Entry(
                        context.Set<Employee>()
                            .OfType<CurrentEmployee>()
                            .Single(b => b.FirstName == "Rowan"))
                    .CurrentValues
                    .ToObject();
                employee.EmployeeId = -77;

                context.Set<Employee>().Attach(employee);

                Assert.Null(await getPropertyValues(context.Entry((object)employee)));
            }
        }

        [Fact]
        public virtual Task GetDatabaseValues_for_the_wrong_type_in_the_store_returns_null()
        {
            return GetDatabaseValues_for_the_wrong_type_in_the_store_returns_null_implementation(e => Task.FromResult(e.GetDatabaseValues()));
        }

        [Fact]
        public virtual Task GetDatabaseValuesAsync_for_the_wrong_type_in_the_store_returns_null()
        {
            return GetDatabaseValues_for_the_wrong_type_in_the_store_returns_null_implementation(e => e.GetDatabaseValuesAsync());
        }

        private async Task GetDatabaseValues_for_the_wrong_type_in_the_store_returns_null_implementation(
            Func<EntityEntry, Task<PropertyValues>> getPropertyValues)
        {
            using (var context = CreateContext())
            {
                var pastEmployeeId = context.Set<Employee>()
                    .OfType<PastEmployee>()
                    .AsNoTracking()
                    .OrderBy(e => e.EmployeeId)
                    .FirstOrDefault()
                    .EmployeeId;

                var employee = (CurrentEmployee)context.Entry(
                        context.Set<Employee>()
                            .OfType<CurrentEmployee>()
                            .Single(b => b.FirstName == "Rowan"))
                    .CurrentValues
                    .ToObject();
                employee.EmployeeId = pastEmployeeId;

                context.Set<Employee>().Attach(employee);

                Assert.Null(await getPropertyValues(context.Entry(employee)));
            }
        }

        [Fact]
        public virtual Task NonGeneric_GetDatabaseValues_for_the_wrong_type_in_the_store_throws()
        {
            return NonGeneric_GetDatabaseValues_for_the_wrong_type_in_the_store_throws_implementation(e => Task.FromResult(e.GetDatabaseValues()));
        }

        [Fact]
        public virtual Task NonGeneric_GetDatabaseValuesAsync_for_the_wrong_type_in_the_store_throws()
        {
            return NonGeneric_GetDatabaseValues_for_the_wrong_type_in_the_store_throws_implementation(e => e.GetDatabaseValuesAsync());
        }

        private async Task NonGeneric_GetDatabaseValues_for_the_wrong_type_in_the_store_throws_implementation(
            Func<EntityEntry, Task<PropertyValues>> getPropertyValues)
        {
            using (var context = CreateContext())
            {
                var pastEmployeeId = context.Set<Employee>()
                    .OfType<PastEmployee>()
                    .AsNoTracking()
                    .OrderBy(e => e.EmployeeId)
                    .FirstOrDefault()
                    .EmployeeId;

                var employee = (CurrentEmployee)context.Entry(
                        context.Set<Employee>()
                            .OfType<CurrentEmployee>()
                            .Single(b => b.FirstName == "Rowan"))
                    .CurrentValues
                    .ToObject();
                employee.EmployeeId = pastEmployeeId;

                context.Set<Employee>().Attach(employee);

                Assert.Null(await getPropertyValues(context.Entry((object)employee)));
            }
        }

        [Fact]
        public Task Store_values_really_are_store_values_not_current_or_original_values()
        {
            return Store_values_really_are_store_values_not_current_or_original_values_implementation(e => Task.FromResult(e.GetDatabaseValues()));
        }

        [Fact]
        public Task Store_values_really_are_store_values_not_current_or_original_values_async()
        {
            return Store_values_really_are_store_values_not_current_or_original_values_implementation(e => e.GetDatabaseValuesAsync());
        }

        private async Task Store_values_really_are_store_values_not_current_or_original_values_implementation(
            Func<EntityEntry, Task<PropertyValues>> getPropertyValues)
        {
            using (var context = CreateContext())
            {
                var building = context.Set<Building>().Single(b => b.Name == "Building One");
                building.Name = "Values End";

                context.Entry(building).State = EntityState.Unchanged;

                var storeValues = (Building)(await getPropertyValues(context.Entry(building))).ToObject();

                Assert.Equal("Building One", storeValues.Name);
            }
        }

        [Fact]
        public virtual void Setting_store_values_does_not_change_current_or_original_values()
        {
            using (var context = CreateContext())
            {
                var building = context.Set<Building>().Single(b => b.Name == "Building One");

                var storeValues = context.Entry(building).GetDatabaseValues();
                storeValues["Name"] = "Bag End";

                var currentValues = (Building)context.Entry(building).CurrentValues.ToObject();
                Assert.Equal("Building One", currentValues.Name);

                var originalValues = (Building)context.Entry(building).OriginalValues.ToObject();
                Assert.Equal("Building One", originalValues.Name);
            }
        }

        protected abstract class Employee : UnMappedPersonBase
        {
            public int EmployeeId { get; set; }
        }

        protected class Building
        {
            private Building()
            {
            }

            public static Building Create(Guid buildingId, string name, decimal value)
                => new Building
                {
                    BuildingId = buildingId,
                    Name = name,
                    Value = value
                };

            public Guid BuildingId { get; set; }
            public string Name { get; set; }
            public decimal Value { get; set; }
            public virtual ICollection<Office> Offices { get; } = new List<Office>();
            public virtual IList<MailRoom> MailRooms { get; } = new List<MailRoom>();

            public int? PrincipalMailRoomId { get; set; }
            public MailRoom PrincipalMailRoom { get; set; }

            public string NotInModel { get; set; }

            private string _noGetter = "NoGetter";

            public string NoGetter
            {
                set => _noGetter = value;
            }

            public string GetNoGetterValue() => _noGetter;

            public string NoSetter => "NoSetter";
        }

        protected class BuildingDto
        {
            public Guid BuildingId { get; set; }
            public string Name { get; set; }
            public decimal Value { get; set; }

            public int? PrincipalMailRoomId { get; set; }

            public string NotInModel { get; set; }

            private string _noGetter = "NoGetter";

            public string NoGetter
            {
                set => _noGetter = value;
            }

            public string GetNoGetterValue() => _noGetter;

            public string NoSetter => "NoSetter";

            public int Shadow1 { get; set; }
        }

        protected class BuildingDtoNoKey
        {
            public string Name { get; set; }
            public decimal Value { get; set; }
            public string Shadow2 { get; set; }
        }

        protected class MailRoom
        {
#pragma warning disable IDE1006 // Naming Styles
            public int id { get; set; }
#pragma warning restore IDE1006 // Naming Styles
            public Building Building { get; set; }
            public Guid BuildingId { get; set; }
        }

        protected class Office : UnMappedOfficeBase
        {
            public Guid BuildingId { get; set; }
            public Building Building { get; set; }
            public IList<Whiteboard> WhiteBoards { get; } = new List<Whiteboard>();
        }

        protected abstract class UnMappedOfficeBase
        {
            public string Number { get; set; }
            public string Description { get; set; }
        }

        protected class BuildingDetail
        {
            public Guid BuildingId { get; set; }
            public Building Building { get; set; }
            public string Details { get; set; }
        }

        protected class WorkOrder
        {
            public int WorkOrderId { get; set; }
            public int EmployeeId { get; set; }
            public Employee Employee { get; set; }
            public string Details { get; set; }
        }

        protected class Whiteboard
        {
#pragma warning disable IDE1006 // Naming Styles
            public byte[] iD { get; set; }
#pragma warning restore IDE1006 // Naming Styles
            public string AssetTag { get; set; }
            public Office Office { get; set; }
        }

        protected class UnMappedPersonBase
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }

        protected class UnMappedOffice : Office
        {
        }

        protected class CurrentEmployee : Employee
        {
            public CurrentEmployee Manager { get; set; }
            public decimal LeaveBalance { get; set; }
            public Office Office { get; set; }
        }

        protected class PastEmployee : Employee
        {
            public DateTime TerminationDate { get; set; }
        }

        protected DbContext CreateContext()
        {
            var context = Fixture.CreateContext();
            context.ChangeTracker.AutoDetectChangesEnabled = false;
            return context;
        }

        public abstract class PropertyValuesFixtureBase : SharedStoreFixtureBase<PoolableDbContext>
        {
            protected override string StoreName { get; } = "PropertyValues";

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                modelBuilder.Entity<Employee>(
                    b =>
                    {
                        b.Property(e => e.EmployeeId).ValueGeneratedNever();
                        b.Property<int>("Shadow1");
                        b.Property<string>("Shadow2");
                    });

                modelBuilder.Entity<CurrentEmployee>(b => b.Property<int>("Shadow3"));

                modelBuilder.Entity<PastEmployee>(b => b.Property<string>("Shadow4"));

                modelBuilder.Entity<Building>()
                    .HasOne<MailRoom>(nameof(Building.PrincipalMailRoom))
                    .WithMany()
                    .HasForeignKey(b => b.PrincipalMailRoomId);

                modelBuilder.Entity<MailRoom>()
                    .HasOne<Building>(nameof(MailRoom.Building))
                    .WithMany(nameof(Building.MailRooms))
                    .HasForeignKey(m => m.BuildingId);

                modelBuilder.Entity<Office>().HasKey(
                    o => new
                    {
                        o.Number,
                        o.BuildingId
                    });

                modelBuilder.Ignore<UnMappedOffice>();

                modelBuilder.Entity<BuildingDetail>(
                    b =>
                    {
                        b.HasKey(d => d.BuildingId);
                        b.HasOne(d => d.Building).WithOne().HasPrincipalKey<Building>(e => e.BuildingId);
                    });

                modelBuilder.Entity<Building>(
                    b =>
                    {
                        b.Ignore(e => e.NotInModel);
                        b.Property<int>("Shadow1");
                        b.Property<string>("Shadow2");
                    });
            }

            protected override void Seed(PoolableDbContext context)
            {
                var buildings = new List<Building>
                {
                    Building.Create(new Guid("21EC2020-3AEA-1069-A2DD-08002B30309D"), "Building One", 1500000),
                    Building.Create(Guid.NewGuid(), "Building Two", 1000000m)
                };

                foreach (var building in buildings)
                {
                    context.Add(building);
                }

                context.Entry(buildings[0]).Property("Shadow1").CurrentValue = 11;
                context.Entry(buildings[0]).Property("Shadow2").CurrentValue = "Meadow Drive";

                context.Entry(buildings[1]).Property("Shadow1").CurrentValue = 807;
                context.Entry(buildings[1]).Property("Shadow2").CurrentValue = "Onyx Circle";

                var offices = new List<Office>
                {
                    new Office
                    {
                        BuildingId = buildings[0].BuildingId,
                        Number = "1/1221"
                    },
                    new Office
                    {
                        BuildingId = buildings[0].BuildingId,
                        Number = "1/1223"
                    },
                    new Office
                    {
                        BuildingId = buildings[0].BuildingId,
                        Number = "2/1458"
                    },
                    new Office
                    {
                        BuildingId = buildings[0].BuildingId,
                        Number = "2/1789"
                    }
                };

                foreach (var office in offices)
                {
                    context.Add(office);
                }

                var employees = new List<Employee>
                {
                    new CurrentEmployee
                    {
                        EmployeeId = 1,
                        FirstName = "Rowan",
                        LastName = "Miller",
                        LeaveBalance = 45,
                        Office = offices[0]
                    },
                    new CurrentEmployee
                    {
                        EmployeeId = 2,
                        FirstName = "Arthur",
                        LastName = "Vickers",
                        LeaveBalance = 62,
                        Office = offices[1]
                    },
                    new PastEmployee
                    {
                        EmployeeId = 3,
                        FirstName = "John",
                        LastName = "Doe",
                        TerminationDate = new DateTime(2006, 1, 23)
                    }
                };

                context.Entry(employees[0]).Property("Shadow1").CurrentValue = 111;
                context.Entry(employees[0]).Property("Shadow2").CurrentValue = "PM";
                context.Entry(employees[0]).Property("Shadow3").CurrentValue = 1111;

                context.Entry(employees[1]).Property("Shadow1").CurrentValue = 222;
                context.Entry(employees[1]).Property("Shadow2").CurrentValue = "SDE";
                context.Entry(employees[1]).Property("Shadow3").CurrentValue = 11112;

                context.Entry(employees[2]).Property("Shadow1").CurrentValue = 333;
                context.Entry(employees[2]).Property("Shadow2").CurrentValue = "SDET";
                context.Entry(employees[2]).Property("Shadow4").CurrentValue = "BSC";

                foreach (var employee in employees)
                {
                    context.Add(employee);
                }

                var whiteboards = new List<Whiteboard>
                {
                    new Whiteboard
                    {
                        AssetTag = "WB1973",
                        iD = new byte[] { 1, 9, 7, 3 },
                        Office = offices[0]
                    },
                    new Whiteboard
                    {
                        AssetTag = "WB1977",
                        iD = new byte[] { 1, 9, 7, 7 },
                        Office = offices[0]
                    },
                    new Whiteboard
                    {
                        AssetTag = "WB1970",
                        iD = new byte[] { 1, 9, 7, 0 },
                        Office = offices[2]
                    }
                };

                foreach (var whiteboard in whiteboards)
                {
                    context.Add(whiteboard);
                }

                context.SaveChanges();
            }
        }
    }
}
