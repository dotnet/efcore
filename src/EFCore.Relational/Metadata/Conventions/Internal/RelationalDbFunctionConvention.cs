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
                var methodInfo = dbFunctionBuilder.Metadata.MethodInfo;
                var dbFunctionAttribute = methodInfo.GetCustomAttributes<DbFunctionAttribute>().SingleOrDefault();

                dbFunctionBuilder.HasName(dbFunctionAttribute?.FunctionName ?? methodInfo.Name, ConfigurationSource.Convention);
                dbFunctionBuilder.HasSchema(dbFunctionAttribute?.Schema ?? modelBuilder.Metadata.Relational().DefaultSchema, ConfigurationSource.Convention);
            }

            return annotation;
        }
    }
}
