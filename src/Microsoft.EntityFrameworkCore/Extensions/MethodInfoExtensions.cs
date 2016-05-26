// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace System.Reflection
{
    /// <summary>
    ///     Extension methods for <see cref="MethodInfo" />.
    /// </summary>
    [DebuggerStepThrough]
    public static class MethodInfoExtensions
    {
        /// <summary>
        ///     Gets a value indicating whether the specified <see cref="MethodInfo" /> is a version of this <see cref="MethodInfo" /> that
        ///     has the generic types specified.
        /// </summary>
        /// <param name="methodInfo"> The base method without the types specified. </param>
        /// <param name="genericMethod"> The method with the types specified. </param>
        /// <returns></returns>
        public static bool MethodIsClosedFormOf([NotNull] this MethodInfo methodInfo, [NotNull] MethodInfo genericMethod)
            => methodInfo.IsGenericMethod
               && ReferenceEquals(
                   methodInfo.GetGenericMethodDefinition(),
                   genericMethod);
    }
}
