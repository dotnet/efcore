// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class DefaultDbFunctionConvention : IDbFunctionConvention
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalDbFunctionBuilder Apply([NotNull] InternalDbFunctionBuilder dbFunctionBuilder)
        {
            Check.NotNull(dbFunctionBuilder, nameof(dbFunctionBuilder));

            var dbFuncMethodInfo = dbFunctionBuilder.Metadata.MethodInfo;

            var dbFuncAttribute = dbFuncMethodInfo.GetCustomAttributes<DbFunctionAttribute>().SingleOrDefault();

            dbFunctionBuilder.HasName(dbFuncAttribute?.Name ?? dbFuncMethodInfo.Name);
            dbFunctionBuilder.HasSchema(dbFuncAttribute?.Schema ?? "");

            var parameters = dbFuncMethodInfo.GetParameters()
                                .Where(p => p.ParameterType != typeof(DbFunctions))
                                .Select((pi, i) => new
                                {
                                    ParameterIndex = i,
                                    ParameterInfo = pi,
                                    DbFuncParamAttr = pi.GetCustomAttributes<DbFunctionParameterAttribute>().SingleOrDefault(),
                                    IsParams = pi.CustomAttributes.Any(ca => ca.AttributeType == typeof(ParamArrayAttribute)),
                                    ParameterType = pi.ParameterType
                                });

            foreach (var p in parameters)
            {
                var paramBuilder = dbFunctionBuilder.Parameter(p.ParameterInfo.Name);

                paramBuilder.IsIdentifier(p.DbFuncParamAttr?.IsIdentifier ?? false);
                paramBuilder.HasParameterIndex(p.DbFuncParamAttr?.ParameterIndex ?? p.ParameterIndex);
                paramBuilder.IsParams(p.IsParams);
                paramBuilder.HasType(p.ParameterType);

                if (p.DbFuncParamAttr?.Value != null)
                    paramBuilder.HasValue(p.DbFuncParamAttr?.Value);
            }

            return dbFunctionBuilder;
        }
    }
}
