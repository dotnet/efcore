// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.BasicTypesModel;

namespace Microsoft.EntityFrameworkCore.Query.Translations;

public abstract class ByteArrayTranslationsTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : BasicTypesQueryFixtureBase, new()
{
    [ConditionalFact]
    public virtual Task Length()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(e => e.ByteArray.Length == 4));

    [ConditionalFact]
    public virtual Task Index()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(e => e.ByteArray.Length >= 3 && e.ByteArray[2] == 0xBE));

    [ConditionalFact]
    public virtual Task First()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(e => e.ByteArray.Length >= 1 && e.ByteArray.First() == 0xDE));

    [ConditionalFact]
    public virtual Task Contains_with_constant()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(s => s.ByteArray.Contains((byte)1)));

    [ConditionalFact]
    public virtual Task Contains_with_parameter()
    {
        byte someByte = 1;

        return AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(s => s.ByteArray.Contains(someByte)));
    }

    [ConditionalFact]
    public virtual Task Contains_with_column()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(s => s.ByteArray.Contains(s.Byte)));

    [ConditionalFact]
    public virtual Task SequenceEqual()
    {
        var byteArrayParam = new byte[] { 0xDE, 0xAD, 0xBE, 0xEF };

        return AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(s => s.ByteArray.SequenceEqual(byteArrayParam)));
    }
}
