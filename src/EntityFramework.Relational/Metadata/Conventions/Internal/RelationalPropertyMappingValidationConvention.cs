// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Conventions.Internal
{
    public class RelationalPropertyMappingValidationConvention : PropertyMappingValidationConvention
    {
        private readonly IRelationalTypeMapper _typeMapper;

        public RelationalPropertyMappingValidationConvention([NotNull] IRelationalTypeMapper typeMapper)
        {
            Check.NotNull(typeMapper, nameof(typeMapper));

            _typeMapper = typeMapper;
        }

        public override bool IsMappedPrimitiveProperty(Type clrType) => _typeMapper.IsTypeMapped(clrType);

        public override Type FindCandidateNavigationPropertyType(PropertyInfo propertyInfo)
        {
            Check.NotNull(propertyInfo, nameof(propertyInfo));

            return propertyInfo.FindCandidateNavigationPropertyType(_typeMapper.IsTypeMapped);
        }
    }
}
