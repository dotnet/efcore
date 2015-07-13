// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Internal
{
    public struct DbSetProperty
    {
        public DbSetProperty([NotNull] Type contextType, [NotNull] string name, [NotNull] Type entityType, bool hasSetter)
        {
            ContextType = contextType;
            Name = name;
            EntityType = entityType;
            HasSetter = hasSetter;
        }

        public Type ContextType { get; }

        public string Name { get; }

        public Type EntityType { get; }

        public bool HasSetter { get; }
    }
}
