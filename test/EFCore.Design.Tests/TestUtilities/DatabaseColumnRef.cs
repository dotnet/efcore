// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

internal class DatabaseColumnRef : DatabaseColumn
{
    public DatabaseColumnRef(string name)
    {
        Name = name;
    }

    public override DatabaseTable Table
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public override bool IsNullable
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public override string StoreType
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public override string DefaultValueSql
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public override string ComputedColumnSql
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public override string Comment
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public override ValueGenerated? ValueGenerated
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }
}
