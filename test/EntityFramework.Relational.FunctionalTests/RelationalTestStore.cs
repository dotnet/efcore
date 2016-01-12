// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public abstract class RelationalTestStore : TestStore
    {
        public abstract DbConnection Connection { get; }
        public abstract DbTransaction Transaction { get; }
        public abstract string ConnectionString { get; }
    }
}
