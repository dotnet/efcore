// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.ComplexProperties;

public abstract class ComplexPropertiesMiscellaneousTestBase<TFixture>(TFixture fixture)
    : RelationshipsMiscellaneousTestBase<TFixture>(fixture)
        where TFixture : ComplexPropertiesFixtureBase, new();
