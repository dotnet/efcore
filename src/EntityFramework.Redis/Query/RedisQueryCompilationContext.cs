// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Redis.Utilities;

namespace Microsoft.Data.Entity.Redis.Query
{
    public class RedisQueryCompilationContext : QueryCompilationContext
    {
        private readonly IQueryMethodProvider _queryMethodProvider;

        public RedisQueryCompilationContext(
            [NotNull] IModel model,
            [NotNull] ILinqOperatorProvider linqOperatorProvider,
            [NotNull] IResultOperatorHandler resultOperatorHandler,
            [NotNull] IQueryMethodProvider queryMethodProvider)
            : base(model, linqOperatorProvider, resultOperatorHandler)
        {
            Check.NotNull(queryMethodProvider, "queryMethodProvider");

            _queryMethodProvider = queryMethodProvider;
        }

        public virtual IQueryMethodProvider QueryMethodProvider
        {
            get { return _queryMethodProvider; }
        }

        public override EntityQueryModelVisitor CreateQueryModelVisitor(
            EntityQueryModelVisitor parentEntityQueryModelVisitor)
        {
            return new RedisQueryModelVisitor(this);
        }
    }
}
