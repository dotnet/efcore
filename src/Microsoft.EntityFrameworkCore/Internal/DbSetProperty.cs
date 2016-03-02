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
            [NotNull] Type clrType,
            [CanBeNull] IClrPropertySetter setter)
        {
            Name = name;
            ClrType = clrType;
            Setter = setter;
        }

        public string Name { get; }

        public Type ClrType { get; }

        public IClrPropertySetter Setter { get; }
    }
}
