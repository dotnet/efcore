// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.OwnedComplexProperties;

public class OwnedComplexPropertiesCollectionSqliteTest(OwnedComplexPropertiesSqliteFixture fixture, ITestOutputHelper testOutputHelper)
    : OwnedComplexPropertiesCollectionRelationalTestBase<OwnedComplexPropertiesSqliteFixture>(fixture, testOutputHelper);
