// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace System.Reflection
{
    [DebuggerStepThrough]
    public static class MethodInfoExtensions
    {
        public static bool MethodIsClosedFormOf([NotNull] this MethodInfo methodInfo, [NotNull] MethodInfo genericMethod)
            => methodInfo.IsGenericMethod
               && ReferenceEquals(
                   methodInfo.GetGenericMethodDefinition(),
                   genericMethod);
    }
}
