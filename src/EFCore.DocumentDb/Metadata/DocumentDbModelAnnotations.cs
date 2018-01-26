// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class DocumentDbModelAnnotations : IDocumentDbModelAnnotations
    {
        public DocumentDbModelAnnotations(IModel model)
            : this(new DocumentDbAnnotations(model))
        {
        }

        protected DocumentDbModelAnnotations(DocumentDbAnnotations annotations)
        {
            Annotations = annotations;
        }

        protected virtual DocumentDbAnnotations Annotations { get; }
        protected virtual IModel Model => (IModel)Annotations.Metadata;
    }
}
