// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class RelationalIndexAnnotations : IRelationalIndexAnnotations
    {
        public RelationalIndexAnnotations([NotNull] IIndex key, [CanBeNull] string providerPrefix)
            : this(new RelationalAnnotations(key, providerPrefix))
        {
        }

        protected RelationalIndexAnnotations([NotNull] RelationalAnnotations annotations)
        {
            Annotations = annotations;
        }

        protected virtual RelationalAnnotations Annotations { get; }

        protected virtual IIndex Index => (IIndex)Annotations.Metadata;

        public virtual string Name
        {
            get { return (string)Annotations.GetAnnotation(RelationalAnnotationNames.Name) ?? GetDefaultName(); }
            [param: CanBeNull] set { SetName(value); }
        }

        protected virtual bool SetName([CanBeNull] string value)
            => Annotations.SetAnnotation(RelationalAnnotationNames.Name, Check.NullButNotEmpty(value, nameof(value)));

        protected virtual string GetDefaultName()
        {
            var entityType = new RelationalEntityTypeAnnotations(Index.DeclaringEntityType, Annotations.ProviderPrefix);

            return GetDefaultIndexName(entityType.TableName, Index.Properties.Select(p => p.Name));
        }

        public static string GetDefaultIndexName(
            [NotNull] string tableName, [NotNull] IEnumerable<string> propertyNames)
        {
            return "IX_" + tableName + "_" + string.Join("_", propertyNames);
        }
    }
}
