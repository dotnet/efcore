// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class DocumentDbIndexAnnotations : IDocumentDbIndexAnnotations
    {
        public DocumentDbIndexAnnotations(IIndex index)
            : this(new DocumentDbAnnotations(index))
        {
        }

        protected DocumentDbIndexAnnotations(DocumentDbAnnotations annotations)
        {
            Annotations = annotations;
        }

        protected virtual DocumentDbAnnotations Annotations { get; }
        protected virtual IIndex Index => (IIndex)Annotations.Metadata;
    }
}
