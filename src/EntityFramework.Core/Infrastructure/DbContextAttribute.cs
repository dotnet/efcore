// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Infrastructure
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DbContextAttribute : Attribute
    {
        public DbContextAttribute([NotNull] Type contextType)
        {
            Check.NotNull(contextType, nameof(contextType));

            ContextType = contextType;
        }

        public Type ContextType { get; }
    }
}
