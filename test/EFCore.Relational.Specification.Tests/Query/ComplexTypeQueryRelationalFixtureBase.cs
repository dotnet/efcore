// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public abstract class ComplexTypeQueryRelationalFixtureBase : ComplexTypeQueryFixtureBase
{
//     // TODO: Temporarily seeding via raw SQL as update pipeline/change tracking support for complex types is still missing.
//     protected override void Seed(PoolableDbContext context)
//         => context.Database.ExecuteSqlRaw(
// """
// INSERT INTO Customer (Id, Name, ShippingAddress_AddressLine1, ShippingAddress_ZipCode, ShippingAddress_Country_FullName, ShippingAddress_Country_Code, BillingAddress_AddressLine1, BillingAddress_ZipCode, BillingAddress_Country_FullName, BillingAddress_Country_Code)
// VALUES
//     (1, 'Mona Cy', '804 S. Lakeshore Road', 38654, 'United States', 'US', '804 S. Lakeshore Road', 38654, 'United States', 'US'),
//     (2, 'Antigonus Mitul', '72 Hickory Rd.', 07728, 'Germany', 'DE', '79 Main St.', 29293, 'Germany', 'DE'),
//     (3, 'Monty Elias', '79 Main St.', 29293, 'Germany', 'DE', '79 Main St.', 29293, 'Germany', 'DE');
//
// INSERT INTO CustomerGroup (Id, RequiredCustomerId, OptionalCustomerId)
// VALUES
//     (1, 1, 2),
//     (2, 2, 1),
//     (3, 1, NULL);
// """);

    public TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;
}
