// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class ReadOnlyRelationalIndexAnnotations : IRelationalIndexAnnotations
    {
        protected const string NameAnnotation = RelationalAnnotationNames.Prefix + RelationalAnnotationNames.Name;

        private readonly IIndex _index;

        public ReadOnlyRelationalIndexAnnotations([NotNull] IIndex index)
        {
            Check.NotNull(index, nameof(index));

            _index = index;
        }

        public virtual string Name => _index[NameAnnotation] as string ?? GetDefaultName();

        protected virtual IIndex Index => _index;

        protected virtual string GetDefaultName()
            => "IX_" +
               _index.EntityType.DisplayName() +
               "_" +
               string.Join("_", _index.Properties.Select(p => p.Name));
    }
}
