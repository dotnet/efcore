// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    [DebuggerDisplay("{Name}")]
    public abstract class MetadataBase
    {
        private readonly string _name;

        private string _storageName;

        private readonly LazyRef<ImmutableDictionary<string, Annotation>> _annotations
            = new LazyRef<ImmutableDictionary<string, Annotation>>(() => ImmutableDictionary<string, Annotation>.Empty);

        protected MetadataBase()
        {
        }

        protected MetadataBase(string name)
        {
            Check.NotEmpty(name, "name");

            _name = name;
        }

        public virtual string Name
        {
            get { return _name; }
        }

        public virtual string StorageName
        {
            get { return _storageName ?? Name; }
            
            set
            {
                Check.NotEmpty(value, "value");

                _storageName = value;
            }
        }

        public virtual void AddAnnotation(Annotation annotation)
        {
            Check.NotNull(annotation, "annotation");

            _annotations.ExchangeValue(d => d.Add(annotation.Name, annotation));
        }

        public virtual void RemoveAnnotation(Annotation annotation)
        {
            Check.NotNull(annotation, "annotation");

            _annotations.ExchangeValue(l => l.Remove(annotation.Name));
        }

        public virtual object this[string annotation]
        {
            get
            {
                Check.NotEmpty(annotation, "annotation");

                Annotation value;
                return _annotations.HasValue
                       && _annotations.Value.TryGetValue(annotation, out value)
                    ? value.Value
                    : null;
            }
            
            set
            {
                Check.NotEmpty(annotation, "annotation");
                Check.NotNull(value, "value");

                _annotations.ExchangeValue(l => l.Remove(annotation));

                AddAnnotation(new Annotation(annotation, value));
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
    }
}
