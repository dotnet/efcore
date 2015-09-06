// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class RelationalForeignKeyAnnotations : IRelationalForeignKeyAnnotations
    {
        public RelationalForeignKeyAnnotations([NotNull] IForeignKey foreignKey, [CanBeNull] string providerPrefix)
            : this(new RelationalAnnotations(foreignKey, providerPrefix))
        {
        }

        protected RelationalForeignKeyAnnotations([NotNull] RelationalAnnotations annotations)
        {
            Annotations = annotations;
        }

        protected virtual RelationalAnnotations Annotations { get; }

        protected virtual IForeignKey ForeignKey => (IForeignKey)Annotations.Metadata;

        public virtual string Name
        {
            get { return (string)Annotations.GetAnnotation(RelationalAnnotationNames.Name) ?? GetDefaultName(); }
            [param: CanBeNull] set { SetName(value); }
        }

        protected virtual bool SetName([CanBeNull] string value)
            => Annotations.SetAnnotation(RelationalAnnotationNames.Name, Check.NullButNotEmpty(value, nameof(value)));

        protected virtual string GetDefaultName()
            => "FK_" + ForeignKey.DeclaringEntityType.DisplayName() +
               "_" + ForeignKey.PrincipalEntityType.DisplayName() +
               "_" + string.Join("_", ForeignKey.Properties.Select(p => p.Name));
    }
}
