// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

internal class DatabaseTableRef : DatabaseTable
{
    public DatabaseTableRef(string name, string schema = null)
    {
        Name = name;
        Schema = schema;
    }

    public override DatabaseModel Database
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public override string Comment
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public override DatabasePrimaryKey PrimaryKey
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public override IList<DatabaseColumn> Columns
        => throw new NotImplementedException();

    public override IList<DatabaseUniqueConstraint> UniqueConstraints
        => throw new NotImplementedException();

    public override IList<DatabaseIndex> Indexes
        => throw new NotImplementedException();

    public override IList<DatabaseForeignKey> ForeignKeys
        => throw new NotImplementedException();
}
