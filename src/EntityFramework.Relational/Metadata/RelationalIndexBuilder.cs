// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Metadata
{
    public class RelationalIndexBuilder
    {
        private readonly Index _index;

        public RelationalIndexBuilder([NotNull] Index index)
        {
            Check.NotNull(index, "index");

            _index = index;
        }

        public virtual RelationalIndexBuilder Name([CanBeNull] string name)
        {
            Check.NullButNotEmpty(name, "name");

            _index.Relational().Name = name;

            return this;
        }
    }
}
