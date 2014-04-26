// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public abstract class MetadataBase : IMetadata
    {
        private readonly Annotations _annotations = new Annotations();

        // ReSharper disable once AnnotationRedundanceInHierarchy
        public virtual string this[[param: NotNull] string annotationName]
        {
            get
            {
                Check.NotEmpty(annotationName, "annotationName");

                return _annotations[annotationName];
            }
            [param: NotNull]
            set
            {
                Check.NotEmpty(annotationName, "annotationName");
                Check.NotEmpty(value, "value");

                _annotations[annotationName] = value;
            }
        }

        public virtual Annotations Annotations
        {
            get { return _annotations; }
        }

        IEnumerable<IAnnotation> IMetadata.Annotations
        {
            get { return Annotations; }
        }
    }
}
