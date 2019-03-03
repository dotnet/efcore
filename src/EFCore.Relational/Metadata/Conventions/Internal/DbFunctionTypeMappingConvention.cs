// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class DbFunctionTypeMappingConvention : IModelBuiltConvention
    {
        private readonly IRelationalTypeMappingSource _typeMappingSource;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public DbFunctionTypeMappingConvention([NotNull] IRelationalTypeMappingSource typeMappingSource)
        {
            _typeMappingSource = typeMappingSource;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalModelBuilder Apply(InternalModelBuilder modelBuilder)
        {
            foreach (var dbFunction in ((IModel)modelBuilder.Metadata).GetDbFunctions())
            {
                var mutableDbFunc = modelBuilder.HasDbFunction(dbFunction.MethodInfo);
                mutableDbFunc.Metadata.ReturnTypeMapping = _typeMappingSource.FindMapping(dbFunction.MethodInfo.ReturnType);

                foreach (var dbFuncParam in dbFunction.Parameters.Cast<IMutableDbFunctionParameter>())
                {
                    dbFuncParam.TypeMapping = _typeMappingSource.FindMapping(dbFuncParam.ClrType);
                }
            }

            return modelBuilder;
        }
    }
}
