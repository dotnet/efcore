// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Compiled
{
    public class NoAnnotations
    {
        public IEnumerable<IAnnotation> Annotations
        {
            get { return ImmutableList<Annotation>.Empty; }
        }

        public string this[[NotNull] string annotationName]
        {
            get
            {
                Check.NotEmpty(annotationName, "annotationName");

                return null;
            }
        }

        public virtual string StorageName
        {
            get { return null; }
        }
    }
}
