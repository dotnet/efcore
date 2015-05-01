// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.Relational.Query.Expressions
{
    public class ValueBufferFactoryExpression : ExtensionExpression
    {
        private readonly IRelationalValueBufferFactoryFactory _factoryFactory;

        public ValueBufferFactoryExpression(
            [NotNull] IRelationalValueBufferFactoryFactory factoryFactory,
            [NotNull] Func<IEnumerable<Type>> typesFactory,
            int offset)
            : base(typeof(IRelationalValueBufferFactory))
        {
            Check.NotNull(factoryFactory, nameof(factoryFactory));
            Check.NotNull(typesFactory, nameof(typesFactory));

            _factoryFactory = factoryFactory;
            TypesFactory = typesFactory;
            Offset = offset;
        }

        public override bool CanReduce => true;

        public override Expression Reduce()
            => Constant(
                _factoryFactory.CreateValueBufferFactory(
                    TypesFactory().Skip(Offset),
                    Offset));

        protected override Expression VisitChildren(ExpressionTreeVisitor visitor) => this;

        public override ExpressionType NodeType => ExpressionType.Extension;

        public virtual int Offset { get; }

        public virtual Func<IEnumerable<Type>> TypesFactory { get; }
    }
}
