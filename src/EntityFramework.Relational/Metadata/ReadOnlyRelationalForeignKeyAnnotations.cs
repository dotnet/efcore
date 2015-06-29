// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class ReadOnlyRelationalForeignKeyAnnotations : IRelationalForeignKeyAnnotations
    {
        protected const string NameAnnotation = RelationalAnnotationNames.Prefix + RelationalAnnotationNames.Name;

        private readonly IForeignKey _foreignKey;

        public ReadOnlyRelationalForeignKeyAnnotations([NotNull] IForeignKey foreignKey)
        {
            Check.NotNull(foreignKey, nameof(foreignKey));

            _foreignKey = foreignKey;
        }

        public virtual string Name => _foreignKey[NameAnnotation] as string ?? GetDefaultName();

        protected virtual IForeignKey ForeignKey => _foreignKey;

        protected virtual string GetDefaultName()
            => "FK_" +
               _foreignKey.EntityType.DisplayName() +
               "_" +
               _foreignKey.PrincipalEntityType.DisplayName() +
               "_" +
               string.Join("_", _foreignKey.Properties.Select(p => p.Name));
    }
}
