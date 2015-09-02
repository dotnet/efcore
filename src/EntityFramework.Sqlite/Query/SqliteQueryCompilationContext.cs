// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Query.Expressions;
using Microsoft.Data.Entity.Query.ExpressionTranslators;
using Microsoft.Data.Entity.Query.Sql;
using Microsoft.Data.Entity.Storage;
using Microsoft.Framework.Logging;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Query
{
    public class SqliteQueryCompilationContext : RelationalQueryCompilationContext
    {
        public SqliteQueryCompilationContext(
            [NotNull] IModel model,
            [NotNull] ILogger logger,
            [NotNull] ILinqOperatorProvider linqOperatorProvider,
            [NotNull] IResultOperatorHandler resultOperatorHandler,
            [NotNull] IEntityMaterializerSource entityMaterializerSource,
            [NotNull] IClrAccessorSource<IClrPropertyGetter> clrPropertyGetterSource,
            [NotNull] IEntityKeyFactorySource entityKeyFactorySource,
            [NotNull] IQueryMethodProvider queryMethodProvider,
            [NotNull] IMethodCallTranslator compositeMethodCallTranslator,
            [NotNull] IMemberTranslator compositeMemberTranslator,
            [NotNull] IExpressionFragmentTranslator compositeExpressionFragmentTranslator,
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
                clrPropertyGetterSource,
                queryMethodProvider,
                compositeMethodCallTranslator,
                compositeMemberTranslator,
                compositeExpressionFragmentTranslator,
                valueBufferFactoryFactory,
                typeMapper,
                relationalExtensions)
        {
        }

        public override ISqlQueryGenerator CreateSqlQueryGenerator(SelectExpression selectExpression) =>
            new SqliteQuerySqlGenerator(selectExpression, TypeMapper);
    }
}
