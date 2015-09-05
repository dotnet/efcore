// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class RelationalAnnotations
    {
        public RelationalAnnotations(
            [NotNull] IAnnotatable metadata,
            [CanBeNull] string providerPrefix)
        {
            Check.NotNull(metadata, nameof(metadata));
            Check.NullButNotEmpty(providerPrefix, nameof(providerPrefix));

            Metadata = metadata;
            ProviderPrefix = providerPrefix;
        }

        public virtual IAnnotatable Metadata { get; }

        public virtual string ProviderPrefix { get; }

        public virtual object GetAnnotation([NotNull] string annotationName)
        {
            Check.NotEmpty(annotationName, nameof(annotationName));

            return (ProviderPrefix == null ? null : Metadata[ProviderPrefix + annotationName])
                   ?? Metadata[RelationalAnnotationNames.Prefix + annotationName];
        }

        public virtual bool SetAnnotation([NotNull] string annotationName, [CanBeNull] object value)
        {
            Check.NotEmpty(annotationName, nameof(annotationName));

            var annotatable = Metadata as Annotatable;
            Debug.Assert(annotatable != null);

            var fullName = (ProviderPrefix ?? RelationalAnnotationNames.Prefix) + annotationName;
            annotatable[fullName] = value;
            return true;
        }
    }
}
