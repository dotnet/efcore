// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
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
    }
}
