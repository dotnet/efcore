// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Sqlite.Scaffolding.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Scaffolding
{
    public class SqliteCodeGeneratorTest
    {
        [Fact]
        public virtual void Use_provider_method_is_generated_correctly()
        {
            var codeGenerator = new SqliteCodeGenerator(new ProviderCodeGeneratorDependencies());

            var result = codeGenerator.GenerateUseProvider("Data Source=Test");

            Assert.Equal("UseSqlite", result.Method);
            Assert.Collection(
                result.Arguments,
                a => Assert.Equal("Data Source=Test", a));
        }
    }
}
