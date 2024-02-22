// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
using System.Text;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class Sequence : ConventionAnnotatable, IMutableSequence, IConventionSequence, ISequence
{
    private readonly string? _schema;
    private long? _startValue;
    private int? _incrementBy;
    private long? _minValue;
    private long? _maxValue;
    private Type? _type;
    private bool? _isCyclic;
    private bool? _isCached;
    private int? _cacheSize;
    private InternalSequenceBuilder? _builder;

    private ConfigurationSource _configurationSource;
    private ConfigurationSource? _startValueConfigurationSource;
    private ConfigurationSource? _incrementByConfigurationSource;
    private ConfigurationSource? _minValueConfigurationSource;
    private ConfigurationSource? _maxValueConfigurationSource;
    private ConfigurationSource? _typeConfigurationSource;
    private ConfigurationSource? _isCyclicConfigurationSource;
    private ConfigurationSource? _isCachedConfigurationSource;
    private ConfigurationSource? _cacheSizeConfigurationSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly Type DefaultClrType = typeof(long);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public const int DefaultIncrementBy = 1;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public const int DefaultStartValue = 1;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly long? DefaultMaxValue = default;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly long? DefaultMinValue = default;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly bool DefaultIsCyclic = default;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly bool DefaultIsCached = true;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly int? DefaultCacheSize = default;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public Sequence(
        string name,
        string? schema,
        IReadOnlyModel model,
        ConfigurationSource configurationSource)
    {
        Model = model;
        Name = name;
        _schema = schema;
        _configurationSource = configurationSource;
        _builder = new InternalSequenceBuilder(this, ((IConventionModel)model).Builder);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [Obsolete("Use the other constructor")] // DO NOT REMOVE
    // Used in snapshot model processor. See issue#18557
    public Sequence(IReadOnlyModel model, string annotationName)
    {
        Model = model;
        _configurationSource = ConfigurationSource.Explicit;

        var data = SequenceData.Deserialize((string)model[annotationName]!);
        Name = data.Name;
        _schema = data.Schema;
        _startValue = data.StartValue;
        _incrementBy = data.IncrementBy;
        _minValue = data.MinValue;
        _maxValue = data.MaxValue;
        _type = data.ClrType;
        _isCyclic = data.IsCyclic;
        _isCached = data.IsCached;
        _cacheSize = data.CacheSize;
        _builder = new InternalSequenceBuilder(this, ((IConventionModel)model).Builder);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IEnumerable<ISequence> GetSequences(IReadOnlyModel model)
        => ((Dictionary<(string, string?), ISequence>?)model[RelationalAnnotationNames.Sequences])
            ?.OrderBy(t => t.Key).Select(t => t.Value)
            ?? Enumerable.Empty<ISequence>();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static ISequence? FindSequence(IReadOnlyModel model, string name, string? schema)
    {
        var sequences = (Dictionary<(string, string?), ISequence>?)model[RelationalAnnotationNames.Sequences];
        if (sequences == null
            || !sequences.TryGetValue((name, schema), out var sequence))
        {
            return null;
        }

        return sequence;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static Sequence AddSequence(
        IMutableModel model,
        string name,
        string? schema,
        ConfigurationSource configurationSource)
    {
        var sequence = new Sequence(name, schema, model, configurationSource);
        var sequences = (Dictionary<(string, string?), ISequence>?)model[RelationalAnnotationNames.Sequences];
        if (sequences == null)
        {
            sequences = new Dictionary<(string, string?), ISequence>();
            model[RelationalAnnotationNames.Sequences] = sequences;
        }

        sequences.Add((name, schema), sequence);
        return sequence;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static Sequence? SetName(
        IMutableModel model,
        Sequence sequence,
        string name)
    {
        sequence.EnsureMutable();

        var sequences = (Dictionary<(string, string?), ISequence>?)model[RelationalAnnotationNames.Sequences];
        var tuple = (sequence.Name, sequence.ModelSchema);
        if (sequences == null
            || !sequences.ContainsKey(tuple))
        {
            return null;
        }

        sequences.Remove(tuple);

        sequence.Name = name;

        sequences.Add((name, sequence.ModelSchema), sequence);

        return sequence;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static Sequence? RemoveSequence(IMutableModel model, string name, string? schema)
    {
        var sequences = (Dictionary<(string, string?), ISequence>?)model[RelationalAnnotationNames.Sequences];
        if (sequences == null
            || !sequences.TryGetValue((name, schema), out var sequence))
        {
            return null;
        }

        var mutableSequence = (Sequence)sequence;
        sequences.Remove((name, schema));
        mutableSequence.SetRemovedFromModel();

        return mutableSequence;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalSequenceBuilder Builder
    {
        [DebuggerStepThrough]
        get => _builder ?? throw new InvalidOperationException(CoreStrings.ObjectRemovedFromModel(Name));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool IsInModel
        => _builder is not null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void SetRemovedFromModel()
        => _builder = null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IReadOnlyModel Model { get; }

    /// <summary>
    ///     Indicates whether the sequence is read-only.
    /// </summary>
    public override bool IsReadOnly
        => ((Annotatable)Model).IsReadOnly;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string Name { get; set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string? ModelSchema
        => _schema;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string? Schema
        => _schema ?? Model.GetDefaultSchema();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConfigurationSource GetConfigurationSource()
        => _configurationSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void UpdateConfigurationSource(ConfigurationSource configurationSource)
        => _configurationSource = _configurationSource.Max(configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual long StartValue
    {
        get => _startValue ?? DefaultStartValue;
        set => SetStartValue(value, ConfigurationSource.Explicit);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual long? SetStartValue(long? startValue, ConfigurationSource configurationSource)
    {
        EnsureMutable();

        _startValue = startValue;

        _startValueConfigurationSource = startValue == null
            ? null
            : configurationSource.Max(_startValueConfigurationSource);

        return startValue;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConfigurationSource? GetStartValueConfigurationSource()
        => _startValueConfigurationSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual int IncrementBy
    {
        get => _incrementBy ?? DefaultIncrementBy;
        set => SetIncrementBy(value, ConfigurationSource.Explicit);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual int? SetIncrementBy(int? incrementBy, ConfigurationSource configurationSource)
    {
        EnsureMutable();

        _incrementBy = incrementBy;

        _incrementByConfigurationSource = incrementBy == null
            ? null
            : configurationSource.Max(_incrementByConfigurationSource);

        return incrementBy;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConfigurationSource? GetIncrementByConfigurationSource()
        => _incrementByConfigurationSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual long? MinValue
    {
        get => _minValue ?? DefaultMinValue;
        set => SetMinValue(value, ConfigurationSource.Explicit);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual long? SetMinValue(long? minValue, ConfigurationSource configurationSource)
    {
        EnsureMutable();

        _minValue = minValue;

        _minValueConfigurationSource = minValue == null
            ? null
            : configurationSource.Max(_minValueConfigurationSource);

        return minValue;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConfigurationSource? GetMinValueConfigurationSource()
        => _minValueConfigurationSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual long? MaxValue
    {
        get => _maxValue;
        set => SetMaxValue(value, ConfigurationSource.Explicit);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual long? SetMaxValue(long? maxValue, ConfigurationSource configurationSource)
    {
        EnsureMutable();

        _maxValue = maxValue;

        _maxValueConfigurationSource = maxValue == null
            ? null
            : configurationSource.Max(_maxValueConfigurationSource);

        return maxValue;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConfigurationSource? GetMaxValueConfigurationSource()
        => _maxValueConfigurationSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IReadOnlyCollection<Type> SupportedTypes { get; }
        = new[] { typeof(byte), typeof(long), typeof(int), typeof(short), typeof(decimal) };

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Type Type
    {
        get => _type ?? DefaultClrType;
        set => SetType(value, ConfigurationSource.Explicit);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Type? SetType(Type? type, ConfigurationSource configurationSource)
    {
        EnsureMutable();

        if (type != null
            && !SupportedTypes.Contains(type))
        {
            throw new ArgumentException(RelationalStrings.BadSequenceType);
        }

        _type = type;

        _typeConfigurationSource = type == null
            ? null
            : configurationSource.Max(_typeConfigurationSource);

        return type;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConfigurationSource? GetTypeConfigurationSource()
        => _typeConfigurationSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool IsCyclic
    {
        get => _isCyclic ?? DefaultIsCyclic;
        set => SetIsCyclic(value, ConfigurationSource.Explicit);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool? SetIsCyclic(bool? cyclic, ConfigurationSource configurationSource)
    {
        EnsureMutable();

        _isCyclic = cyclic;

        _isCyclicConfigurationSource = cyclic == null
            ? null
            : configurationSource.Max(_isCyclicConfigurationSource);

        return cyclic;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConfigurationSource? GetIsCyclicConfigurationSource()
        => _isCyclicConfigurationSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool IsCached
    {
        get => _isCached ?? DefaultIsCached;
        set => SetIsCached(value, ConfigurationSource.Explicit);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool? SetIsCached(bool? cached, ConfigurationSource configurationSource)
    {
        EnsureMutable();

        _isCached = cached;

        _isCachedConfigurationSource = cached == null
            ? null
            : configurationSource.Max(_isCachedConfigurationSource);

        return cached;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConfigurationSource? GetIsCachedConfigurationSource()
        => _isCachedConfigurationSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual int? CacheSize
    {
        get => _cacheSize;
        set => SetCacheSize(value, ConfigurationSource.Explicit);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual int? SetCacheSize(int? cacheSize, ConfigurationSource configurationSource)
    {
        EnsureMutable();

        _cacheSize = cacheSize;

        _cacheSizeConfigurationSource = cacheSize == null
            ? null
            : configurationSource.Max(_cacheSizeConfigurationSource);

        return cacheSize;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConfigurationSource? GetCacheSizeConfigurationSource()
        => _cacheSizeConfigurationSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override string ToString()
        => ((ISequence)this).ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionSequenceBuilder IConventionSequence.Builder
    {
        [DebuggerStepThrough]
        get => Builder;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IMutableModel IMutableSequence.Model
    {
        [DebuggerStepThrough]
        get => (IMutableModel)Model;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionModel IConventionSequence.Model
    {
        [DebuggerStepThrough]
        get => (IConventionModel)Model;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IModel ISequence.Model
    {
        [DebuggerStepThrough]
        get => (IModel)Model;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    long? IConventionSequence.SetStartValue(long? startValue, bool fromDataAnnotation)
        => SetStartValue(startValue, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    int? IConventionSequence.SetIncrementBy(int? incrementBy, bool fromDataAnnotation)
        => SetIncrementBy(incrementBy, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    long? IConventionSequence.SetMinValue(long? minValue, bool fromDataAnnotation)
        => SetMinValue(minValue, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    long? IConventionSequence.SetMaxValue(long? maxValue, bool fromDataAnnotation)
        => SetMaxValue(maxValue, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    Type? IConventionSequence.SetType(Type? type, bool fromDataAnnotation)
        => SetType(type, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    bool? IConventionSequence.SetIsCyclic(bool? cyclic, bool fromDataAnnotation)
        => SetIsCyclic(cyclic, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    int? IConventionSequence.SetCacheSize(int? cacheSize, bool fromDataAnnotation)
        => SetCacheSize(cacheSize, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    bool? IConventionSequence.SetIsCached(bool? cached, bool fromDataAnnotation)
        => SetIsCached(cached, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    [Obsolete("Don't use this in any new code")] // DO NOT REMOVE
    // Used in model snapshot processor code path. See issue#18557
    private sealed class SequenceData
    {
        public string Name { get; set; } = default!;

        public string? Schema { get; set; }

        public long StartValue { get; set; }

        public int IncrementBy { get; set; }

        public long? MinValue { get; set; }

        public long? MaxValue { get; set; }

        public Type ClrType { get; set; } = default!;

        public bool IsCyclic { get; set; }

        public bool IsCached { get; set; }

        public int? CacheSize { get; set; }

        public static SequenceData Deserialize(string value)
        {
            try
            {
                var data = new SequenceData();

                // ReSharper disable PossibleInvalidOperationException
                var position = 0;
                data.Name = ExtractValue(value, ref position)!;
                data.Schema = ExtractValue(value, ref position);
                data.StartValue = (long)AsLong(ExtractValue(value, ref position)!)!;
                data.IncrementBy = (int)AsLong(ExtractValue(value, ref position)!)!;
                data.MinValue = AsLong(ExtractValue(value, ref position));
                data.MaxValue = AsLong(ExtractValue(value, ref position));
                data.ClrType = AsType(ExtractValue(value, ref position)!);
                data.IsCyclic = AsBool(ExtractValue(value, ref position));
                data.IsCached = AsBool(ExtractValue(value, ref position));
                data.CacheSize = AsInt(ExtractValue(value, ref position));
                // ReSharper restore PossibleInvalidOperationException

                return data;
            }
            catch (Exception ex)
            {
                throw new ArgumentException(RelationalStrings.BadSequenceString, ex);
            }
        }

        private static string? ExtractValue(string value, ref int position)
        {
            position = value.IndexOf('\'', position) + 1;

            var end = value.IndexOf('\'', position);

            while (end + 1 < value.Length
                   && value[end + 1] == '\'')
            {
                end = value.IndexOf('\'', end + 2);
            }

            var extracted = value[position..end].Replace("''", "'");
            position = end + 1;

            return extracted.Length == 0 ? null : extracted;
        }

        private static long? AsLong(string? value)
            => value == null ? null : long.Parse(value, CultureInfo.InvariantCulture);

        private static int? AsInt(string? value)
            => value == null ? null : int.Parse(value, CultureInfo.InvariantCulture);

        private static Type AsType(string value)
            => value == nameof(Int64)
                ? typeof(long)
                : value == nameof(Int32)
                    ? typeof(int)
                    : value == nameof(Int16)
                        ? typeof(short)
                        : value == nameof(Decimal)
                            ? typeof(decimal)
                            : typeof(byte);

        private static bool AsBool(string? value)
            => value != null && bool.Parse(value);

        private static void EscapeAndQuote(StringBuilder builder, object? value)
        {
            builder.Append('\'');

            if (value != null)
            {
                builder.Append(value.ToString()!.Replace("'", "''"));
            }

            builder.Append('\'');
        }
    }
}
