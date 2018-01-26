// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class DocumentDbKeyAnnotations : IDocumentDbKeyAnnotations
    {
        public DocumentDbKeyAnnotations(IKey key)
            : this(new DocumentDbAnnotations(key))
        {
        }

        protected DocumentDbKeyAnnotations(DocumentDbAnnotations annotations)
        {
            Annotations = annotations;
        }

        protected virtual DocumentDbAnnotations Annotations { get; }
        protected virtual IKey Key => (IKey)Annotations.Metadata;
    }
}
