// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Sqlite.Scaffolding.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Scaffolding
{
    public class SqliteCodeGeneratorTest
    {
        [ConditionalFact]
        public virtual void Use_provider_method_is_generated_correctly()
        {
            var codeGenerator = new SqliteCodeGenerator(
                new ProviderCodeGeneratorDependencies(
                    Enumerable.Empty<IProviderCodeGeneratorPlugin>()));

            var result = codeGenerator.GenerateUseProvider("Data Source=Test", providerOptions: null);

            Assert.Equal("UseSqlite", result.Method);
            Assert.Collection(
                result.Arguments,
                a => Assert.Equal("Data Source=Test", a));
            Assert.Null(result.ChainedCall);
        }

        [ConditionalFact]
        public virtual void Use_provider_method_is_generated_correctly_with_options()
        {
            var codeGenerator = new SqliteCodeGenerator(
                new ProviderCodeGeneratorDependencies(
                    Enumerable.Empty<IProviderCodeGeneratorPlugin>()));

            var providerOptions = new MethodCallCodeFragment(_setProviderOptionMethodInfo);

            var result = codeGenerator.GenerateUseProvider("Data Source=Test", providerOptions);

            Assert.Equal("UseSqlite", result.Method);
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

        private static readonly MethodInfo _setProviderOptionMethodInfo
            = typeof(SqliteCodeGeneratorTest).GetRuntimeMethod(nameof(SetProviderOption), new[] { typeof(DbContextOptionsBuilder) });

        public static SqliteDbContextOptionsBuilder SetProviderOption(DbContextOptionsBuilder optionsBuilder)
            => throw new NotSupportedException();
    }
}
