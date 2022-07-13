// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Infrastructure;

/// <summary>
///     A key that uniquely identifies the model for a given context. This is used to store and lookup
///     a cached model for a given context. This default implementation uses the context type as they key, thus
///     assuming that all contexts of a given type have the same model.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-model-caching">EF Core model caching</see> for more information and examples.
/// </remarks>
public class ModelCacheKey
{
    private readonly Type _dbContextType;
    private readonly bool _designTime;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ModelCacheKey" /> class.
    /// </summary>
    /// <param name="context">
    ///     The context instance that this key is for.
    /// </param>
    public ModelCacheKey(DbContext context)
    {
        _dbContextType = context.GetType();
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ModelCacheKey" /> class.
    /// </summary>
    /// <param name="context">
    ///     The context instance that this key is for.
    /// </param>
    /// <param name="designTime">Whether the model should contain design-time configuration.</param>
    public ModelCacheKey(DbContext context, bool designTime)
    {
        _dbContextType = context.GetType();
        _designTime = designTime;
    }

    /// <summary>
    ///     Determines if this key is equivalent to a given key (i.e. if they are for the same context type).
    /// </summary>
    /// <param name="other">
    ///     The key to compare this key to.
    /// </param>
    /// <returns>
    ///     <see langword="true" /> if the key is for the same context type, otherwise <see langword="false" />.
    /// </returns>
    protected virtual bool Equals(ModelCacheKey other)
        => _dbContextType == other._dbContextType
            && _designTime == other._designTime;

    /// <summary>
    ///     Determines if this key is equivalent to a given object (i.e. if they are keys for the same context type).
    /// </summary>
    /// <param name="obj">
    ///     The object to compare this key to.
    /// </param>
    /// <returns>
    ///     <see langword="true" /> if the object is a <see cref="ModelCacheKey" /> and is for the same context type, otherwise
    ///     <see langword="false" />.
    /// </returns>
    public override bool Equals(object? obj)
        => (obj is ModelCacheKey otherAsKey) && Equals(otherAsKey);

    /// <summary>
    ///     Gets the hash code for the key.
    /// </summary>
    /// <returns>
    ///     The hash code for the key.
    /// </returns>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(_dbContextType);
        hash.Add(_designTime);
        return hash.ToHashCode();
    }
}
