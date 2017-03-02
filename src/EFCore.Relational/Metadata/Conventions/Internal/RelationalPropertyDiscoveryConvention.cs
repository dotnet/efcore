// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class RelationalPropertyDiscoveryConvention : PropertyDiscoveryConvention
    {
        private readonly IRelationalTypeMapper _typeMapper;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public RelationalPropertyDiscoveryConvention([NotNull] IRelationalTypeMapper typeMapper)
        {
            Check.NotNull(typeMapper, nameof(typeMapper));

            _typeMapper = typeMapper;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override bool IsCandidatePrimitiveProperty(PropertyInfo propertyInfo)
        {
            Check.NotNull(propertyInfo, nameof(propertyInfo));

            return propertyInfo.IsCandidateProperty() && _typeMapper.IsTypeMapped(propertyInfo.PropertyType);
        }
    }
}
