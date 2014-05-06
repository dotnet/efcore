// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Framework.Logging;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq;

namespace Microsoft.Data.Entity.Storage
{
    public abstract class DataStore
    {
        private readonly ILogger _logger;
        private readonly IModel _model;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected DataStore()
        {
        }

        protected DataStore([NotNull] DbContextConfiguration configuration)
        {
            Check.NotNull(configuration, "configuration");

            _logger = configuration.LoggerFactory.Create(GetType().Name);
            _model = configuration.Model;
        }

        public virtual ILogger Logger
        {
            get { return _logger; }
        }

        public virtual IModel Model
        {
            get { return _model; }
        }

        public abstract Task<int> SaveChangesAsync(
            [NotNull] IEnumerable<StateEntry> stateEntries,
            CancellationToken cancellationToken = default(CancellationToken));

        public abstract IEnumerable<TResult> Query<TResult>(
            [NotNull] QueryModel queryModel,
            [NotNull] StateManager stateManager);

        public abstract IAsyncEnumerable<TResult> AsyncQuery<TResult>(
            [NotNull] QueryModel queryModel,
            [NotNull] StateManager stateManager);
    }
}
