// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Infrastructure;

/// <summary>
///     Provides methods for manipulating string identifiers.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     for more information and examples.
/// </remarks>
public static class Uniquifier
{
    private const int StackAllocationLimit = 512;
    private const int MaxIntLength = 10;

    /// <summary>
    ///     Creates a unique identifier by appending a number to the given string.
    /// </summary>
    /// <typeparam name="T">The type of the object the identifier maps to.</typeparam>
    /// <param name="currentIdentifier">The base identifier.</param>
    /// <param name="otherIdentifiers">A dictionary where the identifier will be used as a key.</param>
    /// <param name="maxLength">The maximum length of the identifier.</param>
    /// <param name="suffix">An optional suffix to add after the uniquifier.</param>
    /// <param name="uniquifier">An optional starting number for the uniquifier.</param>
    /// <returns>A unique identifier.</returns>
    public static string Uniquify<T>(
        string currentIdentifier,
        IReadOnlyDictionary<string, T> otherIdentifiers,
        int maxLength,
        string? suffix = null,
        int uniquifier = 1)
        => Uniquify(currentIdentifier, otherIdentifiers, static s => s, maxLength, suffix, uniquifier);

    /// <summary>
    ///     Creates a unique identifier by appending a number to the given string.
    /// </summary>
    /// <typeparam name="TKey">The type of the key that contains the identifier.</typeparam>
    /// <typeparam name="TValue">The type of the object the identifier maps to.</typeparam>
    /// <param name="currentIdentifier">The base identifier.</param>
    /// <param name="otherIdentifiers">A dictionary where the identifier will be used as part of the key.</param>
    /// <param name="keySelector">Creates the key object from an identifier.</param>
    /// <param name="maxLength">The maximum length of the identifier.</param>
    /// <param name="suffix">An optional suffix to add after the uniquifier.</param>
    /// <param name="uniquifier">An optional starting number for the uniquifier.</param>
    /// <returns>A unique identifier.</returns>
    public static string Uniquify<TKey, TValue>(
        string currentIdentifier,
        IReadOnlyDictionary<TKey, TValue> otherIdentifiers,
        Func<string, TKey> keySelector,
        int maxLength,
        string? suffix = null,
        int uniquifier = 1)
    {
        // this is the maximum buffer size we could possibly need
        var bufferSize = Math.Min(currentIdentifier.Length + (suffix?.Length ?? 0) + MaxIntLength, maxLength);
        var buffer = bufferSize <= StackAllocationLimit
            ? stackalloc char[bufferSize]
            : new char[bufferSize];

        ValidateTruncate(maxLength, suffix, uniquifier: null, out var maxNameLength, out _);
        var finalIdentifier = new string(TruncateImpl(currentIdentifier, buffer, maxNameLength, suffix, uniquifier: null));
        while (otherIdentifiers.ContainsKey(keySelector(finalIdentifier)))
        {
            ValidateTruncate(maxLength, suffix, uniquifier, out maxNameLength, out _);
            finalIdentifier = new string(TruncateImpl(currentIdentifier, buffer, maxNameLength, suffix, uniquifier++));
        }

        return finalIdentifier;
    }

    /// <summary>
    ///     Creates a unique identifier by appending a number to the given string.
    /// </summary>
    /// <typeparam name="T">The type of the object the identifier maps to.</typeparam>
    /// <param name="currentIdentifier">The base identifier.</param>
    /// <param name="otherIdentifiers">A dictionary where the identifier will be used as part of the key.</param>
    /// <param name="maxLength">The maximum length of the identifier.</param>
    /// <param name="suffix">An optional suffix to add after the uniquifier.</param>
    /// <param name="uniquifier">An optional starting number for the uniquifier.</param>
    /// <returns>A unique identifier.</returns>
    public static string Uniquify<T>(
        string currentIdentifier,
        Dictionary<string, T> otherIdentifiers,
        int maxLength,
        string? suffix = null,
        int uniquifier = 1)
        => Uniquify<T>(currentIdentifier, otherIdentifiers, static s => s, maxLength, suffix, uniquifier);

    /// <summary>
    ///     Creates a unique identifier by appending a number to the given string.
    /// </summary>
    /// <typeparam name="T">The type of the object the identifier maps to.</typeparam>
    /// <param name="currentIdentifier">The base identifier.</param>
    /// <param name="otherIdentifiers">A dictionary where the identifier will be used as part of the key.</param>
    /// <param name="keySelector">Creates the key object from an identifier.</param>
    /// <param name="maxLength">The maximum length of the identifier.</param>
    /// <param name="suffix">An optional suffix to add after the uniquifier.</param>
    /// <param name="uniquifier">An optional starting number for the uniquifier.</param>
    /// <returns>A unique identifier.</returns>
    public static string Uniquify<T>(
        string currentIdentifier,
        Dictionary<string, T> otherIdentifiers,
        Func<ReadOnlySpan<char>, ReadOnlySpan<char>> keySelector,
        int maxLength,
        string? suffix = null,
        int uniquifier = 1)
    {
        // this is the maximum buffer size we could possibly need
        var bufferSize = Math.Min(currentIdentifier.Length + (suffix?.Length ?? 0) + MaxIntLength, maxLength);
        var buffer = bufferSize <= StackAllocationLimit
            ? stackalloc char[bufferSize]
            : new char[bufferSize];

        var lookup = otherIdentifiers.GetAlternateLookup<ReadOnlySpan<char>>();
        ValidateTruncate(maxLength, suffix, uniquifier: null, out var maxNameLength, out _);
        var finalIdentifier = TruncateImpl(currentIdentifier, buffer, maxNameLength, suffix, uniquifier: null);
        while (lookup.ContainsKey(keySelector(finalIdentifier)))
        {
            ValidateTruncate(maxLength, suffix, uniquifier, out maxNameLength, out _);
            finalIdentifier = TruncateImpl(currentIdentifier, buffer, maxNameLength, suffix, uniquifier++);
        }

        return new string(finalIdentifier);
    }

    /// <summary>
    ///     Creates a unique identifier by appending a number to the given string.
    /// </summary>
    /// <param name="currentIdentifier">The base identifier.</param>
    /// <param name="otherIdentifiers">A dictionary where the identifier will be used as part of the key.</param>
    /// <param name="maxLength">The maximum length of the identifier.</param>
    /// <param name="suffix">An optional suffix to add after the uniquifier.</param>
    /// <param name="uniquifier">An optional starting number for the uniquifier.</param>
    /// <returns>A unique identifier.</returns>
    public static string Uniquify(
        string currentIdentifier,
        ISet<string> otherIdentifiers,
        int maxLength,
        string? suffix = null,
        int uniquifier = 1)
    {
        // this is the maximum buffer size we could possibly need
        var bufferSize = Math.Min(currentIdentifier.Length + (suffix?.Length ?? 0) + MaxIntLength, maxLength);
        var buffer = bufferSize <= StackAllocationLimit
            ? stackalloc char[bufferSize]
            : new char[bufferSize];

        ValidateTruncate(maxLength, suffix, uniquifier: null, out var maxNameLength, out _);
        var finalIdentifier = new string(TruncateImpl(currentIdentifier, buffer, maxNameLength, suffix, uniquifier: null));
        while (otherIdentifiers.Contains(finalIdentifier))
        {
            ValidateTruncate(maxLength, suffix, uniquifier, out maxNameLength, out _);
            finalIdentifier = new string(TruncateImpl(currentIdentifier, buffer, maxNameLength, suffix, uniquifier++));
        }

        return finalIdentifier;
    }

    /// <summary>
    ///     Creates a unique identifier by appending a number to the given string.
    /// </summary>
    /// <param name="currentIdentifier">The base identifier.</param>
    /// <param name="otherIdentifiers">A dictionary where the identifier will be used as part of the key.</param>
    /// <param name="maxLength">The maximum length of the identifier.</param>
    /// <param name="suffix">An optional suffix to add after the uniquifier.</param>
    /// <param name="uniquifier">An optional starting number for the uniquifier.</param>
    /// <returns>A unique identifier.</returns>
    public static string Uniquify(
        string currentIdentifier,
        HashSet<string> otherIdentifiers,
        int maxLength,
        string? suffix = null,
        int uniquifier = 1)
    {
        // this is the maximum buffer size we could possibly need
        var bufferSize = Math.Min(currentIdentifier.Length + (suffix?.Length ?? 0) + MaxIntLength, maxLength);
        var buffer = bufferSize <= StackAllocationLimit
            ? stackalloc char[bufferSize]
            : new char[bufferSize];

        var lookup = otherIdentifiers.GetAlternateLookup<ReadOnlySpan<char>>();
        ValidateTruncate(maxLength, suffix, uniquifier: null, out var maxNameLength, out _);
        var finalIdentifier = TruncateImpl(currentIdentifier, buffer, maxNameLength, suffix, uniquifier: null);
        while (lookup.Contains(finalIdentifier))
        {
            ValidateTruncate(maxLength, suffix, uniquifier, out maxNameLength, out _);
            finalIdentifier = TruncateImpl(currentIdentifier, buffer, maxNameLength, suffix, uniquifier++);
        }

        return new string(finalIdentifier);
    }

    /// <summary>
    ///     Ensures the given identifier is shorter than the given length by removing the extra characters from the end.
    /// </summary>
    /// <param name="identifier">The identifier to shorten.</param>
    /// <param name="maxLength">The maximum length of the identifier.</param>
    /// <param name="suffix">An optional suffix to add after the uniquifier.</param>
    /// <param name="uniquifier">An optional number that will be appended to the identifier.</param>
    /// <returns>The shortened identifier.</returns>
    public static string Truncate(string identifier, int maxLength, string? suffix = null, int? uniquifier = null)
    {
        ValidateTruncate(maxLength, suffix, uniquifier, out var maxNameLength, out var uniquifierLength);

        var bufferSize = Math.Min(identifier.Length, maxNameLength) + uniquifierLength;
        var buffer = bufferSize <= StackAllocationLimit
            ? stackalloc char[bufferSize]
            : new char[bufferSize];

        return new string(TruncateImpl(identifier, buffer, maxNameLength, suffix, uniquifier));
    }

    private static void ValidateTruncate(int maxLength, string? suffix, int? uniquifier, out int maxNameLength, out int uniquifierLength)
    {
        uniquifierLength = (suffix?.Length ?? 0) + GetLength(uniquifier);
        maxNameLength = maxLength - uniquifierLength;
        if (maxNameLength <= 0)
        {
            throw new ArgumentException("Invalid maximum length for truncation.", nameof(maxLength));
        }
    }

    private static ReadOnlySpan<char> TruncateImpl(ReadOnlySpan<char> identifier, Span<char> buffer, int maxNameLength, string? suffix, int? uniquifier)
    {
        int position;

        if (identifier.Length <= maxNameLength)
        {
            identifier.CopyTo(buffer);
            position = identifier.Length;
        }
        else
        {
            identifier[..(maxNameLength - 1)].CopyTo(buffer);
            buffer[maxNameLength - 1] = '~';
            position = maxNameLength;
        }

        if (uniquifier is not null)
        {
            var result = uniquifier.Value.TryFormat(buffer[position..], out var written);
            Check.DebugAssert(result, "Formatting uniquifier failed.");
            position += written;
        }

        if (suffix is not null)
        {
            suffix.CopyTo(buffer[position..]);
            position += suffix.Length;
        }

        return buffer[..position];
    }

    private static int GetLength(int? number)
        => number switch
        {
            null => 0,
            < 10 => 1,
            < 100 => 2,
            < 1_000 => 3,
            < 10_000 => 4,
            < 100_000 => 5,
            < 1_000_000 => 6,
            < 10_000_000 => 7,
            < 100_000_000 => 8,
            < 1_000_000_000 => 9,
            _ => MaxIntLength,
        };
}
