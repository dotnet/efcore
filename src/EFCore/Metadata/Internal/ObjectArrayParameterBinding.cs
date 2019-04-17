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
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class ObjectArrayParameterBinding : ParameterBinding
    {
        private readonly IReadOnlyList<ParameterBinding> _bindings;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public ObjectArrayParameterBinding([NotNull] IReadOnlyList<ParameterBinding> bindings)
            : base(typeof(object[]), bindings.SelectMany(b => b.ConsumedProperties).ToArray())
        {
            _bindings = bindings;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
