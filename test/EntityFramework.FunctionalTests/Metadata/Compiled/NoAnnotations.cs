// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Compiled
{
    public class NoAnnotations
    {
        public IEnumerable<IAnnotation> Annotations
        {
            get { return ImmutableList<IAnnotation>.Empty; }
        }

        public string this[string annotationName]
        {
            get
            {
                return null;
            }
        }

        public virtual string StorageName
        {
            get { return null; }
        }
    }
}
