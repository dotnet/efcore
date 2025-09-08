// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.XuGu.Storage.Internal;

public class XGLongTypeMapping : LongTypeMapping
{
    public static new XGLongTypeMapping Default { get; } = new("bigint");

    public XGLongTypeMapping(
        string storeType,
        DbType? dbType = System.Data.DbType.Int64)
        : base(storeType, dbType)
    {
    }

    protected XGLongTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new XGLongTypeMapping(parameters);
}
