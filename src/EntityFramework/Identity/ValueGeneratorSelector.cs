// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Identity
{
    public class ValueGeneratorSelector
    {
        private readonly SimpleValueGeneratorFactory<GuidValueGenerator> _guidFactory;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected ValueGeneratorSelector()
        {
        }

        public ValueGeneratorSelector(
            [NotNull] SimpleValueGeneratorFactory<GuidValueGenerator> guidFactory)
        {
            Check.NotNull(guidFactory, "guidFactory");

            _guidFactory = guidFactory;
        }

        public virtual IValueGeneratorFactory Select([NotNull] IProperty property)
        {
            Check.NotNull(property, "property");

            if (property.ValueGeneration != ValueGeneration.OnAdd)
            {
                return null;
            }

            if (property.PropertyType == typeof(Guid))
            {
                return _guidFactory;
            }

            throw new NotSupportedException(
                Strings.FormatNoValueGenerator(property.Name, property.EntityType.Name, property.PropertyType.Name));
        }

        public virtual SimpleValueGeneratorFactory<GuidValueGenerator> GuidFactory
        {
            get { return _guidFactory; }
        }
    }
}
