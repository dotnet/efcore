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
    public class RelationalIndexAnnotations : IRelationalIndexAnnotations
    {
        protected const string DefaultIndexNamePrefix = "IX";

        public RelationalIndexAnnotations([NotNull] IIndex index)
            : this(new RelationalAnnotations(index))
        {
        }

        protected RelationalIndexAnnotations([NotNull] RelationalAnnotations annotations)
            => Annotations = annotations;

        protected virtual RelationalAnnotations Annotations { get; }

        protected virtual IIndex Index => (IIndex)Annotations.Metadata;

        protected virtual IRelationalEntityTypeAnnotations GetAnnotations([NotNull] IEntityType entityType)
            => new RelationalEntityTypeAnnotations(entityType);

        protected virtual RelationalIndexAnnotations GetAnnotations([NotNull] IIndex index)
            => new RelationalIndexAnnotations(index);

        protected virtual IRelationalPropertyAnnotations GetAnnotations([NotNull] IProperty property)
            => new RelationalPropertyAnnotations(property);

        public virtual string Name
        {
            get => (string)Annotations.GetAnnotation(RelationalAnnotationNames.Name)
                   ?? GetDefaultName();

            [param: CanBeNull] set => SetName(value);
        }

        public virtual string Filter
        {
            get => (string)Annotations.GetAnnotation(RelationalAnnotationNames.Filter);
            [param: CanBeNull] set => SetFilter(value);
        }

        protected virtual bool SetFilter([CanBeNull] string value)
            => Annotations.SetAnnotation(
                RelationalAnnotationNames.Filter,
                Check.NullButNotEmpty(value, nameof(value)));

        protected virtual bool SetName([CanBeNull] string value)
            => Annotations.SetAnnotation(
                RelationalAnnotationNames.Name,
                Check.NullButNotEmpty(value, nameof(value)));

        protected virtual string GetDefaultName()
        {
            var otherIndexNames = new HashSet<string>(
                Index.DeclaringEntityType.RootType().GetDerivedTypesInclusive()
                    .SelectMany(et => et.GetDeclaredIndexes())
                    .Where(i => i != Index)
                    .Select(GetAnnotations)
                    .Where(a => !ConfigurationSource.Convention.Overrides(a.GetNameConfigurationSource()))
                    .Select(a => a.Name),
                StringComparer.OrdinalIgnoreCase);

            var baseName = GetDefaultIndexName(
                GetAnnotations(Index.DeclaringEntityType).TableName,
                Index.Properties.Select(p => GetAnnotations(p).ColumnName));

            var name = baseName;
            var index = 0;
            while (otherIndexNames.Contains(name))
            {
                name = baseName + index++;
            }

            return name;
        }

        protected virtual ConfigurationSource? GetNameConfigurationSource()
            => (Index as Index)
                ?.FindAnnotation(RelationalAnnotationNames.Name)
                ?.GetConfigurationSource();

        public static string GetDefaultIndexName(
            [NotNull] string tableName,
            [NotNull] IEnumerable<string> propertyNames)
        {
            Check.NotEmpty(tableName, nameof(tableName));
            Check.NotNull(propertyNames, nameof(propertyNames));

            return new StringBuilder()
                .Append(DefaultIndexNamePrefix)
                .Append("_")
                .Append(tableName)
                .Append("_")
                .AppendJoin(propertyNames, "_")
                .ToString();
        }
    }
}
