// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit;

namespace Microsoft.EntityFrameworkCore.Design;

public class MethodCallCodeFragmentTest
{
    [ConditionalFact]
    public virtual void Ctor_throw_when_too_many_parameters_extension()
    {
        _ = new MethodCallCodeFragment(_extensionFuncMethodInfo, 1);
        Assert.Throws<ArgumentException>(() => new MethodCallCodeFragment(_extensionFuncMethodInfo, 1, 2));
    }

    [ConditionalFact]
    public virtual void Ctor_throw_when_too_many_parameters_instance()
    {
        _ = new MethodCallCodeFragment(_instanceFuncMethodInfo, 1);
        Assert.Throws<ArgumentException>(() => new MethodCallCodeFragment(_instanceFuncMethodInfo, 1, 2));
    }

    private static readonly MethodInfo _extensionFuncMethodInfo
        = typeof(MethodCallCodeFragmentTest).GetRequiredRuntimeMethod(nameof(ExtensionFunc), typeof(object), typeof(int));

    private static readonly MethodInfo _instanceFuncMethodInfo
        = typeof(MethodCallCodeFragmentTest).GetRequiredRuntimeMethod(nameof(InstanceFunc), typeof(int));

    public static void ExtensionFunc(object thisParameter, int p)
        => throw new NotSupportedException();

    public void InstanceFunc(int p)
        => throw new NotSupportedException();
}
