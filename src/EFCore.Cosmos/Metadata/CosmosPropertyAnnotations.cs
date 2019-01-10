// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

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
                    ?? GetDefaultPropertyName();

            [param: CanBeNull]
            set => SetPropertyName(value);
        }

        private string GetDefaultPropertyName()
        {
            var entityType = Property.DeclaringEntityType;
            var ownership = entityType.FindOwnership();

            if (ownership != null
                && !entityType.IsDocumentRoot())
            {
                var pk = Property.GetContainingPrimaryKey();
                if (pk != null
                    && pk.Properties.Count == ownership.Properties.Count + (ownership.IsUnique ? 0 : 1)
                    && ownership.Properties.All(fkProperty => pk.Properties.Contains(fkProperty)))
                {
                    return "";
                }
            }

            return Property.Name;
        }

        protected virtual bool SetPropertyName([CanBeNull] string value)
            => Annotations.SetAnnotation(
                CosmosAnnotationNames.PropertyName,
                value);
    }
}
