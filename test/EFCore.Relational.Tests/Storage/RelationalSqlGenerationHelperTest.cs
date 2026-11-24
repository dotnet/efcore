// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Storage;

public class RelationalSqlGenerationHelperTest
{
    [Fact]
    public void GenerateParameterName_returns_parameter_name()
        => Assert.Equal("@name", CreateSqlGenerationHelper().GenerateParameterName("name"));

    [Fact]
    public void Default_BatchCommandSeparator_is_semicolon()
        => Assert.Equal(";", CreateSqlGenerationHelper().StatementTerminator);

    [Fact]
    public void BatchSeparator_returns_separator()
        => Assert.Equal(Environment.NewLine, CreateSqlGenerationHelper().BatchTerminator);

    private ISqlGenerationHelper CreateSqlGenerationHelper()
        => new RelationalSqlGenerationHelper(new RelationalSqlGenerationHelperDependencies());
}
