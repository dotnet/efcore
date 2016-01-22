// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata
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
        {
            var entityType = new RelationalEntityTypeAnnotations(ForeignKey.DeclaringEntityType, Annotations.ProviderPrefix);
            var principalEntityType = new RelationalEntityTypeAnnotations(
                ForeignKey.PrincipalEntityType,
                Annotations.ProviderPrefix);

            return GetDefaultForeignKeyName(entityType.TableName,
                principalEntityType.TableName, ForeignKey.Properties.Select(p => p.Name));
        }

        public static string GetDefaultForeignKeyName(
            [NotNull] string dependentTableName,
            [NotNull] string principalTableName,
            [NotNull] IEnumerable<string> dependentEndPropertyNames)
        {
            Check.NotEmpty(dependentTableName, nameof(dependentTableName));
            Check.NotEmpty(principalTableName, nameof(principalTableName));
            Check.NotNull(dependentEndPropertyNames, nameof(dependentEndPropertyNames));

            return "FK_" + dependentTableName +
                "_" + principalTableName +
                "_" + string.Join("_", dependentEndPropertyNames);
        }
    }
}
