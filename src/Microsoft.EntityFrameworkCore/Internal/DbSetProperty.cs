// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Internal
{
    public struct DbSetProperty
    {
        public DbSetProperty(
            [NotNull] string name,
            [NotNull] Type entityType,
            [CanBeNull] IClrPropertySetter setter)
        {
            Name = name;
            EntityType = entityType;
            Setter = setter;
        }

        public string Name { get; }

        public Type EntityType { get; }

        public IClrPropertySetter Setter { get; }
    }
}
