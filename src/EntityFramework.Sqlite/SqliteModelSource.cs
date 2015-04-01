// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;

namespace Microsoft.Data.Entity.Sqlite
{
    public class SqliteModelSource : ModelSource, ISqliteModelSource
    {
        public SqliteModelSource([NotNull] IDbSetFinder setFinder, [NotNull] IModelValidator modelValidator)
            : base(setFinder, modelValidator)
        {
        }
    }
}
