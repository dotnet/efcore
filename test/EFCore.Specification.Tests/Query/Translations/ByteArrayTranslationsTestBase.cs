// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.BasicTypesModel;

namespace Microsoft.EntityFrameworkCore.Query.Translations;

public abstract class ByteArrayTranslationsTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : BasicTypesQueryFixtureBase, new()
{
    [Fact]
    public virtual Task Length()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(e => e.ByteArray.Length == 4));

    [Fact]
    public virtual Task Index()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(e => e.ByteArray.Length >= 3 && e.ByteArray[2] == 0xBE));

    [Fact]
    public virtual Task First()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(e => e.ByteArray.Length >= 1 && e.ByteArray.First() == 0xDE));

    [Fact]
    public virtual Task Contains_with_constant()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(s => s.ByteArray.Contains((byte)1)));

    [Fact]
    public virtual Task Contains_with_parameter()
    {
        byte someByte = 1;

        return AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(s => s.ByteArray.Contains(someByte)));
    }

    [Fact]
    public virtual Task Contains_with_column()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(s => s.ByteArray.Contains(s.Byte)));

    [Fact]
    public virtual Task Contains_with_parameter_above_ascii_range()
    {
        byte someByte = 0xDE;

        return AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(s => s.ByteArray.Contains(someByte)));
    }

    [Fact]
    public virtual Task IndexOf_with_constant()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(e => Array.IndexOf(e.ByteArray, (byte)0xBE) == 2));

    [Fact]
    public virtual Task IndexOf_with_parameter()
    {
        byte someByte = 3;

        return AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(e => Array.IndexOf(e.ByteArray, someByte) == 2));
    }

    [Fact]
    public virtual Task IndexOf_with_column()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(e => Array.IndexOf(e.ByteArray, e.Byte) >= 0));

    [Fact]
    public virtual Task IndexOf_with_parameter_above_ascii_range()
    {
        byte someByte = 0xDE;

        return AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(e => Array.IndexOf(e.ByteArray, someByte) == 0));
    }

    [Fact]
    public virtual Task IndexOf_not_found()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(e => Array.IndexOf(e.ByteArray, (byte)0x99) == -1));

    [Fact]
    public virtual Task Any()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(e => e.ByteArray.Any()));

    [Fact]
    public virtual Task SequenceEqual()
    {
        var byteArrayParam = new byte[] { 0xDE, 0xAD, 0xBE, 0xEF };

        return AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(s => s.ByteArray.SequenceEqual(byteArrayParam)));
    }
}
