// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.SqlServer.Scaffolding.Internal;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Scaffolding
{
    public class SqlServerCodeGeneratorTest
    {
        [ConditionalFact]
        public virtual void Use_provider_method_is_generated_correctly()
        {
            var codeGenerator = new SqlServerCodeGenerator(
                new ProviderCodeGeneratorDependencies(
                    Enumerable.Empty<IProviderCodeGeneratorPlugin>()));

            var result = codeGenerator.GenerateUseProvider("Data Source=Test", providerOptions: null);

            Assert.Equal("UseSqlServer", result.Method);
            Assert.Collection(
                result.Arguments,
                a => Assert.Equal("Data Source=Test", a));
            Assert.Null(result.ChainedCall);
        }

        [ConditionalFact]
        public virtual void Use_provider_method_is_generated_correctly_with_options()
        {
            var codeGenerator = new SqlServerCodeGenerator(
                new ProviderCodeGeneratorDependencies(
                    Enumerable.Empty<IProviderCodeGeneratorPlugin>()));

            var providerOptions = new MethodCallCodeFragment("SetProviderOption");

            var result = codeGenerator.GenerateUseProvider("Data Source=Test", providerOptions);

            Assert.Equal("UseSqlServer", result.Method);
            Assert.Collection(
                result.Arguments,
                a => Assert.Equal("Data Source=Test", a),
                a =>
                {
                    var nestedClosure = Assert.IsType<NestedClosureCodeFragment>(a);

                    Assert.Equal("x", nestedClosure.Parameter);
                    Assert.Same(providerOptions, nestedClosure.MethodCall);
                });
            Assert.Null(result.ChainedCall);
        }
    }
}
