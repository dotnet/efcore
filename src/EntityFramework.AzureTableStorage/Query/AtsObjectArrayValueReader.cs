// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using JetBrains.Annotations;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.AzureTableStorage.Query
{
    //TODO generalize this functionality
    public class AtsObjectArrayValueReader : IValueReader
    {
        private readonly object[] _values;

        public AtsObjectArrayValueReader([NotNull] object[] valueBuffer)
        {
            Check.NotNull(valueBuffer, "valueBuffer");
            _values = valueBuffer;
        }

        public virtual bool IsNull(int index)
        {
            return _values[index] == null;
        }

        public virtual T ReadValue<T>(int index)
        {
            if (IsNull(index))
            {
                return default(T);
            }
            if (_values[index] is T)
            {
                return (T)_values[index];
            }
            if (_values[index] is string)
            {
                var parsed = FromString<T>(_values[index] as string);
                if (parsed != null)
                {
                    return (T)parsed;
                }
            }
            throw new TypeAccessException(Strings.FormatInvalidReadType(typeof(T).Name, index));
        }

        public virtual int Count
        {
            get { return _values.Length; }
        }

        private static object FromString<T>(string readValue)
        {
            if (readValue == null)
            {
                return null;
            }
            if (typeof(int).IsAssignableFrom(typeof(T)))
            {
                int i;
                if (int.TryParse(readValue, out i))
                {
                    return i;
                }
            }
            else if (typeof(double).IsAssignableFrom(typeof(T)))
            {
                double d;
                if (double.TryParse(readValue, NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out d))
                {
                    return d;
                }
            }
            else if (typeof(long).IsAssignableFrom(typeof(T)))
            {
                long l;
                if (long.TryParse(readValue, out l))
                {
                    return l;
                }
            }
            else if (typeof(bool).IsAssignableFrom(typeof(T)))
            {
                bool b;
                if (bool.TryParse(readValue, out b))
                {
                    return b;
                }
            }
            else if (typeof(Guid).IsAssignableFrom(typeof(T)))
            {
                Guid g;
                if (Guid.TryParse(readValue, out g))
                {
                    return g;
                }
            }
            else if (typeof(DateTimeOffset).IsAssignableFrom(typeof(T)))
            {
                DateTimeOffset d;
                if (DateTimeOffset.TryParse(readValue, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out d))
                {
                    return d;
                }
            }
            else if (typeof(DateTime).IsAssignableFrom(typeof(T)))
            {
                DateTime d;
                if (DateTime.TryParse(readValue, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out d))
                {
                    return d;
                }
            }
            return null;
        }
    }
}
