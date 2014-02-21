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
        private readonly LazyRef<ImmutableDictionary<string, Annotation>> _annotations
            = new LazyRef<ImmutableDictionary<string, Annotation>>(() => ImmutableDictionary<string, Annotation>.Empty);

        public virtual void AddAnnotation([NotNull] Annotation annotation)
        {
            Check.NotNull(annotation, "annotation");

            _annotations.ExchangeValue(d => d.Add(annotation.Name, annotation));
        }

        public virtual void RemoveAnnotation([NotNull] Annotation annotation)
        {
            Check.NotNull(annotation, "annotation");

            _annotations.ExchangeValue(d => d.Remove(annotation.Name));
        }

// ReSharper disable once AnnotationRedundanceInHierarchy
        public virtual string this[[param: NotNull] string annotationName]
        {
            get
            {
                Check.NotEmpty(annotationName, "annotationName");

                Annotation value;
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

                _annotations.ExchangeValue(
                    d => d.SetItem(annotationName, new Annotation(annotationName, value)));
            }
        }

        public virtual IEnumerable<Annotation> Annotations
        {
            get
            {
                return _annotations.HasValue
                    ? _annotations.Value.Values.OrderByOrdinal(e => e.Name)
                    : Enumerable.Empty<Annotation>();
            }
        }

        IEnumerable<IAnnotation> IMetadata.Annotations
        {
            get { return Annotations; }
        }
    }
}
