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
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity
{
    public class DbContextOptions
    {
        private IModel _model;

        private readonly IList<Action<IDbContextOptionsConstruction>> _buildActions
            = new List<Action<IDbContextOptionsConstruction>>();

        public virtual ImmutableDbContextOptions BuildConfiguration()
        {
            return BuildConfiguration(() => new ImmutableDbContextOptions());
        }

        public virtual TConfiguration BuildConfiguration<TConfiguration>([NotNull] Func<TConfiguration> factory)
            where TConfiguration : ImmutableDbContextOptions
        {
            Check.NotNull(factory, "factory");

            var configuration = (IDbContextOptionsConstruction)factory();
            configuration.Model = _model;

            foreach (var buildAction in _buildActions)
            {
                buildAction(configuration);
            }

            configuration.Lock();

            return (TConfiguration)configuration;
        }

        public virtual DbContextOptions UseModel([NotNull] IModel model)
        {
            Check.NotNull(model, "model");

            _model = model;

            return this;
        }

        public virtual DbContextOptions AddBuildAction([NotNull] Action<IDbContextOptionsConstruction> action)
        {
            Check.NotNull(action, "action");

            _buildActions.Add(action);

            return this;
        }
    }
}
