// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.OwnedComplexProperties;

public class OwnedComplexPropertiesStructuralEqualitySqliteTest(OwnedComplexPropertiesSqliteFixture fixture, ITestOutputHelper testOutputHelper)
    : OwnedComplexPropertiesStructuralEqualityRelationalTestBase<OwnedComplexPropertiesSqliteFixture>(fixture, testOutputHelper);
