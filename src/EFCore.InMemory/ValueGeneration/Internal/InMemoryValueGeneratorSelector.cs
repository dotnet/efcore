// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.InMemory.Storage.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.InMemory.ValueGeneration.Internal
{
    /// <summary>
    ///     <para>
    ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///         any release. You should only use it directly in your code with extreme caution and knowing that
    ///         doing so can result in application failures when updating to a new Entity Framework Core release.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped"/>. This means that each
    ///         <see cref="DbContext"/> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public class InMemoryValueGeneratorSelector : ValueGeneratorSelector
    {
        private readonly IInMemoryStore _inMemoryStore;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public InMemoryValueGeneratorSelector(
            [NotNull] ValueGeneratorSelectorDependencies dependencies,
            [NotNull] IInMemoryDatabase inMemoryDatabase)
            : base(dependencies)
        {
            _inMemoryStore = inMemoryDatabase.Store;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override ValueGenerator Select(IProperty property, IEntityType entityType)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(entityType, nameof(entityType));

            return property.GetValueGeneratorFactory() == null
                   && property.ClrType.IsInteger()
                   && property.ClrType.UnwrapNullableType() != typeof(char)
                ? GetOrCreate(property)
                : base.Select(property, entityType);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        private ValueGenerator GetOrCreate(IProperty property)
        {
            Check.NotNull(property, nameof(property));

            var type = property.ClrType.UnwrapNullableType().UnwrapEnumType();

            if (type == typeof(long))
            {
                return _inMemoryStore.GetIntegerValueGenerator<long>(property);
            }

            if (type == typeof(int))
            {
                return _inMemoryStore.GetIntegerValueGenerator<int>(property);
            }

            if (type == typeof(short))
            {
                return _inMemoryStore.GetIntegerValueGenerator<short>(property);
            }

            if (type == typeof(byte))
            {
                return _inMemoryStore.GetIntegerValueGenerator<byte>(property);
            }

            if (type == typeof(ulong))
            {
                return _inMemoryStore.GetIntegerValueGenerator<ulong>(property);
            }

            if (type == typeof(uint))
            {
                return _inMemoryStore.GetIntegerValueGenerator<uint>(property);
            }

            if (type == typeof(ushort))
            {
                return _inMemoryStore.GetIntegerValueGenerator<ushort>(property);
            }

            if (type == typeof(sbyte))
            {
                return _inMemoryStore.GetIntegerValueGenerator<sbyte>(property);
            }

            throw new ArgumentException(
                CoreStrings.InvalidValueGeneratorFactoryProperty(
                    "InMemoryIntegerValueGeneratorFactory", property.Name, property.DeclaringEntityType.DisplayName()));
        }
    }
}
