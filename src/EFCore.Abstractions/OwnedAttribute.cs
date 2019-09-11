// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Marks a type as owned. All references to this type will be configured as owned entity types.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class OwnedAttribute : Attribute
    {
    }
}
