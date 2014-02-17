// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Compiled
{
    public class CompiledModelBase<TEntities, TAnnotations>
        where TEntities : CompiledEntitiesBase, new()
        where TAnnotations : CompiledAnnotationsBase, new()
    {
        protected readonly LazyMetadataRef<TAnnotations> LazyAnnotations = new LazyMetadataRef<TAnnotations>();
        protected readonly LazyMetadataRef<TEntities> LazyEntities = new LazyMetadataRef<TEntities>();

        public IEnumerable<IAnnotation> Annotations
        {
            get { return LazyAnnotations.Value.Annotations; }
        }

        public string this[[NotNull] string annotationName]
        {
            get
            {
                Check.NotEmpty(annotationName, "annotationName");

                var annotation = LazyAnnotations.Value.Annotations.FirstOrDefault(a => a.Name == annotationName);
                return annotation == null ? null : annotation.Value;
            }
        }

        public IEntityType Entity([NotNull] object instance)
        {
            Check.NotNull(instance, "instance");

            return Entity(instance.GetType());
        }

        public IEntityType Entity([NotNull] Type type)
        {
            Check.NotNull(type, "type");

            return Entities.FirstOrDefault(e => e.Type == type);
        }

        public IEnumerable<IEntityType> Entities
        {
            get { return LazyEntities.Value.EntityTypes; }
        }
    }
}
