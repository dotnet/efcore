// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Metadata
{
    public class ReadOnlyRelationalKeyExtensions : IRelationalKeyExtensions
    {
        protected const string NameAnnotation = RelationalAnnotationNames.Prefix + RelationalAnnotationNames.Name;

        private readonly IKey _key;

        public ReadOnlyRelationalKeyExtensions([NotNull] IKey key)
        {
            Check.NotNull(key, "key");

            _key = key;
        }

        public virtual string Name
        {
            get { return _key[NameAnnotation]; }
        }

        protected virtual IKey Key
        {
            get { return _key; }
        }
    }
}
