// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Storage;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.InMemory
{
    public class InMemoryConnection : DataStoreConnection
    {
        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected InMemoryConnection()
        {
        }

        public InMemoryConnection([NotNull] ILoggerFactory loggerFactory)
            : base(loggerFactory)
        {
        }
    }
}
