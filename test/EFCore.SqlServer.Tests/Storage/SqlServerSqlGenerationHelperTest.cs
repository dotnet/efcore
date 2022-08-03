// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.Storage;

public class SqlServerSqlGenerationHelperTest
{
    [ConditionalFact]
    public void BatchSeparator_returns_separator()
        => Assert.Equal("GO" + Environment.NewLine + Environment.NewLine, CreateSqlGenerationHelper().BatchTerminator);

    private ISqlGenerationHelper CreateSqlGenerationHelper()
        => new SqlServerSqlGenerationHelper(new RelationalSqlGenerationHelperDependencies());
}
