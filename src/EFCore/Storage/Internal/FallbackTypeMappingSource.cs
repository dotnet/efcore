// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
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
        protected override CoreTypeMapping FindMapping(in TypeMappingInfo mappingInfo)
            => _typeMapper.IsTypeMapped(mappingInfo.ClrType)
                ? new ConcreteTypeMapping(mappingInfo.ClrType)
                : base.FindMapping(mappingInfo);

        private class ConcreteTypeMapping : CoreTypeMapping
        {
            private ConcreteTypeMapping(CoreTypeMappingParameters parameters)
                : base(parameters)
            {
            }

            public ConcreteTypeMapping([NotNull] Type clrType)
                : base(new CoreTypeMappingParameters(clrType))
            {
            }

            public override CoreTypeMapping Clone(ValueConverter converter)
                => new ConcreteTypeMapping(Parameters.WithComposedConverter(converter));
        }
    }
}
