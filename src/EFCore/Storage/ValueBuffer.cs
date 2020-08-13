// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         Represents a set of indexed values. Typically used to represent a row of data returned from a database.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public readonly struct ValueBuffer
    {
        /// <summary>
        ///     A buffer with no values in it.
        /// </summary>
        public static readonly ValueBuffer Empty = new ValueBuffer();

        private readonly object[] _values;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ValueBuffer" /> class.
        /// </summary>
        /// <param name="values"> The list of values for this buffer. </param>
        public ValueBuffer([NotNull] object[] values)
        {
            Debug.Assert(values != null);

            _values = values;
        }

        /// <summary>
        ///     Gets the value at a requested index.
        /// </summary>
        /// <param name="index"> The index of the value to get. </param>
        /// <returns> The value at the requested index. </returns>
        public object this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[index];

            [param: CanBeNull] set => _values[index] = value;
        }

        internal static readonly MethodInfo GetValueMethod
            = typeof(ValueBuffer).GetRuntimeProperties().Single(p => p.GetIndexParameters().Length > 0).GetMethod;

        /// <summary>
        ///     Gets the number of values in this buffer.
        /// </summary>
        public int Count => _values.Length;

        /// <summary>
        ///     Gets a value indicating whether the value buffer is empty.
        /// </summary>
        public bool IsEmpty => _values == null;

        /// <summary>
        ///     Determines if this value buffer is equivalent to a given object (i.e. if they are both value buffers and contain the same values).
        /// </summary>
        /// <param name="obj">
        ///     The object to compare this value buffer to.
        /// </param>
        /// <returns>
        ///     True if the object is a <see cref="ValueBuffer" /> and contains the same values, otherwise false.
        /// </returns>
        public override bool Equals(object obj)
            => !(obj is null)
                && obj is ValueBuffer buffer
                && Equals(buffer);

        private bool Equals(ValueBuffer other)
        {
            if (_values.Length != other._values.Length)
            {
                return false;
            }

            for (var i = 0; i < _values.Length; i++)
            {
                if (!Equals(_values[i], other._values[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        ///     Gets the hash code for the value buffer.
        /// </summary>
        /// <returns>
        ///     The hash code for the value buffer.
        /// </returns>
        public override int GetHashCode()
        {
            var hash = new HashCode();
            foreach (var value in _values)
            {
                hash.Add(value);
            }

            return hash.ToHashCode();
        }
    }
}
