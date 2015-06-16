// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;

namespace Microsoft.Data.Entity.Metadata
{
    public class TypedAnnotation
    {
        private static readonly HashSet<Type> _supportedTypes = new HashSet<Type>
        {
            typeof(string),
            typeof(int),
            typeof(long),
            typeof(short),
            typeof(byte),
            typeof(decimal),
            typeof(float),
            typeof(double),
            typeof(bool),
            typeof(DateTime),
            typeof(char),
            typeof(sbyte),
            typeof(ulong),
            typeof(uint),
            typeof(ushort),
            typeof(Guid),
            typeof(DateTimeOffset),
            typeof(TimeSpan),
            typeof(byte[])
        };

        private readonly object _value;

        public TypedAnnotation([CanBeNull] object value)
        {
            _value = value;

            if (value != null)
            {
                CheckType(value.GetType());
            }
        }

        public TypedAnnotation([CanBeNull] string typeString, [CanBeNull] string valueString)
        {
            _value = typeString == null || valueString == null
                ? null
                : Deserialize(CheckType(Type.GetType(typeString)), valueString);
        }

        private static Type CheckType(Type type)
        {
            if (!_supportedTypes.Contains(type))
            {
                throw new NotSupportedException(Strings.UnsupportedAnnotationType(type.Name));
            }

            return type;
        }

        public virtual object Value => _value;

        public virtual string ValueString => _value == null ? null : Serialize();

        public virtual string TypeString => _value?.GetType().FullName;

        private string Serialize()
        {
            var type = _value.GetType();

            if (type == typeof(DateTimeOffset))
            {
                var dto = (DateTimeOffset)_value;

                return dto.Ticks + ", " + dto.Offset.Ticks;
            }

            if (type == typeof(DateTime))
            {
                return ((DateTime)_value).Ticks.ToString();
            }

            if (type == typeof(TimeSpan))
            {
                return ((TimeSpan)_value).Ticks.ToString();
            }

            if (type == typeof(byte[]))
            {
                return Convert.ToBase64String((byte[])_value);
            }

            return _value.ToString();
        }

        private static object Deserialize(Type type, string value)
        {
            if (type == typeof(string))
            {
                return value;
            }

            if (type == typeof(Guid))
            {
                return new Guid(value);
            }

            if (type == typeof(DateTimeOffset))
            {
                var commaIndex = value.IndexOf(",");
                return new DateTimeOffset(
                    (long)Deserialize(typeof(long), value.Substring(0, commaIndex)),
                    (TimeSpan)Deserialize(typeof(TimeSpan), value.Substring(commaIndex + 1)));
            }

            if (type == typeof(DateTime))
            {
                return new DateTime((long)Deserialize(typeof(long), value));
            }

            if (type == typeof(TimeSpan))
            {
                return new TimeSpan((long)Deserialize(typeof(long), value));
            }

            if (type == typeof(byte[]))
            {
                return Convert.FromBase64String(value);
            }

            return Convert.ChangeType(value, type);
        }
    }
}
