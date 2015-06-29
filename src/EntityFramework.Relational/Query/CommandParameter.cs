// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Query
{
    public class CommandParameter
    {
        public CommandParameter(
            [NotNull] string name,
            [NotNull] object value,
            [NotNull] RelationalTypeMapping typeMapping)
        {
            Check.NotNull(name, nameof(name));
            Check.NotNull(value, nameof(value));
            Check.NotNull(typeMapping, nameof(typeMapping));

            Name = name;
            Value = value;
            TypeMapping = typeMapping;
        }

        public virtual string Name { get; }

        public virtual object Value { get; }

        public virtual RelationalTypeMapping TypeMapping { get; }
    }
}
