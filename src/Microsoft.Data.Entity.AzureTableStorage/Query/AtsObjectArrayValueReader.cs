// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.AzureTableStorage.Query
{
    public class AtsObjectArrayValueReader : ObjectArrayValueReader
    {
        public AtsObjectArrayValueReader(object[] valueBuffer)
            : base(valueBuffer)
        {
        }

        public override T ReadValue<T>(int index)
        {
            try
            {
                return base.ReadValue<T>(index);
            }
            catch (InvalidCastException)
            {
                var readValue = base.ReadValue<string>(index);
                var parsed = FromString<T>(readValue);
                if (parsed != null)
                {
                    return (T)parsed;
                }
                throw;
            }
        }

        private static object FromString<T>(string readValue)
        {
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
                if (double.TryParse(readValue, out d))
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
                if (DateTimeOffset.TryParse(readValue,CultureInfo.InvariantCulture,DateTimeStyles.AdjustToUniversal, out d))
                {
                    return d;
                }
            }
            else if (typeof(DateTime).IsAssignableFrom(typeof(T)))
            {
                DateTime d;
                if (DateTime.TryParse(readValue,CultureInfo.InvariantCulture,DateTimeStyles.AdjustToUniversal, out d))
                {
                    return d;
                }
            }
            return null;
        }


    }
}
