// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class RelationalKeyAnnotations : IRelationalKeyAnnotations
    {
        public RelationalKeyAnnotations([NotNull] IKey key)
            : this(new RelationalAnnotations(key))
        {
        }

        protected RelationalKeyAnnotations([NotNull] RelationalAnnotations annotations)
            => Annotations = annotations;

        protected virtual RelationalAnnotations Annotations { get; }

        protected virtual IKey Key => (IKey)Annotations.Metadata;

        public virtual string Name
        {
            get => (string)Annotations.Metadata[RelationalAnnotationNames.Name]
                   ?? ConstraintNamer.GetDefaultName(Key);

            [param: CanBeNull] set => SetName(value);
        }

        protected virtual bool SetName([CanBeNull] string value)
            => Annotations.SetAnnotation(
                RelationalAnnotationNames.Name,
                Check.NullButNotEmpty(value, nameof(value)));
    }
}
