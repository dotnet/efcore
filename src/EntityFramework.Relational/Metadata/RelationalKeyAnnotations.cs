// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class RelationalKeyAnnotations : IRelationalKeyAnnotations
    {
        public RelationalKeyAnnotations([NotNull] IKey key, [CanBeNull] string providerPrefix)
            : this(new RelationalAnnotations(key, providerPrefix))
        {
        }

        protected RelationalKeyAnnotations([NotNull] RelationalAnnotations annotations)
        {
            Annotations = annotations;
        }

        protected virtual RelationalAnnotations Annotations { get; }

        protected virtual IKey Key => (IKey)Annotations.Metadata;

        public virtual string Name
        {
            get { return (string)Annotations.GetAnnotation(RelationalAnnotationNames.Name) ?? GetDefaultName(); }
            [param: CanBeNull] set { SetName(value); }
        }

        protected virtual bool SetName([CanBeNull] string value)
            => Annotations.SetAnnotation(RelationalAnnotationNames.Name, Check.NullButNotEmpty(value, nameof(value)));

        protected virtual string GetDefaultName()
        {
            var entityType = new RelationalEntityTypeAnnotations(Key.DeclaringEntityType, Annotations.ProviderPrefix);

            return GetDefaultKeyName(entityType.TableName, Key.IsPrimaryKey(), Key.Properties.Select(p => p.Name));
        }

        public static string GetDefaultKeyName(
            [NotNull] string tableName, bool isPrimaryKey, [NotNull] IEnumerable<string> propertyNames)
        {
            var builder = new StringBuilder();
            builder
                .Append(isPrimaryKey ? "PK_" : "AK_")
                .Append(tableName);

            if (!isPrimaryKey)
            {
                builder
                    .Append("_")
                    .Append(string.Join("_", propertyNames));
            }

            return builder.ToString();
        }
    }
}
