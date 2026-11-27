// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.OwnedComplexProperties;

public abstract class OwnedComplexPropertiesMiscellaneousTestBase<TFixture>(TFixture fixture)
    : AssociationsMiscellaneousTestBase<TFixture>(fixture)
    where TFixture : OwnedComplexPropertiesFixtureBase, new();
