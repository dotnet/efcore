// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.XuGu.Storage.Internal;

public class XGUShortTypeMapping : UShortTypeMapping
{
    public static new XGUShortTypeMapping Default { get; } = new("smallint unsigned");

    public XGUShortTypeMapping(
        string storeType,
        DbType? dbType = System.Data.DbType.UInt16)
        : base(storeType, dbType)
    {
    }

    protected XGUShortTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new XGUShortTypeMapping(parameters);
}
