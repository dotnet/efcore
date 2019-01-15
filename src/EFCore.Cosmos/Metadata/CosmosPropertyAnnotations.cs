// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Cosmos.Metadata
{
    public class CosmosPropertyAnnotations : ICosmosPropertyAnnotations
    {
        public CosmosPropertyAnnotations(IProperty property)
            : this(new CosmosAnnotations(property))
        {
        }

        protected CosmosPropertyAnnotations(CosmosAnnotations annotations) => Annotations = annotations;

        protected virtual CosmosAnnotations Annotations { get; }

        protected virtual IProperty Property => (IProperty)Annotations.Metadata;

        public virtual string PropertyName
        {
            get => ((string)Annotations.Metadata[CosmosAnnotationNames.PropertyName])
                   ?? Property.Name;

            [param: CanBeNull] set => SetPropertyName(value);
        }

        protected virtual bool SetPropertyName([CanBeNull] string value)
            => Annotations.SetAnnotation(
                CosmosAnnotationNames.PropertyName,
                value);
    }
}
