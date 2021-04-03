// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
