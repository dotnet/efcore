// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.SqlServer.Utilities;

namespace Microsoft.Data.Entity.SqlServer.Metadata
{
    public class SqlServerKeyBuilder
    {
        private readonly Key _key;

        public SqlServerKeyBuilder([NotNull] Key key)
        {
            Check.NotNull(key, "key");

            _key = key;
        }

        public virtual SqlServerKeyBuilder Name([CanBeNull] string name)
        {
            Check.NullButNotEmpty(name, "name");

            _key.SqlServer().Name = name;

            return this;
        }
    }
}
