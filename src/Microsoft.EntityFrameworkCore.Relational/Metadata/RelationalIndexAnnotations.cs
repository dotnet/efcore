// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata
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

            return "IX_" +
                entityType.TableName +
                "_" +
                string.Join("_", Index.Properties.Select(p => p.Name));
        }
    }
}
