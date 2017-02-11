// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class InternalDbFunctionBuilder : InternalMetadataItemBuilder<DbFunction>
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public InternalDbFunctionBuilder([NotNull] DbFunction metadata, [NotNull] InternalModelBuilder modelBuilder)
            : base(metadata, modelBuilder)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void HasSchema([CanBeNull]string schema) => Metadata.Schema = schema;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void HasName([CanBeNull] string name) => Metadata.Name = name;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalDbFunctionParameterBuilder Parameter([NotNull] string name)
            => new InternalDbFunctionParameterBuilder(GetOrCreateParameter(name), ModelBuilder);

        private DbFunctionParameter GetOrCreateParameter([NotNull] string name)
            => Metadata.FindParameter(name) ?? Metadata.AddParameter(name);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void RemoveParameter([NotNull] string name, bool shiftParameters = false)
            => Metadata.RemoveParameter(name, shiftParameters);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalDbFunctionBuilder BeforeInitialization([NotNull] Func<MethodCallExpression, IDbFunction, bool> beforeCallback)
        {
            Check.NotNull(beforeCallback, nameof(beforeCallback));

            Metadata.BeforeDbFunctionExpressionCreateCallback = beforeCallback;

            return this;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalDbFunctionBuilder AfterInitialization([NotNull] Action<IDbFunction, DbFunctionExpression> afterCallback)
        {
            Check.NotNull(afterCallback, nameof(afterCallback));

            Metadata.AfterDbFunctionExpressionCreateCallback = afterCallback;

            return this;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalDbFunctionBuilder TranslateWith([NotNull] Func<IReadOnlyCollection<Expression>, IDbFunction, Expression> translateCallback)
        {
            Check.NotNull(translateCallback, nameof(translateCallback));

            Metadata.TranslateCallback = translateCallback;

            return this;
        }
    }
}
