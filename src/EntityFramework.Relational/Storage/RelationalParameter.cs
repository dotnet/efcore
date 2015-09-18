// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Storage
{
    public class RelationalParameter
    {
        public RelationalParameter(
            [NotNull] string name,
            [CanBeNull] object value,
            [CanBeNull] IProperty property = null)
        {
            Check.NotNull(name, nameof(name));

            Name = name;
            Value = value;
            Property = property;
        }

        public virtual string Name { get; }

        public virtual object Value { get; }

        public virtual IProperty Property { get; }
    }
}