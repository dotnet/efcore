// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ObjectArrayParameterBinding : ParameterBinding
    {
        private readonly IReadOnlyList<ParameterBinding> _bindings;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ObjectArrayParameterBinding([NotNull] IReadOnlyList<ParameterBinding> bindings)
            : base(typeof(object[]), bindings.SelectMany(b => b.ConsumedProperties).ToArray())
        {
            _bindings = bindings;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override Expression BindToParameter(ParameterBindingInfo bindingInfo)
            => Expression.NewArrayInit(
                typeof(object),
                _bindings.Select(
                    b =>
                    {
                        var expression = b.BindToParameter(bindingInfo);

                        if (expression.Type.GetTypeInfo().IsValueType)
                        {
                            expression = Expression.Convert(expression, typeof(object));
                        }

                        return expression;
                    }));
    }
}
