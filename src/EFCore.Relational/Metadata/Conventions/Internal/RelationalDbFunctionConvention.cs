// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    public class RelationalDbFunctionConvention : IModelAnnotationChangedConvention
    {
        public virtual Annotation Apply(InternalModelBuilder modelBuilder, string name, Annotation annotation, Annotation oldAnnotation)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NotNull(name, nameof(name));

            if (name.StartsWith(RelationalAnnotationNames.DbFunction, StringComparison.OrdinalIgnoreCase)
                && annotation != null
                && oldAnnotation == null)
            { 
                var dbFunctionBuilder = new InternalDbFunctionBuilder((DbFunction)annotation.Value);

                var dbFuncMethodInfo = dbFunctionBuilder.Metadata.MethodInfo;

                var dbFuncAttribute = dbFuncMethodInfo.GetCustomAttributes<DbFunctionAttribute>().SingleOrDefault();

                dbFunctionBuilder.HasName(dbFuncAttribute?.Name ?? dbFuncMethodInfo.Name, ConfigurationSource.Convention);
                dbFunctionBuilder.HasSchema(dbFuncAttribute?.Schema ?? modelBuilder.Metadata.Relational().DefaultSchema, ConfigurationSource.Convention); 

                var parameters = dbFuncMethodInfo.GetParameters()
                                    .Where(p => p.ParameterType != typeof(DbFunctions))
                                    .Select((pi, i) => new
                                    {
                                        ParameterIndex = i,
                                        ParameterInfo = pi,
                                        DbFuncParamAttr = pi.GetCustomAttributes<DbFunctionParameterAttribute>().SingleOrDefault(),
                                        pi.ParameterType
                                    });

                foreach (var p in parameters)
                {
                    var paramBuilder = dbFunctionBuilder.HasParameter(p.ParameterInfo.Name, ConfigurationSource.Convention);

                    paramBuilder.HasIndex(p.DbFuncParamAttr?.ParameterIndex ?? p.ParameterIndex, ConfigurationSource.Convention);
                    paramBuilder.HasType(p.ParameterType, ConfigurationSource.Convention);
                }
            }

            return annotation;
        }
    }
}
