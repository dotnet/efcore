// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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

        protected readonly RelationalFullAnnotationNames ProviderFullAnnotationNames;

        public RelationalForeignKeyAnnotations([NotNull] IForeignKey foreignKey,
            [CanBeNull] RelationalFullAnnotationNames providerFullAnnotationNames)
            : this(new RelationalAnnotations(foreignKey), providerFullAnnotationNames)
        {
        }

        protected RelationalForeignKeyAnnotations([NotNull] RelationalAnnotations annotations,
            [CanBeNull] RelationalFullAnnotationNames providerFullAnnotationNames)
        {
            Annotations = annotations;
            ProviderFullAnnotationNames = providerFullAnnotationNames;
        }

        protected virtual RelationalAnnotations Annotations { get; }
        protected virtual IForeignKey ForeignKey => (IForeignKey)Annotations.Metadata;

        protected virtual IRelationalEntityTypeAnnotations GetAnnotations([NotNull] IEntityType entityType)
            => new RelationalEntityTypeAnnotations(entityType, ProviderFullAnnotationNames);

        protected virtual IRelationalPropertyAnnotations GetAnnotations([NotNull] IProperty property)
            => new RelationalPropertyAnnotations(property, ProviderFullAnnotationNames);

        public virtual string Name
        {
            get
            {
                return (string)Annotations.GetAnnotation(
                           RelationalFullAnnotationNames.Instance.Name,
                           ProviderFullAnnotationNames?.Name)
                       ?? GetDefaultName();
            }
            [param: CanBeNull] set { SetName(value); }
        }

        protected virtual bool SetName([CanBeNull] string value)
            => Annotations.SetAnnotation(
                RelationalFullAnnotationNames.Instance.Name,
                ProviderFullAnnotationNames?.Name,
                Check.NullButNotEmpty(value, nameof(value)));

        protected virtual string GetDefaultName()
        {
            return GetDefaultForeignKeyName(
                GetAnnotations(ForeignKey.DeclaringEntityType).TableName,
                GetAnnotations(ForeignKey.PrincipalEntityType).TableName,
                ForeignKey.Properties.Select(p => GetAnnotations(p).ColumnName));
        }

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
