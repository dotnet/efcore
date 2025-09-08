// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.XuGu.Storage.Internal;

public class XGIntTypeMapping : IntTypeMapping
{
    public static new XGIntTypeMapping Default { get; } = new("int");

    public XGIntTypeMapping(
        string storeType,
        DbType? dbType = System.Data.DbType.Int32)
        : base(storeType, dbType)
    {
    }

    protected XGIntTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new XGIntTypeMapping(parameters);
}
