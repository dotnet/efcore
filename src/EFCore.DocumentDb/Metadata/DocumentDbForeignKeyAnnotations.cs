// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class DocumentDbForeignKeyAnnotations : IDocumentDbForeignKeyAnnotations
    {
        public DocumentDbForeignKeyAnnotations(IForeignKey foreignKey)
            : this(new DocumentDbAnnotations(foreignKey))
        {
        }

        protected DocumentDbForeignKeyAnnotations(DocumentDbAnnotations annotations)
        {
            Annotations = annotations;
        }

        protected virtual DocumentDbAnnotations Annotations { get; }
        protected virtual IForeignKey ForeignKey => (IForeignKey)Annotations.Metadata;
    }
}
