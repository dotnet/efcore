// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.XuGu.Storage.Internal;

public class XGSByteTypeMapping : SByteTypeMapping
{
    public static new XGSByteTypeMapping Default { get; } = new("tinyint");

    public XGSByteTypeMapping(
        string storeType,
        DbType? dbType = System.Data.DbType.SByte)
        : base(storeType, dbType)
    {
    }

    protected XGSByteTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new XGSByteTypeMapping(parameters);
}
