// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public class RelationalSqlGenerationHelperTest
    {
        [ConditionalFact]
        public void GenerateParameterName_returns_parameter_name()
            => Assert.Equal("@name", CreateSqlGenerationHelper().GenerateParameterName("name"));

        [ConditionalFact]
        public void Default_BatchCommandSeparator_is_semicolon()
            => Assert.Equal(";", CreateSqlGenerationHelper().StatementTerminator);

        [ConditionalFact]
        public void BatchSeparator_returns_separator()
            => Assert.Equal(Environment.NewLine, CreateSqlGenerationHelper().BatchTerminator);

        private ISqlGenerationHelper CreateSqlGenerationHelper()
            => new RelationalSqlGenerationHelper(new RelationalSqlGenerationHelperDependencies());
    }
}
