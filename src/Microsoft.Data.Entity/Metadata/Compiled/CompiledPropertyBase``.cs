using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Compiled
{
    public class CompiledPropertyBase<TEntity, TProperty, TAnnotations> 
        : CompiledPropertyBase<TEntity, TProperty>
        where TAnnotations : CompiledAnnotationsBase, new()
    {
        protected readonly LazyMetadataRef<TAnnotations> LazyAnnotations = new LazyMetadataRef<TAnnotations>();

        public new IEnumerable<IAnnotation> Annotations
        {
            get { return LazyAnnotations.Value.Annotations; }
        }

        public new string this[[NotNull] string annotationName]
        {
            get
            {
                Check.NotEmpty(annotationName, "annotationName"); 
                
                var annotation = LazyAnnotations.Value.Annotations.FirstOrDefault(a => a.Name == annotationName);
                return annotation == null ? null : annotation.Value;
            }
        }

        public Type Type
        {
            get { return typeof(TProperty); }
        }

        public Type DeclaringType
        {
            get { return typeof(TEntity); }
        }
    }
}