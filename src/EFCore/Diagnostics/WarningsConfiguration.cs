// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Immutable;

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     Represents configuration for which warnings should be thrown, logged, or ignored.
///     by database providers or extensions. These options are set using <see cref="WarningsConfigurationBuilder" />.
/// </summary>
/// <remarks>
///     <para>
///         Instances of this class are designed to be immutable. To change an option, call one of the 'With...'
///         methods to obtain a new instance with the option changed.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-warning-configuration">Configuration for specific messages</see> for more information and
///         examples.
///     </para>
/// </remarks>
public class WarningsConfiguration
{
    private ImmutableSortedDictionary<int, (WarningBehavior? Behavior, LogLevel? Level)> _explicitBehaviors
        = ImmutableSortedDictionary<int, (WarningBehavior? Behavior, LogLevel? Level)>.Empty;

    private WarningBehavior _defaultBehavior = WarningBehavior.Log;

    private int? _serviceProviderHash;

    /// <summary>
    ///     Creates a new, empty configuration, with all options set to their defaults.
    /// </summary>
    public WarningsConfiguration()
    {
    }

    /// <summary>
    ///     Called by a derived class constructor when implementing the <see cref="Clone" /> method.
    /// </summary>
    /// <param name="copyFrom">The instance that is being cloned.</param>
    protected WarningsConfiguration(WarningsConfiguration copyFrom)
    {
        _defaultBehavior = copyFrom._defaultBehavior;
        _explicitBehaviors = copyFrom._explicitBehaviors;
    }

    /// <summary>
    ///     Override this method in a derived class to ensure that any clone created is also of that class.
    /// </summary>
    /// <returns>A clone of this instance, which can be modified before being returned as immutable.</returns>
    protected virtual WarningsConfiguration Clone()
        => new(this);

    /// <summary>
    ///     The option set from the <see cref="DefaultBehavior" /> method.
    /// </summary>
    public virtual WarningBehavior DefaultBehavior
        => _defaultBehavior;

    /// <summary>
    ///     Creates a new instance with all options the same as for this instance, but with the given option changed.
    ///     It is unusual to call this method directly. Instead use <see cref="WarningsConfigurationBuilder" />.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-warning-configuration">Configuration for specific messages</see> for more information and
    ///     examples.
    /// </remarks>
    /// <param name="warningBehavior">The option to change.</param>
    /// <returns>A new instance with the option changed.</returns>
    public virtual WarningsConfiguration WithDefaultBehavior(WarningBehavior warningBehavior)
    {
        var clone = Clone();

        clone._defaultBehavior = warningBehavior;

        return clone;
    }

    /// <summary>
    ///     Creates a new instance with the given explicit <see cref="WarningBehavior" /> set for
    ///     all given event IDs.
    ///     It is unusual to call this method directly. Instead use <see cref="WarningsConfigurationBuilder" />.
    /// </summary>
    /// <param name="eventIds">The event IDs for which the behavior should be set.</param>
    /// <param name="warningBehavior">The behavior to set.</param>
    /// <returns>A new instance with the behaviors set.</returns>
    public virtual WarningsConfiguration WithExplicit(
        IEnumerable<EventId> eventIds,
        WarningBehavior warningBehavior)
    {
        var clone = Clone();

        var builder = ImmutableSortedDictionary.CreateBuilder<int, (WarningBehavior? Behavior, LogLevel? Level)>();
        builder.AddRange(clone._explicitBehaviors);
        foreach (var eventId in eventIds)
        {
            if (_explicitBehaviors.TryGetValue(eventId.Id, out var pair))
            {
                pair = (warningBehavior, pair.Level);
            }
            else
            {
                pair = (warningBehavior, null);
            }

            builder[eventId.Id] = pair;
        }

        clone._explicitBehaviors = builder.ToImmutable();

        return clone;
    }

    /// <summary>
    ///     Creates a new instance with the given log level set for all given event IDs.
    ///     It is unusual to call this method directly. Instead use <see cref="WarningsConfigurationBuilder" />.
    /// </summary>
    /// <param name="eventsAndLevels">The event IDs and corresponding log levels to set.</param>
    /// <returns>A new instance with the behaviors set.</returns>
    public virtual WarningsConfiguration WithExplicit(
        IEnumerable<(EventId Id, LogLevel Level)> eventsAndLevels)
    {
        var clone = Clone();

        var builder = ImmutableSortedDictionary.CreateBuilder<int, (WarningBehavior? Behavior, LogLevel? Level)>();
        builder.AddRange(clone._explicitBehaviors);

        foreach (var (id, level) in eventsAndLevels)
        {
            builder[id.Id] = (WarningBehavior.Log, level);
        }

        clone._explicitBehaviors = builder.ToImmutable();

        return clone;
    }

    /// <summary>
    ///     Gets the <see cref="WarningBehavior" /> set for the given event ID, or <see langword="null" />
    ///     if no explicit behavior has been set.
    /// </summary>
    public virtual WarningBehavior? GetBehavior(EventId eventId)
        => _explicitBehaviors.TryGetValue(eventId.Id, out var warningBehavior)
            ? warningBehavior.Behavior
            : null;

    /// <summary>
    ///     Gets the <see cref="LogLevel" /> set for the given event ID, or <see langword="null" />
    ///     if no explicit behavior has been set.
    /// </summary>
    /// <returns>The <see cref="LogLevel" /> set for the given event ID.</returns>
    public virtual LogLevel? GetLevel(EventId eventId)
        => _explicitBehaviors.TryGetValue(eventId.Id, out var warningBehavior)
            ? warningBehavior.Level
            : null;

    /// <summary>
    ///     Creates a new instance with the given explicit <see cref="WarningBehavior" /> set for
    ///     the given event ID, but only if no explicit behavior has already been set.
    ///     It is unusual to call this method directly. Instead use <see cref="WarningsConfigurationBuilder" />.
    /// </summary>
    /// <param name="eventId">The event ID for which the behavior should be set.</param>
    /// <param name="warningBehavior">The behavior to set.</param>
    /// <returns>A new instance with the behavior set, or this instance if a behavior was already set.</returns>
    public virtual WarningsConfiguration TryWithExplicit(EventId eventId, WarningBehavior warningBehavior)
        => _explicitBehaviors.ContainsKey(eventId.Id)
            ? this
            : WithExplicit(new[] { eventId }, warningBehavior);

    /// <summary>
    ///     Returns a value indicating whether all of the options used in <see cref="GetServiceProviderHashCode" />
    ///     are the same as in the given extension.
    /// </summary>
    /// <param name="other">The other configuration object.</param>
    /// <returns>A value indicating whether all of the options that require a new service provider are the same.</returns>
    public virtual bool ShouldUseSameServiceProvider(WarningsConfiguration other)
        => _defaultBehavior == other._defaultBehavior
            && _explicitBehaviors.Count == other._explicitBehaviors.Count
            && _explicitBehaviors.SequenceEqual(other._explicitBehaviors);

    /// <summary>
    ///     Returns a hash code created from any options that would cause a new <see cref="IServiceProvider" />
    ///     to be needed.
    /// </summary>
    /// <returns>A hash over options that require a new service provider when changed.</returns>
    public virtual int GetServiceProviderHashCode()
    {
        if (_serviceProviderHash == null)
        {
            var hashCode = new HashCode();
            hashCode.Add(_defaultBehavior);

            foreach (var (key, value) in _explicitBehaviors)
            {
                hashCode.Add(key);
                hashCode.Add(value);
            }

            _serviceProviderHash = hashCode.ToHashCode();
        }

        return _serviceProviderHash.Value;
    }
}
