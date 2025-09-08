// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.XuGu.Storage.Internal;

public class XGUIntTypeMapping : UIntTypeMapping
{
    public static new XGUIntTypeMapping Default { get; } = new("int unsigned");

    public XGUIntTypeMapping(
        string storeType,
        DbType? dbType = System.Data.DbType.UInt32)
        : base(storeType, dbType)
    {
    }

    protected XGUIntTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new XGUIntTypeMapping(parameters);
}
