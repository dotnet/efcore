// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
