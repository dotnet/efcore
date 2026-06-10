// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.CompilerServices;

namespace Microsoft.EntityFrameworkCore.Storage;

/// <summary>
///     <para>
///         Represents a set of indexed values. Typically used to represent a row of data returned from a database.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     for more information and examples.
/// </remarks>
public readonly struct ValueBuffer : IEquatable<ValueBuffer>
{
    /// <summary>
    ///     A buffer with no values in it.
    /// </summary>
    public static readonly ValueBuffer Empty = new();

    private readonly object?[] _values;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ValueBuffer" /> class.
    /// </summary>
    /// <param name="values">The list of values for this buffer.</param>
    public ValueBuffer(object?[] values)
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        Check.DebugAssert(values != null, "values is null");

        _values = values;
    }

    /// <summary>
    ///     Gets the value at a requested index.
    /// </summary>
    /// <param name="index">The index of the value to get.</param>
    /// <returns>The value at the requested index.</returns>
    public object? this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _values[index];

        set => _values[index] = value;
    }

    internal static readonly PropertyInfo Indexer
        = typeof(ValueBuffer).GetRuntimeProperties().Single(p => p.GetIndexParameters().Length > 0);

    internal static readonly MethodInfo GetValueMethod
        = Indexer.GetMethod!;

    /// <summary>
    ///     Gets the number of values in this buffer.
    /// </summary>
    public int Count
        => _values.Length;

    /// <summary>
    ///     Gets a value indicating whether the value buffer is empty.
    /// </summary>
    public bool IsEmpty
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        => _values == null;

    /// <summary>
    ///     Determines if this value buffer is equivalent to a given object (i.e. if they are both value buffers and contain the same values).
    /// </summary>
    /// <param name="obj">
    ///     The object to compare this value buffer to.
    /// </param>
    /// <returns>
    ///     <see langword="true" /> if the object is a <see cref="ValueBuffer" /> and contains the same values, otherwise <see langword="false" />.
    /// </returns>
    public override bool Equals(object? obj)
        => obj is ValueBuffer buffer && Equals(buffer);

    /// <summary>
    ///     Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">
    ///     An object to compare with this object.
    /// </param>
    /// <returns>
    ///     <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.
    /// </returns>
    public bool Equals(ValueBuffer other)
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
