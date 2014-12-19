// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Identity
{
    public class TemporaryIntegerValueGenerator : SimpleValueGenerator
    {
        private long _current;

        public override object Next(IProperty property)
        {
            Check.NotNull(property, "property");

            var generatedValue = Interlocked.Decrement(ref _current);
            var targetType = property.PropertyType.UnwrapNullableType();

            if (targetType == typeof(uint))
            {
                return unchecked((uint)generatedValue);
            }

            if (targetType == typeof(ulong))
            {
                return unchecked((ulong)generatedValue);
            }

            if (targetType == typeof(ushort))
            {
                return unchecked((ushort)generatedValue);
            }

            if (targetType == typeof(byte))
            {
                return unchecked((byte)generatedValue);
            }

            return Convert.ChangeType(generatedValue, targetType);
        }

        public override bool GeneratesTemporaryValues => true;
    }
}
