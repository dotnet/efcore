// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class RelationalForeignKeyAnnotations : RelationalAnnotationsBase, IRelationalForeignKeyAnnotations
    {
        public RelationalForeignKeyAnnotations([NotNull] IForeignKey foreignKey, [CanBeNull] string providerPrefix)
            : base(foreignKey, providerPrefix)
        {
        }

        protected virtual IForeignKey ForeignKey => (IForeignKey)Metadata;

        public virtual string Name
        {
            get { return (string)GetAnnotation(RelationalAnnotationNames.Name) ?? GetDefaultName(); }
            [param: CanBeNull] set { SetAnnotation(RelationalAnnotationNames.Name, Check.NullButNotEmpty(value, nameof(value))); }
        }

        protected virtual string GetDefaultName()
            => "FK_" +
               ForeignKey.DeclaringEntityType.DisplayName() +
               "_" +
               ForeignKey.PrincipalEntityType.DisplayName() +
               "_" +
               string.Join("_", ForeignKey.Properties.Select(p => p.Name));
    }
}
