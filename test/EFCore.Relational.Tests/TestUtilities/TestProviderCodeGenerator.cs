// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class TestProviderCodeGenerator(ProviderCodeGeneratorDependencies dependencies) : ProviderCodeGenerator(dependencies)
{
    public override MethodCallCodeFragment GenerateUseProvider(
        string connectionString,
        MethodCallCodeFragment providerOptions)
        => new(
            _useTestProviderMethodInfo,
            providerOptions == null
                ? [connectionString]
                : [connectionString, new NestedClosureCodeFragment("x", providerOptions)]);

    private static readonly MethodInfo _useTestProviderMethodInfo
        = typeof(TestProviderCodeGenerator).GetRuntimeMethod(
            nameof(UseTestProvider), [typeof(DbContextOptionsBuilder), typeof(string), typeof(Action<object>)])!;

    public static void UseTestProvider(
        DbContextOptionsBuilder optionsBuilder,
        string connectionString,
        Action<object> optionsAction = null)
        => throw new NotSupportedException();
}
