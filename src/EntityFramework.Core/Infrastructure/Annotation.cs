// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Infrastructure
{
    public class Annotation : IAnnotation
    {
        public Annotation([NotNull] string name, [NotNull] object value)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(value, nameof(value));

            Name = name;
            Value = value;
        }

        public virtual string Name { get; }
        public virtual object Value { get; }
    }
}
