// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Redis.Query
{
    public class RedisQueryCompilationContext : QueryCompilationContext
    {
        private readonly IQueryMethodProvider _queryMethodProvider;

        public RedisQueryCompilationContext(
            [NotNull] IModel model,
            [NotNull] ILogger logger,
            [NotNull] ILinqOperatorProvider linqOperatorProvider,
            [NotNull] IResultOperatorHandler resultOperatorHandler,
            [NotNull] IQueryMethodProvider queryMethodProvider,
            [NotNull] EntityMaterializerSource entityMaterializerSource)
            : base(
                Check.NotNull(model, "model"),
                Check.NotNull(logger, "logger"),
                Check.NotNull(linqOperatorProvider, "linqOperatorProvider"),
                Check.NotNull(resultOperatorHandler, "resultOperatorHandler"),
                Check.NotNull(entityMaterializerSource, "entityMaterializerSource"))
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
