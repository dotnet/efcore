// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class RelationalAnnotations
    {
        public RelationalAnnotations([NotNull] IAnnotatable metadata)
        {
            Check.NotNull(metadata, nameof(metadata));

            Metadata = metadata;
        }

        public virtual IAnnotatable Metadata { get; }

        public virtual object GetAnnotation([NotNull] string fallbackAnnotationName, [CanBeNull] string primaryAnnotationName)
        {
            // Not using Check for perf
            Debug.Assert(!string.IsNullOrEmpty(fallbackAnnotationName));

            return (primaryAnnotationName == null ? null : Metadata[primaryAnnotationName])
                   ?? Metadata[fallbackAnnotationName];
        }

        public virtual bool SetAnnotation(
            [NotNull] string relationalAnnotationName,
            [CanBeNull] string providerAnnotationName,
            [CanBeNull] object value)
        {
            // Not using Check for perf
            Debug.Assert(!string.IsNullOrEmpty(relationalAnnotationName));

            var annotatable = Metadata as IMutableAnnotatable;
            Debug.Assert(annotatable != null);

            annotatable[providerAnnotationName ?? relationalAnnotationName] = value;
            return true;
        }
    }
}
