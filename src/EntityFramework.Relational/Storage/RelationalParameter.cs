// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Storage
{
    public class RelationalParameter
    {
        public RelationalParameter(
            [NotNull] string name,
            [CanBeNull] object value,
            [NotNull] RelationalTypeMapping relationalTypeMapping,
            [CanBeNull] bool? nullable)
        {
            Name = name;
            Value = value;
            RelationalTypeMapping = relationalTypeMapping;
            Nullable = nullable;
        }

        public virtual string Name { get; }

        public virtual object Value { get; }

        public virtual RelationalTypeMapping RelationalTypeMapping { get; }

        public virtual bool? Nullable { get; }
    }
}
