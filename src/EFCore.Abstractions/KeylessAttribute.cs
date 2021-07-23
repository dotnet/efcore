// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Marks a type as keyless entity.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class KeylessAttribute : Attribute
    {
    }
}
