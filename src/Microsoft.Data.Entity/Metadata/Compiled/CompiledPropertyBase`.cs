using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Compiled
{
    public class CompiledPropertyBase<TEntity, TProperty>
    {
        public IEnumerable<IAnnotation> Annotations
        {
            get { return CompiledAnnotationsBase.EmptyAnnotations; }
        }

        public string this[[NotNull] string annotationName]
        {
            get
            {
                Check.NotEmpty(annotationName, "annotationName");

                return null;
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