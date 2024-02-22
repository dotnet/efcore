// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a database sequence in the model.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-sequences">Database sequences</see> for more information and examples.
/// </remarks>
public class RuntimeSequence : AnnotatableBase, ISequence
{
    private readonly string? _schema;
    private readonly Type _type;
    private readonly long _startValue;
    private readonly int _incrementBy;
    private readonly long? _minValue;
    private readonly long? _maxValue;
    private readonly bool _isCyclic;
    private readonly int? _cacheSize;
    private readonly bool _isCached;
    private readonly bool _modelSchemaIsNull;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RuntimeSequence" /> class.
    /// </summary>
    /// <param name="name">The sequence name.</param>
    /// <param name="model">The model.</param>
    /// <param name="type">The type of values generated.</param>
    /// <param name="schema">The schema.</param>
    /// <param name="startValue">The initial value.</param>
    /// <param name="incrementBy">The value increment.</param>
    /// <param name="cyclic">Whether the sequence is cyclic.</param>
    /// <param name="minValue">The minimum value.</param>
    /// <param name="maxValue">The maximum value.</param>
    /// <param name="cacheSize">The cache size.</param>
    /// <param name="cached">Whether the sequence use preallocated values.</param>
    /// <param name="modelSchemaIsNull">A value indicating whether <see cref="ModelSchema" /> is null.</param>
    public RuntimeSequence(
        string name,
        RuntimeModel model,
        Type type,
        string? schema = null,
        long startValue = Sequence.DefaultStartValue,
        int incrementBy = Sequence.DefaultIncrementBy,
        bool cyclic = false,
        long? minValue = null,
        long? maxValue = null,
        bool cached = true,
        int? cacheSize = null,
        bool modelSchemaIsNull = false)
    {
        Model = model;
        Name = name;
        _schema = schema;
        _type = type;
        _startValue = startValue;
        _incrementBy = incrementBy;
        _isCyclic = cyclic;
        _minValue = minValue;
        _maxValue = maxValue;
        _isCached = cached;
        _cacheSize = cacheSize;
        _modelSchemaIsNull = modelSchemaIsNull;
    }

    /// <summary>
    ///     Gets the model in which this sequence is defined.
    /// </summary>
    public virtual RuntimeModel Model { get; }

    /// <summary>
    ///     Gets the name of the sequence in the database.
    /// </summary>
    public virtual string Name { get; }

    /// <summary>
    ///     Gets the metadata schema of the sequence.
    /// </summary>
    public virtual string? ModelSchema
        => _modelSchemaIsNull ? null : _schema;

    /// <summary>
    ///     Gets the database schema that contains the sequence.
    /// </summary>
    public virtual string? Schema
        => _schema;

    /// <summary>
    ///     Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString()
        => ((ISequence)this).ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual DebugView DebugView
        => new(
            () => ((ISequence)this).ToDebugString(),
            () => ((ISequence)this).ToDebugString(MetadataDebugStringOptions.LongDefault));

    /// <inheritdoc />
    IReadOnlyModel IReadOnlySequence.Model
    {
        [DebuggerStepThrough]
        get => Model;
    }

    /// <inheritdoc />
    IModel ISequence.Model
    {
        [DebuggerStepThrough]
        get => Model;
    }

    /// <inheritdoc />
    long IReadOnlySequence.StartValue
    {
        [DebuggerStepThrough]
        get => _startValue;
    }

    /// <inheritdoc />
    int IReadOnlySequence.IncrementBy
    {
        [DebuggerStepThrough]
        get => _incrementBy;
    }

    /// <inheritdoc />
    long? IReadOnlySequence.MinValue
    {
        [DebuggerStepThrough]
        get => _minValue;
    }

    /// <inheritdoc />
    long? IReadOnlySequence.MaxValue
    {
        [DebuggerStepThrough]
        get => _maxValue;
    }

    /// <inheritdoc />
    Type IReadOnlySequence.Type
    {
        [DebuggerStepThrough]
        get => _type;
    }

    /// <inheritdoc />
    bool IReadOnlySequence.IsCyclic
    {
        [DebuggerStepThrough]
        get => _isCyclic;
    }

    /// <inheritdoc />
    bool IReadOnlySequence.IsCached
    {
        [DebuggerStepThrough]
        get => _isCached;
    }

    /// <inheritdoc />
    int? IReadOnlySequence.CacheSize
    {
        [DebuggerStepThrough]
        get => _cacheSize;
    }
}
