// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class RelationalIndexAnnotations : RelationalAnnotationsBase, IRelationalIndexAnnotations
    {
        public RelationalIndexAnnotations([NotNull] IIndex key, [CanBeNull] string providerPrefix)
            : base(key, providerPrefix)
        {
        }

        public RelationalIndexAnnotations(
            [NotNull] InternalIndexBuilder internalBuilder,
            ConfigurationSource configurationSource,
            [CanBeNull] string providerPrefix)
            : base(internalBuilder, configurationSource, providerPrefix)
        {
        }

        protected virtual IIndex Index => (IIndex)Metadata;

        public virtual string Name
        {
            get { return (string)GetAnnotation(RelationalAnnotationNames.Name) ?? GetDefaultName(); }
            [param: CanBeNull] set { SetAnnotation(RelationalAnnotationNames.Name, Check.NullButNotEmpty(value, nameof(value))); }
        }

        protected virtual string GetDefaultName()
            => "IX_" +
               Index.DeclaringEntityType.DisplayName() +
               "_" +
               string.Join("_", Index.Properties.Select(p => p.Name));
    }
}
