// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class ReadOnlyRelationalKeyAnnotations : IRelationalKeyAnnotations
    {
        protected const string NameAnnotation = RelationalAnnotationNames.Prefix + RelationalAnnotationNames.Name;

        private readonly IKey _key;

        public ReadOnlyRelationalKeyAnnotations([NotNull] IKey key)
        {
            Check.NotNull(key, nameof(key));

            _key = key;
        }

        public virtual string Name => _key[NameAnnotation] as string ?? GetDefaultName();

        protected virtual IKey Key => _key;

        protected virtual string GetDefaultName()
        {
            var builder = new StringBuilder();

            builder
                .Append(_key.IsPrimaryKey() ? "PK_" : "AK_")
                .Append(_key.EntityType.DisplayName());

            if (!_key.IsPrimaryKey())
            {
                builder
                    .Append("_")
                    .Append(string.Join("_", _key.Properties.Select(p => p.Name)));
            }

            return builder.ToString();
        }
    }
}
