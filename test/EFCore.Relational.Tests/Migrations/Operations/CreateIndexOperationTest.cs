// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Migrations.Operations;

public class CreateIndexOperationTest
{
    [ConditionalFact]
    public void IsDescending_count_matches_column_count()
    {
        var operation = new CreateIndexOperation();

        operation.IsDescending = [true];
        Assert.Throws<ArgumentException>(() => operation.Columns = ["X", "Y"]);

        operation.IsDescending = null;

        operation.Columns = ["X", "Y"];
        Assert.Throws<ArgumentException>(() => operation.IsDescending = [true]);
    }

    [ConditionalFact]
    public void IsDescending_accepts_empty_array()
    {
        var operation = new CreateIndexOperation();

        operation.IsDescending = [];
        operation.Columns = ["X", "Y"];

        operation.IsDescending = null;
        operation.IsDescending = [];
    }
}
