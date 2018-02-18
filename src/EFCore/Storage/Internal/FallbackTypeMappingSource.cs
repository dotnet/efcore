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
    public class FallbackTypeMappingSource : TypeMappingSource
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
#pragma warning disable 618
        private readonly ITypeMapper _typeMapper;
#pragma warning restore 618

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public FallbackTypeMappingSource(
            [NotNull] TypeMappingSourceDependencies dependencies,
#pragma warning disable 618
            [NotNull] ITypeMapper typeMapper)
#pragma warning restore 618
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

            return _typeMapper.IsTypeMapped(mappingInfo.ProviderClrType)
                ? new CoreTypeMapping(mappingInfo.ProviderClrType)
                : null;
        }
    }
}
