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
    public class RelationalKeyAnnotations : IRelationalKeyAnnotations
    {
        protected const string DefaultPrimaryKeyNamePrefix = "PK";
        protected const string DefaultAlternateKeyNamePrefix = "AK";

        public RelationalKeyAnnotations([NotNull] IKey key)
            : this(new RelationalAnnotations(key))
        {
        }

        protected RelationalKeyAnnotations([NotNull] RelationalAnnotations annotations)
            => Annotations = annotations;

        protected virtual RelationalAnnotations Annotations { get; }

        protected virtual IKey Key => (IKey)Annotations.Metadata;

        protected virtual IRelationalEntityTypeAnnotations GetAnnotations([NotNull] IEntityType entityType)
            => new RelationalEntityTypeAnnotations(entityType);

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
            => GetDefaultKeyName(
                GetAnnotations(Key.DeclaringEntityType).TableName,
                Key.IsPrimaryKey(),
                Key.Properties.Select(p => GetAnnotations(p).ColumnName));

        public static string GetDefaultKeyName(
            [NotNull] string tableName, bool primaryKey, [NotNull] IEnumerable<string> propertyNames)
        {
            Check.NotEmpty(tableName, nameof(tableName));
            Check.NotNull(propertyNames, nameof(propertyNames));

            var builder = new StringBuilder();

            if (primaryKey)
            {
                builder
                    .Append(DefaultPrimaryKeyNamePrefix)
                    .Append("_")
                    .Append(tableName);
            }
            else
            {
                builder
                    .Append(DefaultAlternateKeyNamePrefix)
                    .Append("_")
                    .Append(tableName)
                    .Append("_")
                    .AppendJoin(propertyNames, "_");
            }

            return builder.ToString();
        }
    }
}
