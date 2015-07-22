// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public abstract class RelationalAnnotationsBase
    {
        private readonly ConfigurationSource _configurationSource;

        private readonly InternalMetadataBuilder _internalBuilder;

        protected RelationalAnnotationsBase(
            [NotNull] IAnnotatable metadata,
            [CanBeNull] string providerPrefix)
        {
            Check.NotNull(metadata, nameof(metadata));
            Check.NullButNotEmpty(providerPrefix, nameof(providerPrefix));

            Metadata = metadata;
            _configurationSource = ConfigurationSource.Explicit;
            ProviderPrefix = providerPrefix;
        }

        protected RelationalAnnotationsBase(
            [NotNull] InternalMetadataBuilder internalBuilder,
            ConfigurationSource configurationSource,
            [CanBeNull] string providerPrefix)
        {
            Check.NotNull(internalBuilder, nameof(internalBuilder));
            Check.NullButNotEmpty(providerPrefix, nameof(providerPrefix));

            Metadata = internalBuilder.Metadata;
            _internalBuilder = internalBuilder;
            _configurationSource = configurationSource;
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

            var fullName = (ProviderPrefix ?? RelationalAnnotationNames.Prefix) + annotationName;

            if (_configurationSource != ConfigurationSource.Explicit)
            {
                _internalBuilder.Annotation(fullName, value, _configurationSource);
            }
            else
            {
                ((Annotatable)Metadata)[fullName] = value;
            }
        }

        protected virtual object GetAnnotation([NotNull] IAnnotatable metadata, [NotNull] string annotationName)
        {
            Check.NotNull(metadata, nameof(metadata));
            Check.NotEmpty(annotationName, nameof(annotationName));

            return (ProviderPrefix == null ? null : metadata[ProviderPrefix + annotationName])
                   ?? metadata[RelationalAnnotationNames.Prefix + annotationName];
        }
    }
}
