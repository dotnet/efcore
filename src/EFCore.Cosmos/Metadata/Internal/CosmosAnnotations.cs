// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal
{
    public class CosmosAnnotations
    {
        public CosmosAnnotations([NotNull] IAnnotatable metadata)
        {
            Check.NotNull(metadata, nameof(metadata));

            Metadata = metadata;
        }

        public virtual IAnnotatable Metadata { get; }

        public virtual bool SetAnnotation(
            [NotNull] string annotationName,
            [CanBeNull] object value)
        {
            ((IMutableAnnotatable)Metadata)[annotationName] = value;

            return true;
        }

        public virtual bool CanSetAnnotation(
            [NotNull] string relationalAnnotationName,
            [CanBeNull] object value)
            => true;

        public virtual bool RemoveAnnotation([NotNull] string annotationName)
        {
            ((IMutableAnnotatable)Metadata).RemoveAnnotation(annotationName);

            return true;
        }
    }
}
