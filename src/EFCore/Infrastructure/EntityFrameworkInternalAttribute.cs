// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    /// Marks a type or member as supporting the Entity Framework Core infrastructure and not intended to be used directly from
    /// user or provider code.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Enum | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface |
        AttributeTargets.Event | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Delegate |
        AttributeTargets.Property | AttributeTargets.Constructor)]
    public sealed class EntityFrameworkInternalAttribute : Attribute
    {
    }
}
