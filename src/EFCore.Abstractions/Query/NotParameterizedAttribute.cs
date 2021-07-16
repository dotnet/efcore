// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     Signals that custom LINQ operator parameter should not be parameterized during query compilation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class NotParameterizedAttribute : Attribute
    {
    }
}
