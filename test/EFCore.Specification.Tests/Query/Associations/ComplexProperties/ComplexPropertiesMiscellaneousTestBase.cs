// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.ComplexProperties;

public abstract class ComplexPropertiesMiscellaneousTestBase<TFixture>(TFixture fixture)
    : AssociationsMiscellaneousTestBase<TFixture>(fixture)
    where TFixture : ComplexPropertiesFixtureBase, new()
{
    #region Value types

    [ConditionalFact]
    public virtual Task Where_property_on_non_nullable_value_type()
        => AssertQuery(ss => ss.Set<ValueRootEntity>().Where(e => e.RequiredRelated.Int == 8));

    [ConditionalFact]
    public virtual Task Where_property_on_nullable_value_type_Value()
        => AssertQuery(
            ss => ss.Set<ValueRootEntity>().Where(e => e.OptionalRelated!.Value.Int == 8),
            ss => ss.Set<ValueRootEntity>().Where(e => e.OptionalRelated.HasValue && e.OptionalRelated!.Value.Int == 8));

    [ConditionalFact]
    public virtual Task Where_HasValue_on_nullable_value_type()
        => AssertQuery(ss => ss.Set<ValueRootEntity>().Where(e => e.OptionalRelated.HasValue));

    #endregion Value types
}
