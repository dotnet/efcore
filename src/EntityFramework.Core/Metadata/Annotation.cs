// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class Annotation : IAnnotation
    {
        public Annotation([NotNull] string name, [NotNull] string value)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotEmpty(value, nameof(value));

            Name = name;
            Value = value;
        }

        public virtual string Name { get; }

        public virtual string Value { get; }
    }
}
