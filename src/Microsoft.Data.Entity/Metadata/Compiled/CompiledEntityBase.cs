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
        protected TProperties LazyProperties;
        protected TAnnotations LazyAnnotations;

        public IEnumerable<IAnnotation> Annotations
        {
            get { return LazyMetadata.Init(ref LazyAnnotations).Annotations; }
        }

        public string this[[NotNull] string annotationName]
        {
            get
            {
                var annotation = LazyMetadata.Init(ref LazyAnnotations).Annotations.FirstOrDefault(a => a.Name == annotationName);
                return annotation == null ? null : annotation.Value;
            }
        }

        public IProperty Property([NotNull] string name)
        {
            return LazyMetadata.Init(ref LazyProperties).Properties.FirstOrDefault(p => p.Name == name);
        }

        public Type Type
        {
            get { return typeof(TEntity); }
        }

        public IEnumerable<IProperty> Key
        {
            get { return LazyMetadata.Init(ref LazyProperties).Keys; }
        }

        public IEnumerable<IProperty> Properties
        {
            get { return LazyMetadata.Init(ref LazyProperties).Properties; }
        }

        public IEnumerable<IForeignKey> ForeignKeys
        {
            get { return null; }
        }
    }
}
