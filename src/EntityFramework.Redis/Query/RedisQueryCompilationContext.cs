// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;

namespace Microsoft.Data.Entity.Redis.Query
{
    public class RedisQueryCompilationContext : QueryCompilationContext
    {
        public RedisQueryCompilationContext(
            [NotNull] IModel model,
            [NotNull] ILinqOperatorProvider linqOperatorProvider,
            [NotNull] IResultOperatorHandler resultOperatorHandler)
            : base(model, linqOperatorProvider, resultOperatorHandler)
        {
        }

        public override EntityQueryModelVisitor CreateQueryModelVisitor(
            EntityQueryModelVisitor parentEntityQueryModelVisitor)
        {
            return new RedisQueryModelVisitor(this);
        }
    }
}
