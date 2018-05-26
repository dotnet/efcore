// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Parsing.ExpressionVisitors.Transformation;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class RelationalIExpressionTranformationProvider : IExpressionTranformationProvider
    {
        private readonly ExpressionTransformerRegistry _transformProvider;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public RelationalIExpressionTranformationProvider([NotNull] IModel model)
        {
            Check.NotNull(model, nameof(model));

            _transformProvider = ExpressionTransformerRegistry.CreateDefault();
            _transformProvider.Register(new RelationalDbFunctionTransformer(model));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<ExpressionTransformation> GetTransformations(Expression expression)
        {
            return _transformProvider.GetTransformations(expression);
        }
    }
}
