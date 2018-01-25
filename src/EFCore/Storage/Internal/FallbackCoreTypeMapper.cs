// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class FallbackCoreTypeMapper : CoreTypeMapperBase
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        private readonly ITypeMapper _typeMapper;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public FallbackCoreTypeMapper(
            [NotNull] CoreTypeMapperDependencies dependencies,
            [NotNull] ITypeMapper typeMapper)
            : base(dependencies)
        {
            Check.NotNull(typeMapper, nameof(typeMapper));

            _typeMapper = typeMapper;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override CoreTypeMapping FindMapping(TypeMappingInfo mappingInfo)
        {
            Check.NotNull(mappingInfo, nameof(mappingInfo));

            return _typeMapper.IsTypeMapped(mappingInfo.TargetClrType)
                ? new CoreTypeMapping(mappingInfo.TargetClrType)
                : null;
        }
    }
}
