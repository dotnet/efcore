// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.BasicTypesModel;

namespace Microsoft.EntityFrameworkCore.Query.Translations;

public abstract class ByteArrayTranslationsTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : BasicTypesQueryFixtureBase, new()
{
    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Length(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(e => e.ByteArray.Length == 4));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Index(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(e => e.ByteArray.Length >= 3 && e.ByteArray[2] == 0xBE));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task First(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(e => e.ByteArray.Length >= 1 && e.ByteArray.First() == 0xDE));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_with_constant(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(s => s.ByteArray.Contains((byte)1)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_with_parameter(bool async)
    {
        byte someByte = 1;

        return AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(s => s.ByteArray.Contains(someByte)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_with_column(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(s => s.ByteArray.Contains(s.Byte)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SequenceEqual(bool async)
    {
        var byteArrayParam = new byte[] { 0xDE, 0xAD, 0xBE, 0xEF };

        return AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(s => s.ByteArray.SequenceEqual(byteArrayParam)));
    }
}
