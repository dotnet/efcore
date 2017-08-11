// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Scaffolding
{
    public class SqlServerScaffoldingCodeGeneratorTest
    {
        [Theory]
        [InlineData("dummy", ".UseSqlServer(@\"dummy\")")]
        [InlineData("du\"mmy", ".UseSqlServer(@\"du\"\"mmy\")")]
        public virtual void Use_provider_method_is_generated_correctly(string connectionstring, string output)
        {
            var codeGenerator = new SqlServerScaffoldingCodeGenerator();

            Assert.Equal(output, codeGenerator.GenerateUseProvider(connectionstring, "CSharp"));
        }

        [Fact]
        public virtual void Use_provider_method_is_generated_correctly_with_new_line()
        {
            var codeGenerator = new SqlServerScaffoldingCodeGenerator();

            Assert.Equal(".UseSqlServer(@\"du" + Environment.NewLine + "mmy\")", codeGenerator.GenerateUseProvider("du" + Environment.NewLine + "mmy", "CSharp"));
        }
    }
}
