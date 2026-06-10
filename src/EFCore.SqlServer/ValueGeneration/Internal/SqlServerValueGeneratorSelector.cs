// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.SqlServer.ValueGeneration.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerValueGeneratorSelector : RelationalValueGeneratorSelector
{
    private readonly ISqlServerSequenceValueGeneratorFactory _sequenceFactory;
    private readonly ISqlServerConnection _connection;
    private readonly IRawSqlCommandBuilder _rawSqlCommandBuilder;
    private readonly IRelationalCommandDiagnosticsLogger _commandLogger;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerValueGeneratorSelector(
        ValueGeneratorSelectorDependencies dependencies,
        ISqlServerSequenceValueGeneratorFactory sequenceFactory,
        ISqlServerConnection connection,
        IRawSqlCommandBuilder rawSqlCommandBuilder,
        IRelationalCommandDiagnosticsLogger commandLogger)
        : base(dependencies)
    {
        _sequenceFactory = sequenceFactory;
        _connection = connection;
        _rawSqlCommandBuilder = rawSqlCommandBuilder;
        _commandLogger = commandLogger;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public new virtual ISqlServerValueGeneratorCache Cache
        => (ISqlServerValueGeneratorCache)base.Cache;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [Obsolete("Use TrySelect and throw if needed when the generator is not found.")]
    public override ValueGenerator? Select(IProperty property, ITypeBase typeBase)
    {
        if (TrySelect(property, typeBase, out var valueGenerator))
        {
            return valueGenerator;
        }

        throw new NotSupportedException(
            CoreStrings.NoValueGenerator(property.Name, property.DeclaringType.DisplayName(), property.ClrType.ShortDisplayName()));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override bool TrySelect(IProperty property, ITypeBase typeBase, out ValueGenerator? valueGenerator)
    {
        if (property.GetValueGeneratorFactory() != null
            || property.GetValueGenerationStrategy() != SqlServerValueGenerationStrategy.SequenceHiLo)
        {
            return base.TrySelect(property, typeBase, out valueGenerator);
        }

        var propertyType = property.ClrType.UnwrapNullableType().UnwrapEnumType();

        valueGenerator = _sequenceFactory.TryCreate(
            property,
            propertyType,
            Cache.GetOrAddSequenceState(property, _connection),
            _connection,
            _rawSqlCommandBuilder,
            _commandLogger);

        if (valueGenerator != null)
        {
            return true;
        }

        var converter = property.GetTypeMapping().Converter;
        if (converter != null
            && converter.ProviderClrType != propertyType)
        {
            valueGenerator = _sequenceFactory.TryCreate(
                property,
                converter.ProviderClrType,
                Cache.GetOrAddSequenceState(property, _connection),
                _connection,
                _rawSqlCommandBuilder,
                _commandLogger);

            if (valueGenerator != null)
            {
                valueGenerator = valueGenerator.WithConverter(converter);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ValueGenerator? FindForType(IProperty property, ITypeBase typeBase, Type clrType)
        => property.ClrType.UnwrapNullableType() == typeof(Guid)
            ? property.ValueGenerated == ValueGenerated.Never || property.GetDefaultValueSql() != null
                ? new TemporaryGuidValueGenerator()
                : new SequentialGuidValueGenerator()
            : base.FindForType(property, typeBase, clrType);
}
