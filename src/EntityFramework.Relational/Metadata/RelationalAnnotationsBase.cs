// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public abstract class RelationalAnnotationsBase
    {
        protected RelationalAnnotationsBase(
            [NotNull] IAnnotatable metadata,
            [CanBeNull] string providerPrefix)
        {
            Check.NotNull(metadata, nameof(metadata));
            Check.NullButNotEmpty(providerPrefix, nameof(providerPrefix));

            Metadata = metadata;
            ProviderPrefix = providerPrefix;
        }

        protected virtual IAnnotatable Metadata { get; }

        protected virtual string ProviderPrefix { get; }

        protected virtual object GetAnnotation([NotNull] string annotationName)
        {
            Check.NotEmpty(annotationName, nameof(annotationName));

            return (ProviderPrefix == null ? null : Metadata[ProviderPrefix + annotationName])
                   ?? Metadata[RelationalAnnotationNames.Prefix + annotationName];
        }

        protected virtual void SetAnnotation([NotNull] string annotationName, [CanBeNull] object value)
        {
            Check.NotEmpty(annotationName, nameof(annotationName));

            ((Annotatable)Metadata)[(ProviderPrefix ?? RelationalAnnotationNames.Prefix) + annotationName] = value;
        }

        protected virtual object GetAnnotation([NotNull] IAnnotatable metadata, [NotNull] string annotationName)
        {
            Check.NotNull(metadata, nameof(metadata));
            Check.NotEmpty(annotationName, nameof(annotationName));

            return (ProviderPrefix == null ? null : metadata[ProviderPrefix + annotationName])
                   ?? metadata[RelationalAnnotationNames.Prefix + annotationName];
        }

        protected virtual void SetAnnotation([NotNull] IAnnotatable metadata, [NotNull] string annotationName, [CanBeNull] object value)
        {
            Check.NotNull(metadata, nameof(metadata));
            Check.NotEmpty(annotationName, nameof(annotationName));

            ((Annotatable)metadata)[(ProviderPrefix ?? RelationalAnnotationNames.Prefix) + annotationName] = value;
        }
    }
}
