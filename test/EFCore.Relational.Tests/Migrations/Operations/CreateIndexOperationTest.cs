// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Migrations.Operations;

public class CreateIndexOperationTest
{
    [Fact]
    public void IsDescending_count_matches_column_count()
    {
        var operation = new CreateIndexOperation();

        operation.IsDescending = new[] { true };
        Assert.Throws<ArgumentException>(() => operation.Columns = new[] { "X", "Y" });

        operation.IsDescending = null;

        operation.Columns = new[] { "X", "Y" };
        Assert.Throws<ArgumentException>(() => operation.IsDescending = new[] { true });
    }
}
