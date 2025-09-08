// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.XuGu.Storage.Internal;

public class XGShortTypeMapping : ShortTypeMapping
{
    public static new XGShortTypeMapping Default { get; } = new("smallint");

    public XGShortTypeMapping(
        string storeType,
        DbType? dbType = System.Data.DbType.Int16)
        : base(storeType, dbType)
    {
    }

    protected XGShortTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new XGShortTypeMapping(parameters);
}
