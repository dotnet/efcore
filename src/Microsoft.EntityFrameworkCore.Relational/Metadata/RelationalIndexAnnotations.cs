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
    public class RelationalIndexAnnotations : IRelationalIndexAnnotations
    {
        protected const string DefaultIndexNamePrefix = "IX";

        protected readonly RelationalFullAnnotationNames ProviderFullAnnotationNames;

        public RelationalIndexAnnotations([NotNull] IIndex index,
            [CanBeNull] RelationalFullAnnotationNames providerFullAnnotationNames)
            : this(new RelationalAnnotations(index), providerFullAnnotationNames)
        {
        }

        protected RelationalIndexAnnotations([NotNull] RelationalAnnotations annotations,
            [CanBeNull] RelationalFullAnnotationNames providerFullAnnotationNames)
        {
            Annotations = annotations;
            ProviderFullAnnotationNames = providerFullAnnotationNames;
        }

        protected virtual RelationalAnnotations Annotations { get; }
        protected virtual IIndex Index => (IIndex)Annotations.Metadata;

        protected virtual IRelationalEntityTypeAnnotations GetAnnotations([NotNull] IEntityType entityType)
            => new RelationalEntityTypeAnnotations(entityType, ProviderFullAnnotationNames);

        protected virtual IRelationalPropertyAnnotations GetAnnotations([NotNull] IProperty property)
            => new RelationalPropertyAnnotations(property, ProviderFullAnnotationNames);

        public virtual string Name
        {
            get
            {
                return (string)Annotations.GetAnnotation(RelationalFullAnnotationNames.Instance.Name, ProviderFullAnnotationNames?.Name)
                       ?? GetDefaultName();
            }
            [param: CanBeNull] set { SetName(value); }
        }

        public virtual string Filter
        {
            get
            {
                return (string)Annotations.GetAnnotation(RelationalFullAnnotationNames.Instance.Filter, ProviderFullAnnotationNames?.Filter);
            }
            [param: CanBeNull] set { SetFilter(value); }
        }

        protected virtual bool SetFilter([CanBeNull] string value)
            => Annotations.SetAnnotation(
                RelationalFullAnnotationNames.Instance.Filter,
                ProviderFullAnnotationNames?.Filter,
                Check.NullButNotEmpty(value, nameof(value)));

        protected virtual bool SetName([CanBeNull] string value)
            => Annotations.SetAnnotation(
                RelationalFullAnnotationNames.Instance.Name,
                ProviderFullAnnotationNames?.Name,
                Check.NullButNotEmpty(value, nameof(value)));

        protected virtual string GetDefaultName()
            => GetDefaultIndexName(
                GetAnnotations(Index.DeclaringEntityType).TableName,
                Index.Properties.Select(p => GetAnnotations(p).ColumnName));

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
