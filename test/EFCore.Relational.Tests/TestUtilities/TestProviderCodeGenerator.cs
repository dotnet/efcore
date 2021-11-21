// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Scaffolding;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class TestProviderCodeGenerator : ProviderCodeGenerator
{
    public TestProviderCodeGenerator(ProviderCodeGeneratorDependencies dependencies)
        : base(dependencies)
    {
    }

    public override MethodCallCodeFragment GenerateUseProvider(
        string connectionString,
        MethodCallCodeFragment providerOptions)
        => new(
            _useTestProviderMethodInfo,
            providerOptions == null
                ? new object[] { connectionString }
                : new object[] { connectionString, new NestedClosureCodeFragment("x", providerOptions) });

    private static readonly MethodInfo _useTestProviderMethodInfo
        = typeof(TestProviderCodeGenerator).GetRequiredRuntimeMethod(
            nameof(UseTestProvider), typeof(DbContextOptionsBuilder), typeof(string), typeof(Action<object>));

    public static void UseTestProvider(
        DbContextOptionsBuilder optionsBuilder,
        string connectionString,
        Action<object> optionsAction = null)
        => throw new NotSupportedException();
}
