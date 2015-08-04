// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Query.Expressions;
using Microsoft.Data.Entity.Query.Internal;
using Microsoft.Data.Entity.Query.Methods;
using Microsoft.Data.Entity.Query.Sql;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Query
{
    public abstract class RelationalQueryCompilationContext : QueryCompilationContext
    {
        private readonly List<RelationalQueryModelVisitor> _relationalQueryModelVisitors
            = new List<RelationalQueryModelVisitor>();

        protected RelationalQueryCompilationContext(
            [NotNull] IModel model,
            [NotNull] ILogger logger,
            [NotNull] ILinqOperatorProvider linqOperatorProvider,
            [NotNull] IResultOperatorHandler resultOperatorHandler,
            [NotNull] IEntityMaterializerSource entityMaterializerSource,
            [NotNull] IEntityKeyFactorySource entityKeyFactorySource,
            [NotNull] IClrAccessorSource<IClrPropertyGetter> clrPropertyGetterSource,
            [NotNull] IQueryMethodProvider queryMethodProvider,
            [NotNull] IMethodCallTranslator compositeMethodCallTranslator,
            [NotNull] IMemberTranslator compositeMemberTranslator,
            [NotNull] IRelationalValueBufferFactoryFactory valueBufferFactoryFactory,
            [NotNull] IRelationalTypeMapper typeMapper,
            [NotNull] IRelationalMetadataExtensionProvider relationalExtensions)
            : base(
                model,
                logger,
                linqOperatorProvider,
                resultOperatorHandler,
                entityMaterializerSource,
                entityKeyFactorySource,
                clrPropertyGetterSource)

        {
            Check.NotNull(queryMethodProvider, nameof(queryMethodProvider));
            Check.NotNull(compositeMethodCallTranslator, nameof(compositeMethodCallTranslator));
            Check.NotNull(compositeMemberTranslator, nameof(compositeMemberTranslator));
            Check.NotNull(valueBufferFactoryFactory, nameof(valueBufferFactoryFactory));
            Check.NotNull(typeMapper, nameof(typeMapper));
            Check.NotNull(relationalExtensions, nameof(relationalExtensions));

            QueryMethodProvider = queryMethodProvider;
            CompositeMethodCallTranslator = compositeMethodCallTranslator;
            CompositeMemberTranslator = compositeMemberTranslator;
            ValueBufferFactoryFactory = valueBufferFactoryFactory;
            TypeMapper = typeMapper;
            RelationalExtensions = relationalExtensions;
        }

        public override EntityQueryModelVisitor CreateQueryModelVisitor(
            EntityQueryModelVisitor parentEntityQueryModelVisitor)
        {
            var relationalQueryModelVisitor
                = new RelationalQueryModelVisitor(
                    this, (RelationalQueryModelVisitor)parentEntityQueryModelVisitor);

            _relationalQueryModelVisitors.Add(relationalQueryModelVisitor);

            return relationalQueryModelVisitor;
        }

        public override IExpressionPrinter CreateExpressionPrinter() => new RelationalExpressionPrinter();

        public virtual bool IsCrossApplySupported => false;

        public virtual SelectExpression FindSelectExpression([NotNull] IQuerySource querySource)
        {
            Check.NotNull(querySource, nameof(querySource));

            return
                (from v in _relationalQueryModelVisitors
                    let selectExpression = v.TryGetQuery(querySource)
                    where selectExpression != null
                    select selectExpression)
                    .First();
        }

        public virtual IQueryMethodProvider QueryMethodProvider { get; }

        public virtual IMethodCallTranslator CompositeMethodCallTranslator { get; }

        public virtual IMemberTranslator CompositeMemberTranslator { get; }

        public virtual IRelationalValueBufferFactoryFactory ValueBufferFactoryFactory { get; }

        public virtual IRelationalTypeMapper TypeMapper { get; }

        public virtual ISqlQueryGenerator CreateSqlQueryGenerator([NotNull] SelectExpression selectExpression)
        {
            Check.NotNull(selectExpression, nameof(selectExpression));

            return new DefaultQuerySqlGenerator(selectExpression, TypeMapper);
        }

        public virtual IRelationalMetadataExtensionProvider RelationalExtensions { get; }
    }
}
