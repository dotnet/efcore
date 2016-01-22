// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Conventions.Internal
{
    public class RelationalPropertyDiscoveryConvention : PropertyDiscoveryConvention
    {
        private readonly IRelationalTypeMapper _typeMapper;

        public RelationalPropertyDiscoveryConvention([NotNull] IRelationalTypeMapper typeMapper)
        {
            Check.NotNull(typeMapper, nameof(typeMapper));

            _typeMapper = typeMapper;
        }

        protected override bool IsCandidatePrimitiveProperty(PropertyInfo propertyInfo)
        {
            Check.NotNull(propertyInfo, nameof(propertyInfo));

            return propertyInfo.IsCandidateProperty() && _typeMapper.IsTypeMapped(propertyInfo.PropertyType);
        }
    }
}
