// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Metadata
{
    public class ReadOnlyRelationalIndexExtensions : IRelationalIndexExtensions
    {
        protected const string NameAnnotation = RelationalAnnotationNames.Prefix + RelationalAnnotationNames.Name;

        private readonly IIndex _index;

        public ReadOnlyRelationalIndexExtensions([NotNull] IIndex index)
        {
            Check.NotNull(index, nameof(index));

            _index = index;
        }

        public virtual string Name => _index[NameAnnotation] as string;

        protected virtual IIndex Index => _index;
    }
}
