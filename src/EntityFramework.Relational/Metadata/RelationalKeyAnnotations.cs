// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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

        protected RelationalAnnotations Annotations { get; }

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
            var builder = new StringBuilder();

            builder
                .Append(Key.IsPrimaryKey() ? "PK_" : "AK_")
                .Append(Key.EntityType.DisplayName());

            if (!Key.IsPrimaryKey())
            {
                builder
                    .Append("_")
                    .Append(string.Join("_", Key.Properties.Select(p => p.Name)));
            }

            return builder.ToString();
        }
    }
}
