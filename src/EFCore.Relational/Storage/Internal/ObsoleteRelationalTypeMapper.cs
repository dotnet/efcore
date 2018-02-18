// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [Obsolete("Use RelationalTypeMappingSource.")]
    public class ObsoleteRelationalTypeMapper : IRelationalTypeMapper
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsTypeMapped(Type clrType)
            => throw new InvalidOperationException(CoreStrings.StillUsingTypeMapper);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual RelationalTypeMapping FindMapping(IProperty property)
            => throw new InvalidOperationException(CoreStrings.StillUsingTypeMapper);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual RelationalTypeMapping FindMapping(Type clrType)
            => throw new InvalidOperationException(CoreStrings.StillUsingTypeMapper);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual RelationalTypeMapping FindMapping(string storeType)
            => throw new InvalidOperationException(CoreStrings.StillUsingTypeMapper);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void ValidateTypeName(string storeType)
            => throw new InvalidOperationException(CoreStrings.StillUsingTypeMapper);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IByteArrayRelationalTypeMapper ByteArrayMapper
            => throw new InvalidOperationException(CoreStrings.StillUsingTypeMapper);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IStringRelationalTypeMapper StringMapper
            => throw new InvalidOperationException(CoreStrings.StillUsingTypeMapper);
    }
}
