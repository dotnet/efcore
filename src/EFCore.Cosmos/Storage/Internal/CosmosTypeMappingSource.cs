// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class CosmosTypeMappingSource : TypeMappingSource
    {
        private readonly Dictionary<Type, CosmosTypeMapping> _clrTypeMappings;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public CosmosTypeMappingSource([NotNull] TypeMappingSourceDependencies dependencies)
            : base(dependencies)
        {
            _clrTypeMappings
                = new Dictionary<Type, CosmosTypeMapping>
                {
                    { typeof(byte[]), new CosmosTypeMapping(typeof(byte[]), structuralComparer: new ArrayStructuralComparer<byte>()) }
                };
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override CoreTypeMapping FindMapping(in TypeMappingInfo mappingInfo)
        {
            var clrType = mappingInfo.ClrType;
            Debug.Assert(clrType != null);

            if (_clrTypeMappings.TryGetValue(clrType, out var mapping))
            {
                return mapping;
            }

            if (clrType.IsValueType
                || clrType == typeof(string))
            {
                return new CosmosTypeMapping(clrType);
            }

            return base.FindMapping(mappingInfo);
        }
    }
}
