﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
                    Assert.Same(providerOptions, nestedClosure.MethodCalls[0]);
                });
            Assert.Null(result.ChainedCall);
        }
    }
}
