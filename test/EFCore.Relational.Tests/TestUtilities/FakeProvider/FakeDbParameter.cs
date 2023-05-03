// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;

namespace Microsoft.EntityFrameworkCore.TestUtilities.FakeProvider;

public class FakeDbParameter : DbParameter
{
    public override string ParameterName { get; set; }

    public override object Value { get; set; }

    public override ParameterDirection Direction { get; set; }

    public static bool DefaultIsNullable = false;
    public override bool IsNullable { get; set; } = DefaultIsNullable;

    public static DbType DefaultDbType = DbType.AnsiString;
    public override DbType DbType { get; set; } = DefaultDbType;

    public override int Size { get; set; }

    public override string SourceColumn
    {
        get => throw new NotImplementedException();

        set => throw new NotImplementedException();
    }

    public override bool SourceColumnNullMapping
    {
        get => throw new NotImplementedException();

        set => throw new NotImplementedException();
    }

    public override void ResetDbType()
        => throw new NotImplementedException();
}
