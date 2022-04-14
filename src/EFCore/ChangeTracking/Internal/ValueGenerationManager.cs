// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class ValueGenerationManager : IValueGenerationManager
{
    private readonly IValueGeneratorSelector _valueGeneratorSelector;
    private readonly IKeyPropagator _keyPropagator;
    private readonly IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> _logger;
    private readonly ILoggingOptions _loggingOptions;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public ValueGenerationManager(
        IValueGeneratorSelector valueGeneratorSelector,
        IKeyPropagator keyPropagator,
        IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> logger,
        ILoggingOptions loggingOptions)
    {
        _valueGeneratorSelector = valueGeneratorSelector;
        _keyPropagator = keyPropagator;
        _logger = logger;
        _loggingOptions = loggingOptions;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalEntityEntry? Propagate(InternalEntityEntry entry)
    {
        InternalEntityEntry? chosenPrincipal = null;
        foreach (var property in entry.EntityType.GetForeignKeyProperties())
        {
            if (!entry.HasDefaultValue(property))
            {
                continue;
            }

            var principalEntry = _keyPropagator.PropagateValue(entry, property);
            chosenPrincipal ??= principalEntry;
        }

        return chosenPrincipal;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void Generate(InternalEntityEntry entry, bool includePrimaryKey = true)
    {
        var entityEntry = new EntityEntry(entry);

        foreach (var property in entry.EntityType.GetValueGeneratingProperties())
        {
            if (!entry.HasDefaultValue(property)
                || (!includePrimaryKey
                    && property.IsPrimaryKey()))
            {
                continue;
            }

            var valueGenerator = GetValueGenerator(property);

            var generatedValue = valueGenerator.Next(entityEntry);
            var temporary = valueGenerator.GeneratesTemporaryValues;

            Log(entry, property, generatedValue, temporary);

            SetGeneratedValue(entry, property, generatedValue, temporary);

            MarkKeyUnknown(entry, includePrimaryKey, property, valueGenerator);
        }
    }

    private void Log(InternalEntityEntry entry, IProperty property, object? generatedValue, bool temporary)
    {
        if (_loggingOptions.IsSensitiveDataLoggingEnabled)
        {
            _logger.ValueGeneratedSensitive(entry, property, generatedValue, temporary);
        }
        else
        {
            _logger.ValueGenerated(entry, property, generatedValue, temporary);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual async Task GenerateAsync(
        InternalEntityEntry entry,
        bool includePrimaryKey = true,
        CancellationToken cancellationToken = default)
    {
        var entityEntry = new EntityEntry(entry);

        foreach (var property in entry.EntityType.GetValueGeneratingProperties())
        {
            if (!entry.HasDefaultValue(property)
                || (!includePrimaryKey
                    && property.IsPrimaryKey()))
            {
                continue;
            }

            var valueGenerator = GetValueGenerator(property);
            var generatedValue = await valueGenerator.NextAsync(entityEntry, cancellationToken)
                .ConfigureAwait(false);
            var temporary = valueGenerator.GeneratesTemporaryValues;

            Log(entry, property, generatedValue, temporary);

            SetGeneratedValue(
                entry,
                property,
                generatedValue,
                temporary);

            MarkKeyUnknown(entry, includePrimaryKey, property, valueGenerator);
        }
    }

    private ValueGenerator GetValueGenerator(IProperty property)
        => _valueGeneratorSelector.Select(property, property.DeclaringEntityType);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool MayGetTemporaryValue(IProperty property, IEntityType entityType)
        => property.RequiresValueGenerator()
            && _valueGeneratorSelector.Select(property, entityType).GeneratesTemporaryValues;

    private static void SetGeneratedValue(InternalEntityEntry entry, IProperty property, object? generatedValue, bool isTemporary)
    {
        if (generatedValue != null)
        {
            if (isTemporary)
            {
                entry.SetTemporaryValue(property, generatedValue);
            }
            else
            {
                entry[property] = generatedValue;
            }
        }
    }

    private static void MarkKeyUnknown(
        InternalEntityEntry entry,
        bool includePrimaryKey,
        IProperty property,
        ValueGenerator valueGenerator)
    {
        if (includePrimaryKey
            && property.IsKey()
            && property.IsShadowProperty()
            && !property.IsForeignKey()
            && !valueGenerator.GeneratesStableValues)
        {
            entry.MarkUnknown(property);
        }
    }
}
