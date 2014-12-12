// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Identity
{
    public class ValueGeneratorSelector
    {
        private readonly SimpleValueGeneratorFactory<GuidValueGenerator> _guidFactory;
        private readonly SimpleValueGeneratorFactory<TemporaryIntegerValueGenerator> _integerFactory;
        private readonly SimpleValueGeneratorFactory<TemporaryStringValueGenerator> _stringFactory;
        private readonly SimpleValueGeneratorFactory<TemporaryBinaryValueGenerator> _binaryFactory;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected ValueGeneratorSelector()
        {
        }

        public ValueGeneratorSelector(
            [NotNull] SimpleValueGeneratorFactory<GuidValueGenerator> guidFactory,
            [NotNull] SimpleValueGeneratorFactory<TemporaryIntegerValueGenerator> integerFactory,
            [NotNull] SimpleValueGeneratorFactory<TemporaryStringValueGenerator> stringFactory,
            [NotNull] SimpleValueGeneratorFactory<TemporaryBinaryValueGenerator> binaryFactory)
        {
            Check.NotNull(guidFactory, "guidFactory");

            _guidFactory = guidFactory;
            _integerFactory = integerFactory;
            _stringFactory = stringFactory;
            _binaryFactory = binaryFactory;
        }

        public virtual IValueGeneratorFactory Select([NotNull] IProperty property)
        {
            Check.NotNull(property, "property");

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
