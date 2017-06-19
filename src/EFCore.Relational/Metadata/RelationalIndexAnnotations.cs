// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class RelationalIndexAnnotations : IRelationalIndexAnnotations
    {
        public RelationalIndexAnnotations([NotNull] IIndex index)
            : this(new RelationalAnnotations(index))
        {
        }

        protected RelationalIndexAnnotations([NotNull] RelationalAnnotations annotations)
            => Annotations = annotations;

        protected virtual RelationalAnnotations Annotations { get; }
        protected virtual IIndex Index => (IIndex)Annotations.Metadata;

        public virtual string Name
        {
            get => (string)Annotations.Metadata[RelationalAnnotationNames.Name]
                   ?? ConstraintNamer.GetDefaultName(Index);

            [param: CanBeNull] set => SetName(value);
        }

        public virtual string Filter
        {
            get => (string)Annotations.Metadata[RelationalAnnotationNames.Filter];
            [param: CanBeNull] set => SetFilter(value);
        }

        protected virtual bool SetFilter([CanBeNull] string value)
            => Annotations.SetAnnotation(
                RelationalAnnotationNames.Filter,
                Check.NullButNotEmpty(value, nameof(value)));

        protected virtual bool SetName([CanBeNull] string value)
            => Annotations.SetAnnotation(
                RelationalAnnotationNames.Name,
                Check.NullButNotEmpty(value, nameof(value)));
    }
}
