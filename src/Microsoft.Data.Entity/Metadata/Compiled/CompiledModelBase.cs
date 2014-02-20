// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Metadata.Compiled
{
    public class CompiledModelBase<TEntities, TAnnotations>
        where TEntities : CompiledEntitiesBase, new()
        where TAnnotations : CompiledAnnotationsBase, new()
    {
        protected TAnnotations LazyAnnotations;
        protected TEntities LazyEntities;

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

        public IEntityType Entity([NotNull] object instance)
        {
            return Entity(instance.GetType());
        }

        public IEntityType Entity([NotNull] Type type)
        {
            return Entities.FirstOrDefault(e => e.Type == type);
        }

        public IEnumerable<IEntityType> Entities
        {
            get { return LazyMetadata.Init(ref LazyEntities).EntityTypes; }
        }
    }
}
