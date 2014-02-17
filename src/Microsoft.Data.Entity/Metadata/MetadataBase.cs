// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public abstract class MetadataBase : IMetadata
    {
        private readonly LazyRef<ImmutableDictionary<string, IAnnotation>> _annotations
            = new LazyRef<ImmutableDictionary<string, IAnnotation>>(() => ImmutableDictionary<string, IAnnotation>.Empty);

        public virtual void AddAnnotation([NotNull] IAnnotation annotation)
        {
            Check.NotNull(annotation, "annotation");

            _annotations.ExchangeValue(d => d.Add(annotation.Name, annotation));
        }

        public virtual void RemoveAnnotation([NotNull] IAnnotation annotation)
        {
            Check.NotNull(annotation, "annotation");

            _annotations.ExchangeValue(l => l.Remove(annotation.Name));
        }

        public virtual string this[string annotationName]
        {
            get
            {
                Check.NotEmpty(annotationName, "annotationName");

                IAnnotation value;
                return _annotations.HasValue
                       && _annotations.Value.TryGetValue(annotationName, out value)
                    ? value.Value
                    : null;
            }
            [param: NotNull]
            set
            {
                Check.NotEmpty(annotationName, "annotationName");
                Check.NotEmpty(value, "value");

                _annotations.ExchangeValue(l => l.Remove(annotationName));

                AddAnnotation(new Annotation(annotationName, value));
            }
        }

        public virtual IEnumerable<IAnnotation> Annotations
        {
            get
            {
                return _annotations.HasValue
                    ? _annotations.Value.Values.OrderByOrdinal(e => e.Name)
                    : Enumerable.Empty<IAnnotation>();
            }
        }
    }
}
