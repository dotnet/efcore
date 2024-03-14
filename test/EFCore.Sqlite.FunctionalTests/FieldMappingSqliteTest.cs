// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public abstract class FieldMappingSqliteTest
{
    public abstract class FieldMappingSqliteTestBase<TFixture> : FieldMappingTestBase<TFixture>
        where TFixture : FieldMappingSqliteTestBase<TFixture>.FieldMappingSqliteFixtureBase, new()
    {
        protected FieldMappingSqliteTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
            => facade.UseTransaction(transaction.GetDbTransaction());

        public abstract class FieldMappingSqliteFixtureBase : FieldMappingFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory
                => SqliteTestStoreFactory.Instance;
        }
    }

    public class DefaultMappingTest(DefaultMappingTest.DefaultMappingFixture fixture)
        : FieldMappingSqliteTestBase<DefaultMappingTest.DefaultMappingFixture>(fixture)
    {
        public class DefaultMappingFixture : FieldMappingSqliteFixtureBase;
    }

    public class EnforceFieldTest(EnforceFieldTest.EnforceFieldFixture fixture)
        : FieldMappingSqliteTestBase<EnforceFieldTest.EnforceFieldFixture>(fixture)
    {
        public class EnforceFieldFixture : FieldMappingSqliteFixtureBase
        {
            protected override string StoreName
                => "FieldMappingEnforceFieldTest";

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                modelBuilder.UsePropertyAccessMode(PropertyAccessMode.Field);
                base.OnModelCreating(modelBuilder, context);
            }
        }
    }

    public class EnforceFieldForQueryTest(EnforceFieldForQueryTest.EnforceFieldForQueryFixture fixture)
        : FieldMappingSqliteTestBase<EnforceFieldForQueryTest.EnforceFieldForQueryFixture>(fixture)
    {
        public class EnforceFieldForQueryFixture : FieldMappingSqliteFixtureBase
        {
            protected override string StoreName
                => "FieldMappingFieldQueryTest";

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                modelBuilder.UsePropertyAccessMode(PropertyAccessMode.FieldDuringConstruction);
                base.OnModelCreating(modelBuilder, context);
            }
        }
    }

    public class EnforcePropertyTest(EnforcePropertyTest.EnforcePropertyFixture fixture)
        : FieldMappingSqliteTestBase<EnforcePropertyTest.EnforcePropertyFixture>(fixture)
    {

        // Cannot force property access when properties missing getter/setter
        public override void Simple_query_read_only_props(bool tracking)
        {
        }

        public override void Include_collection_read_only_props(bool tracking)
        {
        }

        public override void Include_reference_read_only_props(bool tracking)
        {
        }

        public override void Load_collection_read_only_props()
        {
        }

        public override void Load_reference_read_only_props()
        {
        }

        public override void Query_with_conditional_constant_read_only_props(bool tracking)
        {
        }

        public override void Query_with_conditional_param_read_only_props(bool tracking)
        {
        }

        public override void Projection_read_only_props(bool tracking)
        {
        }

        public override Task Update_read_only_props()
            => Task.CompletedTask;

        public override void Simple_query_read_only_props_with_named_fields(bool tracking)
        {
        }

        public override void Include_collection_read_only_props_with_named_fields(bool tracking)
        {
        }

        public override void Include_reference_read_only_props_with_named_fields(bool tracking)
        {
        }

        public override void Load_collection_read_only_props_with_named_fields()
        {
        }

        public override void Load_reference_read_only_props_with_named_fields()
        {
        }

        public override void Query_with_conditional_constant_read_only_props_with_named_fields(bool tracking)
        {
        }

        public override void Query_with_conditional_param_read_only_props_with_named_fields(bool tracking)
        {
        }

        public override void Projection_read_only_props_with_named_fields(bool tracking)
        {
        }

        public override Task Update_read_only_props_with_named_fields()
            => Task.CompletedTask;

        public override void Simple_query_write_only_props(bool tracking)
        {
        }

        public override void Include_collection_write_only_props(bool tracking)
        {
        }

        public override void Include_reference_write_only_props(bool tracking)
        {
        }

        public override void Load_collection_write_only_props()
        {
        }

        public override void Load_reference_write_only_props()
        {
        }

        public override void Query_with_conditional_constant_write_only_props(bool tracking)
        {
        }

        public override void Query_with_conditional_param_write_only_props(bool tracking)
        {
        }

        public override void Projection_write_only_props(bool tracking)
        {
        }

        public override Task Update_write_only_props()
            => Task.CompletedTask;

        public override void Simple_query_write_only_props_with_named_fields(bool tracking)
        {
        }

        public override void Include_collection_write_only_props_with_named_fields(bool tracking)
        {
        }

        public override void Include_reference_write_only_props_with_named_fields(bool tracking)
        {
        }

        public override void Load_collection_write_only_props_with_named_fields()
        {
        }

        public override void Load_reference_write_only_props_with_named_fields()
        {
        }

        public override void Query_with_conditional_constant_write_only_props_with_named_fields(bool tracking)
        {
        }

        public override void Query_with_conditional_param_write_only_props_with_named_fields(bool tracking)
        {
        }

        public override void Projection_write_only_props_with_named_fields(bool tracking)
        {
        }

        public override Task Update_write_only_props_with_named_fields()
            => Task.CompletedTask;

        public override void Simple_query_fields_only(bool tracking)
        {
        }

        public override void Include_collection_fields_only(bool tracking)
        {
        }

        public override void Include_reference_fields_only(bool tracking)
        {
        }

        public override void Load_collection_fields_only()
        {
        }

        public override void Load_reference_fields_only()
        {
        }

        public override void Query_with_conditional_constant_fields_only(bool tracking)
        {
        }

        public override void Query_with_conditional_param_fields_only(bool tracking)
        {
        }

        public override void Projection_fields_only(bool tracking)
        {
        }

        public override Task Update_fields_only()
            => Task.CompletedTask;

        public override void Simple_query_fields_only_for_navs_too(bool tracking)
        {
        }

        public override void Include_collection_fields_only_for_navs_too(bool tracking)
        {
        }

        public override void Include_reference_fields_only_only_for_navs_too(bool tracking)
        {
        }

        public override void Load_collection_fields_only_only_for_navs_too()
        {
        }

        public override void Load_reference_fields_only_only_for_navs_too()
        {
        }

        public override void Query_with_conditional_constant_fields_only_only_for_navs_too(bool tracking)
        {
        }

        public override void Query_with_conditional_param_fields_only_only_for_navs_too(bool tracking)
        {
        }

        public override void Projection_fields_only_only_for_navs_too(bool tracking)
        {
        }

        public override Task Update_fields_only_only_for_navs_too()
            => Task.CompletedTask;

        public override void Include_collection_full_props(bool tracking)
        {
        }

        public override void Include_reference_full_props(bool tracking)
        {
        }

        public override void Load_collection_full_props()
        {
        }

        public override void Load_reference_full_props()
        {
        }

        public override Task Update_full_props()
            => Task.CompletedTask;

        public override void Simple_query_props_with_IReadOnlyCollection(bool tracking)
        {
        }

        public override void Include_collection_props_with_IReadOnlyCollection(bool tracking)
        {
        }

        public override void Include_reference_props_with_IReadOnlyCollection(bool tracking)
        {
        }

        public override void Load_collection_props_with_IReadOnlyCollection()
        {
        }

        public override void Load_reference_props_with_IReadOnlyCollection()
        {
        }

        public override void Query_with_conditional_constant_props_with_IReadOnlyCollection(bool tracking)
        {
        }

        public override void Query_with_conditional_param_props_with_IReadOnlyCollection(bool tracking)
        {
        }

        public override void Projection_props_with_IReadOnlyCollection(bool tracking)
        {
        }

        public override Task Update_props_with_IReadOnlyCollection()
            => Task.CompletedTask;

        public class EnforcePropertyFixture : FieldMappingSqliteFixtureBase
        {
            protected override string StoreName
                => "FieldMappingEnforcePropertyTest";

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                modelBuilder.UsePropertyAccessMode(PropertyAccessMode.Property);
                base.OnModelCreating(modelBuilder, context);
            }
        }
    }
}
