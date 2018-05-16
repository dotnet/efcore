// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Cosmos.Sql.Metadata
{
    public class CosmosSqlEntityTypeAnnotations : ICosmosSqlEntityTypeAnnotations
    {
        public CosmosSqlEntityTypeAnnotations(IEntityType entityType)
            : this(new CosmosSqlAnnotations(entityType))
        {
        }

        protected CosmosSqlEntityTypeAnnotations(CosmosSqlAnnotations annotations) => Annotations = annotations;

        protected virtual CosmosSqlAnnotations Annotations { get; }

        protected virtual IEntityType EntityType => (IEntityType)Annotations.Metadata;

        protected virtual CosmosSqlEntityTypeAnnotations GetAnnotations([NotNull] IEntityType entityType)
            => new CosmosSqlEntityTypeAnnotations(entityType);

        public virtual string CollectionName
        {
            get => EntityType.BaseType != null
                ? GetAnnotations(EntityType.RootType()).CollectionName
                : ((string)Annotations.Metadata[CosmosSqlAnnotationNames.CollectionName])
                    ?? GetDefaultCollectionName();

            [param: CanBeNull]
            set => SetCollectionName(value);
        }

        private static string GetDefaultCollectionName() => "Unicorn";

        protected virtual bool SetCollectionName([CanBeNull] string value)
        {
            return Annotations.SetAnnotation(
                CosmosSqlAnnotationNames.CollectionName,
                Check.NullButNotEmpty(value, nameof(value)));
        }
    }
}
