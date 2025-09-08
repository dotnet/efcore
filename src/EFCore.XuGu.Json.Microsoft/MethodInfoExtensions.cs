// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace System.Reflection
{
    [DebuggerStepThrough]
    internal static class MethodInfoExtensions
    {
        internal static bool IsClosedFormOf(
            [NotNull] this MethodInfo methodInfo, [NotNull] MethodInfo genericMethod)
            => methodInfo.IsGenericMethod
               && Equals(
                   methodInfo.GetGenericMethodDefinition(),
                   genericMethod);
    }
}
