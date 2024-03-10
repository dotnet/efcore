// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class ComplexTypesTrackingSqlServerTest(
        ComplexTypesTrackingSqlServerTest.SqlServerFixture fixture,
        ITestOutputHelper testOutputHelper)
    : ComplexTypesTrackingSqlServerTestBase<ComplexTypesTrackingSqlServerTest.SqlServerFixture>(fixture, testOutputHelper)
{
    public class SqlServerFixture : SqlServerFixtureBase
    {
        protected override string StoreName
            => nameof(ComplexTypesTrackingSqlServerTest);
    }
}

public class ComplexTypesTrackingProxiesSqlServerTest(
        ComplexTypesTrackingProxiesSqlServerTest.SqlServerFixture fixture,
        ITestOutputHelper testOutputHelper)
    : ComplexTypesTrackingSqlServerTestBase<ComplexTypesTrackingProxiesSqlServerTest.SqlServerFixture>(fixture, testOutputHelper)
{
    // Fields can't be proxied
    public override Task Can_track_entity_with_complex_objects_with_fields(EntityState state, bool async)
        => Task.CompletedTask;

    // Fields can't be proxied
    public override void Can_mark_complex_type_properties_modified_with_fields(bool trackFromQuery)
    {
    }

    // Fields can't be proxied
    public override void Can_read_original_values_for_properties_of_complex_types_with_fields(bool trackFromQuery)
    {
    }

    // Fields can't be proxied
    public override void Can_write_original_values_for_properties_of_complex_types_with_fields(bool trackFromQuery)
    {
    }

    // Fields can't be proxied
    public override Task Can_track_entity_with_complex_structs_with_fields(EntityState state, bool async)
        => Task.CompletedTask;

    // Fields can't be proxied
    public override void Can_mark_complex_readonly_struct_properties_modified_with_fields(bool trackFromQuery)
    {
    }

    // Fields can't be proxied
    public override void Can_read_original_values_for_properties_of_structs_with_fields(bool trackFromQuery)
    {
    }

    // Fields can't be proxied
    public override void Can_write_original_values_for_properties_of_structs_with_fields(bool trackFromQuery)
    {
    }

    // Fields can't be proxied
    public override Task Can_track_entity_with_complex_readonly_structs_with_fields(EntityState state, bool async)
        => Task.CompletedTask;

    // Fields can't be proxied
    public override void Can_mark_complex_readonly_readonly_struct_properties_modified_with_fields(bool trackFromQuery)
    {
    }

    // Fields can't be proxied
    public override void Can_read_original_values_for_properties_of_readonly_structs_with_fields(bool trackFromQuery)
    {
    }

    // Fields can't be proxied
    public override void Can_write_original_values_for_properties_of_readonly_structs_with_fields(bool trackFromQuery)
    {
    }

    // Fields can't be proxied
    public override Task Can_track_entity_with_complex_record_objects_with_fields(EntityState state, bool async)
        => Task.CompletedTask;

    // Fields can't be proxied
    public override void Can_mark_complex_record_type_properties_modified_with_fields(bool trackFromQuery)
    {
    }

    // Fields can't be proxied
    public override void Can_read_original_values_for_properties_of_record_complex_types_with_fields(bool trackFromQuery)
    {
    }

    // Fields can't be proxied
    public override void Can_write_original_values_for_properties_of_record_complex_types_with_fields(bool trackFromQuery)
    {
    }

    public class SqlServerFixture : SqlServerFixtureBase
    {
        protected override string StoreName
            => nameof(ComplexTypesTrackingProxiesSqlServerTest);

        public override bool UseProxies
            => true;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder.UseLazyLoadingProxies().UseChangeTrackingProxies());

        protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
            => base.AddServices(serviceCollection.AddEntityFrameworkProxies());
    }
}

public abstract class ComplexTypesTrackingSqlServerTestBase<TFixture> : ComplexTypesTrackingTestBase<TFixture>
    where TFixture : ComplexTypesTrackingSqlServerTestBase<TFixture>.SqlServerFixtureBase, new()
{
    protected ComplexTypesTrackingSqlServerTestBase(TFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        fixture.TestSqlLoggerFactory.Clear();
        fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());

    public abstract class SqlServerFixtureBase : FixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;
    }
}
