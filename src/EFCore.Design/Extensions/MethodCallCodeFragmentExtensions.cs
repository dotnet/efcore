// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Design;

/// <summary>
///     Design-time <see cref="MethodCallCodeFragment" /> extensions.
/// </summary>
public static class MethodCallCodeFragmentExtensions
{
    /// <summary>
    ///     Gets the using statements required for this method call.
    /// </summary>
    /// <param name="methodCall">The method call.</param>
    /// <returns>The usings.</returns>
    public static IEnumerable<string> GetRequiredUsings(this MethodCallCodeFragment methodCall)
    {
        var method = methodCall.MethodInfo;
        if (method?.IsStatic == true)
        {
            yield return method.DeclaringType!.Namespace!;
        }

        foreach (var argument in methodCall.Arguments)
        {
            if (argument is NestedClosureCodeFragment nestedClosure)
            {
                foreach (var nestedUsing in nestedClosure.MethodCalls.SelectMany(GetRequiredUsings))
                {
                    yield return nestedUsing;
                }
            }
            else if (argument is not null)
            {
                yield return argument.GetType().Namespace!;
            }
        }
    }
}
