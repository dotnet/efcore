// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class RelationalForeignKeyAnnotations : IRelationalForeignKeyAnnotations
    {
        protected const string DefaultForeignKeyNamePrefix = "FK";

        public RelationalForeignKeyAnnotations([NotNull] IForeignKey foreignKey)
            : this(new RelationalAnnotations(foreignKey))
        {
        }

        protected RelationalForeignKeyAnnotations([NotNull] RelationalAnnotations annotations)
            => Annotations = annotations;

        protected virtual RelationalAnnotations Annotations { get; }
        protected virtual IForeignKey ForeignKey => (IForeignKey)Annotations.Metadata;

        protected virtual IRelationalEntityTypeAnnotations GetAnnotations([NotNull] IEntityType entityType)
            => new RelationalEntityTypeAnnotations(entityType);

        protected virtual RelationalForeignKeyAnnotations GetAnnotations([NotNull] IForeignKey foreignKey)
            => new RelationalForeignKeyAnnotations(foreignKey);

        protected virtual IRelationalPropertyAnnotations GetAnnotations([NotNull] IProperty property)
            => new RelationalPropertyAnnotations(property);

        public virtual string Name
        {
            get => (string)Annotations.GetAnnotation(RelationalAnnotationNames.Name)
                   ?? GetDefaultName();

            [param: CanBeNull] set => SetName(value);
        }

        protected virtual bool SetName([CanBeNull] string value)
            => Annotations.SetAnnotation(
                RelationalAnnotationNames.Name,
                Check.NullButNotEmpty(value, nameof(value)));

        protected virtual string GetDefaultName()
        {
            var otherForeignKeyNames = new HashSet<string>(
                ForeignKey.DeclaringEntityType.RootType().GetDerivedTypesInclusive()
                    .SelectMany(et => et.GetDeclaredForeignKeys())
                    .Where(fk => fk != ForeignKey)
                    .Select(GetAnnotations)
                    .Where(a => !ConfigurationSource.Convention.Overrides(a.GetNameConfigurationSource()))
                    .Select(a => a.Name),
                StringComparer.OrdinalIgnoreCase);

            var baseName = GetDefaultForeignKeyName(
                GetAnnotations(ForeignKey.DeclaringEntityType).TableName,
                GetAnnotations(ForeignKey.PrincipalEntityType).TableName,
                ForeignKey.Properties.Select(p => GetAnnotations(p).ColumnName));

            var name = baseName;
            var index = 0;
            while (otherForeignKeyNames.Contains(name))
            {
                name = baseName + index++;
            }

            return name;
        }

        protected virtual ConfigurationSource? GetNameConfigurationSource()
            => (ForeignKey as ForeignKey)
                ?.FindAnnotation(RelationalAnnotationNames.Name)
                ?.GetConfigurationSource();

        public static string GetDefaultForeignKeyName(
            [NotNull] string dependentTableName,
            [NotNull] string principalTableName,
            [NotNull] IEnumerable<string> dependentEndPropertyNames)
        {
            Check.NotEmpty(dependentTableName, nameof(dependentTableName));
            Check.NotEmpty(principalTableName, nameof(principalTableName));
            Check.NotNull(dependentEndPropertyNames, nameof(dependentEndPropertyNames));

            return new StringBuilder()
                .Append(DefaultForeignKeyNamePrefix)
                .Append("_")
                .Append(dependentTableName)
                .Append("_")
                .Append(principalTableName)
                .Append("_")
                .AppendJoin(dependentEndPropertyNames, "_")
                .ToString();
        }
    }
}
