// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ValueGeneration
{
    public class ValueGeneratorFactorySelector
    {
        private readonly ValueGeneratorFactory<GuidValueGenerator> _guidFactory;
        private readonly ValueGeneratorFactory<TemporaryIntegerValueGenerator> _integerFactory;
        private readonly ValueGeneratorFactory<TemporaryStringValueGenerator> _stringFactory;
        private readonly ValueGeneratorFactory<TemporaryBinaryValueGenerator> _binaryFactory;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected ValueGeneratorFactorySelector()
        {
        }

        public ValueGeneratorFactorySelector(
            [NotNull] ValueGeneratorFactory<GuidValueGenerator> guidFactory,
            [NotNull] ValueGeneratorFactory<TemporaryIntegerValueGenerator> integerFactory,
            [NotNull] ValueGeneratorFactory<TemporaryStringValueGenerator> stringFactory,
            [NotNull] ValueGeneratorFactory<TemporaryBinaryValueGenerator> binaryFactory)
        {
            Check.NotNull(guidFactory, nameof(guidFactory));
            Check.NotNull(integerFactory, nameof(integerFactory));
            Check.NotNull(stringFactory, nameof(stringFactory));
            Check.NotNull(binaryFactory, nameof(binaryFactory));

            _guidFactory = guidFactory;
            _integerFactory = integerFactory;
            _stringFactory = stringFactory;
            _binaryFactory = binaryFactory;
        }

        public virtual IValueGeneratorFactory Select([NotNull] IProperty property)
        {
            Check.NotNull(property, nameof(property));

            var propertyType = property.PropertyType;

            if (propertyType == typeof(Guid))
            {
                return _guidFactory;
            }

            if (propertyType.UnwrapNullableType().IsInteger())
            {
                return _integerFactory;
            }

            if (propertyType == typeof(string))
            {
                return _stringFactory;
            }

            if (propertyType == typeof(byte[]))
            {
                return _binaryFactory;
            }

            throw new NotSupportedException(
                Strings.NoValueGenerator(property.Name, property.EntityType.SimpleName, propertyType.Name));
        }
    }
}
