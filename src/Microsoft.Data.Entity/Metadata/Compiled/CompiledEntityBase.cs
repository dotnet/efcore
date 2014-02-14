// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Metadata.Compiled
{
    public class CompiledEntityBase<TEntity, TProperties, TAnnotations>
        where TProperties : CompiledPropertiesBase, new()
        where TAnnotations : CompiledAnnotationsBase, new()
    {
        protected readonly LazyMetadataRef<TProperties> LazyProperties = new LazyMetadataRef<TProperties>();
        protected readonly LazyMetadataRef<TAnnotations> LazyAnnotations = new LazyMetadataRef<TAnnotations>();

        public IEnumerable<IAnnotation> Annotations
        {
            get { return LazyAnnotations.Value.Annotations; }
        }

        public string this[[NotNull] string annotationName]
        {
            get
            {
                var annotation = LazyAnnotations.Value.Annotations.FirstOrDefault(a => a.Name == annotationName);
                return annotation == null ? null : annotation.Value;
            }
        }

        public IProperty Property([NotNull] string name)
        {
            return LazyProperties.Value.Properties.FirstOrDefault(p => p.Name == name);
        }

        public Type Type
        {
            get { return typeof(TEntity); }
        }

        public IEnumerable<IProperty> Key
        {
            get { return LazyProperties.Value.Keys; }
        }

        public IEnumerable<IProperty> Properties
        {
            get { return LazyProperties.Value.Properties; }
        }
    }
}
