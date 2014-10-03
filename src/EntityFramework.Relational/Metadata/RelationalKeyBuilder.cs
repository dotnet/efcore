// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Utilities;

namespace Microsoft.Data.Entity.Relational.Metadata
{
    public class RelationalKeyBuilder
    {
        private readonly Key _key;

        public RelationalKeyBuilder([NotNull] Key key)
        {
            Check.NotNull(key, "key");

            _key = key;
        }

        public virtual RelationalKeyBuilder Name([CanBeNull] string name)
        {
            Check.NullButNotEmpty(name, "name");

            _key.Relational().Name = name;

            return this;
        }
    }
}
