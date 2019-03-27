// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
        private readonly int _offset;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ValueBuffer" /> class.
        /// </summary>
        /// <param name="values"> The list of values for this buffer. </param>
        public ValueBuffer([NotNull] object[] values)
            : this(values, 0)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ValueBuffer" /> class.
        /// </summary>
        /// <param name="values"> The list of values for this buffer. </param>
        /// <param name="offset">
        ///     The starting slot in <paramref name="values" /> for this buffer.
        /// </param>
        public ValueBuffer([NotNull] object[] values, int offset)
        {
            Debug.Assert(values != null);
            Debug.Assert(offset >= 0);

            _values = values;
            _offset = offset;
        }

        /// <summary>
        ///     Gets the value at a requested index.
        /// </summary>
        /// <param name="index"> The index of the value to get. </param>
        /// <returns> The value at the requested index. </returns>
        public object this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[_offset + index];

            [param: CanBeNull] set => _values[_offset + index] = value;
        }

        internal static readonly MethodInfo GetValueMethod
            = typeof(ValueBuffer).GetRuntimeProperties().Single(p => p.GetIndexParameters().Length > 0).GetMethod;

        /// <summary>
        ///     Gets the number of values in this buffer.
        /// </summary>
        public int Count => _values.Length - _offset;

        /// <summary>
        ///     Creates a new buffer with data starting at the given index in the current buffer.
        /// </summary>
        /// <param name="offset">
        ///     The slot in the current buffer that will be the starting slot in the new buffer.
        /// </param>
        /// <returns> The newly created buffer. </returns>
        public ValueBuffer WithOffset(int offset)
        {
            Debug.Assert(offset >= _offset);

            return offset > _offset
                ? new ValueBuffer(_values, offset)
                : this;
        }

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
        {
            return obj is null
                ? false
                : obj is ValueBuffer buffer
                   && Equals(buffer);
        }

        private bool Equals(ValueBuffer other)
            => Equals(_values, other._values)
               && _offset == other._offset;

        /// <summary>
        ///     Gets the hash code for the value buffer.
        /// </summary>
        /// <returns>
        ///     The hash code for the value buffer.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return ((_values?.GetHashCode() ?? 0) * 397) ^ _offset;
            }
        }
    }
}
