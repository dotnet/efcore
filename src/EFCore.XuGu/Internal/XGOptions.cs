// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Data.Common;
using System.Linq;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Storage.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.Extensions.DependencyInjection;
using XuguClient;

namespace Microsoft.EntityFrameworkCore.XuGu.Internal
{
    public class XGOptions : IXGOptions
    {
        private static readonly XGSchemaNameTranslator _ignoreSchemaNameTranslator = (_, objectName) => objectName;

        public XGOptions()
        {
            ConnectionSettings = new XGConnectionSettings();
            DataSource = null;
            ServerVersion = null;

            // We explicitly use `utf8mb4` in all instances, where charset based calculations need to be done, but accessing annotations
            // isn't possible (e.g. in `XGTypeMappingSource`).
            // This is also being used as the universal fallback character set, if no character set was explicitly defined for the model,
            // which will result in similar behavior as in previous versions and ensure that databases use a decent/the recommended charset
            // by default, if none was explicitly set.
            DefaultCharSet = CharSet.Utf8Mb4;

            // NCHAR and NVARCHAR are prefdefined by MySQL.
            NationalCharSet = CharSet.Utf8Mb3;

            // Optimize space and performance for GUID columns.
            DefaultGuidCollation = "ascii_general_ci";

            ReplaceLineBreaksWithCharFunction = true;
            DefaultDataTypeMappings = new XGDefaultDataTypeMappings();

            // Throw by default if a schema is being used with any type.
            SchemaNameTranslator = null;

            // TODO: Change to `true` for EF Core 5.
            IndexOptimizedBooleanColumns = false;

            LimitKeyedOrIndexedStringColumnLength = true;
            StringComparisonTranslations = false;
            PrimitiveCollectionsSupport = false;
        }

        public virtual void Initialize(IDbContextOptions options)
        {
            var xgOptions = options.FindExtension<XGOptionsExtension>() ?? new XGOptionsExtension();
            var xgJsonOptions = (XGJsonOptionsExtension)options.Extensions.LastOrDefault(e => e is XGJsonOptionsExtension);

            ConnectionSettings = GetConnectionSettings(xgOptions, options);
            DataSource = xgOptions.DataSource;
            ServerVersion = xgOptions.ServerVersion ?? throw new InvalidOperationException($"The {nameof(ServerVersion)} has not been set.");
            NoBackslashEscapes = xgOptions.NoBackslashEscapes;
            ReplaceLineBreaksWithCharFunction = xgOptions.ReplaceLineBreaksWithCharFunction;
            DefaultDataTypeMappings = ApplyDefaultDataTypeMappings(xgOptions.DefaultDataTypeMappings, ConnectionSettings);
            SchemaNameTranslator = xgOptions.SchemaNameTranslator ?? (xgOptions.SchemaBehavior == XGSchemaBehavior.Ignore
                ? _ignoreSchemaNameTranslator
                : null);
            IndexOptimizedBooleanColumns = xgOptions.IndexOptimizedBooleanColumns;
            JsonChangeTrackingOptions = xgJsonOptions?.JsonChangeTrackingOptions ?? default;
            LimitKeyedOrIndexedStringColumnLength = xgOptions.LimitKeyedOrIndexedStringColumnLength;
            StringComparisonTranslations = xgOptions.StringComparisonTranslations;
            PrimitiveCollectionsSupport = xgOptions.PrimitiveCollectionsSupport;
        }

        public virtual void Validate(IDbContextOptions options)
        {
            var xgOptions = options.FindExtension<XGOptionsExtension>() ?? new XGOptionsExtension();
            var xgJsonOptions = (XGJsonOptionsExtension)options.Extensions.LastOrDefault(e => e is XGJsonOptionsExtension);
            var connectionSettings = GetConnectionSettings(xgOptions, options);

            //
            // CHECK: To we have to ensure that the ApplicationServiceProvider itself is not replaced, because we rely on it in our
            //        DbDataSource check, or is that not possible?
            //

            // Even though we only save a DbDataSource that has been explicitly set using the XGOptionsExtensions here in XGOptions,
            // we will later also fall back to a DbDataSource that has been added as a service to the ApplicationServiceProvider, if no
            // DbDataSource has been explicitly set here. We call that DbDataSource the "effective" DbDataSource and handle it in the same
            // way we would handle a singleton option.
            var effectiveDataSource = xgOptions.DataSource ??
                                      options.FindExtension<CoreOptionsExtension>()?.ApplicationServiceProvider?.GetService<DbDataSource>();
            if (effectiveDataSource is not null &&
                !ReferenceEquals(DataSource, xgOptions.DataSource))
            {
                throw new InvalidOperationException(
                    XGStrings.TwoDataSourcesInSameServiceProvider(nameof(DbContextOptionsBuilder.UseInternalServiceProvider)));
            }

            if (!Equals(ServerVersion, xgOptions.ServerVersion))
            {
                throw new InvalidOperationException(
                    CoreStrings.SingletonOptionChanged(
                        nameof(XGOptionsExtension.ServerVersion),
                        nameof(DbContextOptionsBuilder.UseInternalServiceProvider)));
            }

            if (!Equals(NoBackslashEscapes, xgOptions.NoBackslashEscapes))
            {
                throw new InvalidOperationException(
                    CoreStrings.SingletonOptionChanged(
                        nameof(XGDbContextOptionsBuilder.DisableBackslashEscaping),
                        nameof(DbContextOptionsBuilder.UseInternalServiceProvider)));
            }

            if (!Equals(ReplaceLineBreaksWithCharFunction, xgOptions.ReplaceLineBreaksWithCharFunction))
            {
                throw new InvalidOperationException(
                    CoreStrings.SingletonOptionChanged(
                        nameof(XGDbContextOptionsBuilder.DisableLineBreakToCharSubstition),
                        nameof(DbContextOptionsBuilder.UseInternalServiceProvider)));
            }

            if (!Equals(DefaultDataTypeMappings, ApplyDefaultDataTypeMappings(xgOptions.DefaultDataTypeMappings ?? new XGDefaultDataTypeMappings(), connectionSettings)))
            {
                throw new InvalidOperationException(
                    CoreStrings.SingletonOptionChanged(
                        nameof(XGDbContextOptionsBuilder.DefaultDataTypeMappings),
                        nameof(DbContextOptionsBuilder.UseInternalServiceProvider)));
            }

            if (!Equals(
                SchemaNameTranslator,
                xgOptions.SchemaBehavior == XGSchemaBehavior.Ignore
                    ? _ignoreSchemaNameTranslator
                    : SchemaNameTranslator))
            {
                throw new InvalidOperationException(
                    CoreStrings.SingletonOptionChanged(
                        nameof(XGDbContextOptionsBuilder.SchemaBehavior),
                        nameof(DbContextOptionsBuilder.UseInternalServiceProvider)));
            }

            if (!Equals(IndexOptimizedBooleanColumns, xgOptions.IndexOptimizedBooleanColumns))
            {
                throw new InvalidOperationException(
                    CoreStrings.SingletonOptionChanged(
                        nameof(XGDbContextOptionsBuilder.EnableIndexOptimizedBooleanColumns),
                        nameof(DbContextOptionsBuilder.UseInternalServiceProvider)));
            }

            if (!Equals(JsonChangeTrackingOptions, xgJsonOptions?.JsonChangeTrackingOptions ?? default))
            {
                throw new InvalidOperationException(
                    CoreStrings.SingletonOptionChanged(
                        nameof(XGJsonOptionsExtension.JsonChangeTrackingOptions),
                        nameof(DbContextOptionsBuilder.UseInternalServiceProvider)));
            }

            if (!Equals(LimitKeyedOrIndexedStringColumnLength, xgOptions.LimitKeyedOrIndexedStringColumnLength))
            {
                throw new InvalidOperationException(
                    CoreStrings.SingletonOptionChanged(
                        nameof(XGDbContextOptionsBuilder.LimitKeyedOrIndexedStringColumnLength),
                        nameof(DbContextOptionsBuilder.UseInternalServiceProvider)));
            }

            if (!Equals(StringComparisonTranslations, xgOptions.StringComparisonTranslations))
            {
                throw new InvalidOperationException(
                    CoreStrings.SingletonOptionChanged(
                        nameof(XGDbContextOptionsBuilder.EnableStringComparisonTranslations),
                        nameof(DbContextOptionsBuilder.UseInternalServiceProvider)));
            }

            if (!Equals(PrimitiveCollectionsSupport, xgOptions.PrimitiveCollectionsSupport))
            {
                throw new InvalidOperationException(
                    CoreStrings.SingletonOptionChanged(
                        nameof(XGDbContextOptionsBuilder.EnablePrimitiveCollectionsSupport),
                        nameof(DbContextOptionsBuilder.UseInternalServiceProvider)));
            }
        }

        protected virtual XGDefaultDataTypeMappings ApplyDefaultDataTypeMappings(XGDefaultDataTypeMappings defaultDataTypeMappings, XGConnectionSettings connectionSettings)
        {
            defaultDataTypeMappings ??= DefaultDataTypeMappings;

            // Explicitly set XGDefaultDataTypeMappings values take precedence over connection string options.
            if (connectionSettings.TreatTinyAsBoolean.HasValue &&
                defaultDataTypeMappings.ClrBoolean == XGBooleanType.Default)
            {
                defaultDataTypeMappings = defaultDataTypeMappings.WithClrBoolean(
                    connectionSettings.TreatTinyAsBoolean.Value
                        ? XGBooleanType.TinyInt1
                        : XGBooleanType.Bit1);
            }

            if (defaultDataTypeMappings.ClrDateTime == XGDateTimeType.Default)
            {
                defaultDataTypeMappings = defaultDataTypeMappings.WithClrDateTime(
                    ServerVersion.Supports.DateTime6
                        ? XGDateTimeType.DateTime6
                        : XGDateTimeType.DateTime);
            }

            if (defaultDataTypeMappings.ClrDateTimeOffset == XGDateTimeType.Default)
            {
                defaultDataTypeMappings = defaultDataTypeMappings.WithClrDateTimeOffset(
                    ServerVersion.Supports.DateTime6
                        ? XGDateTimeType.DateTime6
                        : XGDateTimeType.DateTime);
            }

            if (defaultDataTypeMappings.ClrTimeSpan == XGTimeSpanType.Default)
            {
                defaultDataTypeMappings = defaultDataTypeMappings.WithClrTimeSpan(
                    ServerVersion.Supports.DateTime6
                        ? XGTimeSpanType.Time6
                        : XGTimeSpanType.Time);
            }

            if (defaultDataTypeMappings.ClrTimeOnlyPrecision < 0)
            {
                defaultDataTypeMappings = defaultDataTypeMappings.WithClrTimeOnly(
                    ServerVersion.Supports.DateTime6
                        ? 6
                        : 0);
            }

            return defaultDataTypeMappings;
        }

        private static XGConnectionSettings GetConnectionSettings(XGOptionsExtension relationalOptions, IDbContextOptions options)
            => relationalOptions.Connection != null
                ? new XGConnectionSettings(relationalOptions.Connection)
                : new XGConnectionSettings(
                    new NamedConnectionStringResolver(options)
                        .ResolveConnectionString(relationalOptions.ConnectionString ?? string.Empty));

        protected virtual bool Equals(XGOptions other)
        {
            return Equals(ConnectionSettings, other.ConnectionSettings) &&
                   ReferenceEquals(DataSource, other.DataSource) &&
                   Equals(ServerVersion, other.ServerVersion) &&
                   Equals(DefaultCharSet, other.DefaultCharSet) &&
                   Equals(NationalCharSet, other.NationalCharSet) &&
                   Equals(DefaultGuidCollation, other.DefaultGuidCollation) &&
                   NoBackslashEscapes == other.NoBackslashEscapes &&
                   ReplaceLineBreaksWithCharFunction == other.ReplaceLineBreaksWithCharFunction &&
                   Equals(DefaultDataTypeMappings, other.DefaultDataTypeMappings) &&
                   Equals(SchemaNameTranslator, other.SchemaNameTranslator) &&
                   IndexOptimizedBooleanColumns == other.IndexOptimizedBooleanColumns &&
                   JsonChangeTrackingOptions == other.JsonChangeTrackingOptions &&
                   LimitKeyedOrIndexedStringColumnLength == other.LimitKeyedOrIndexedStringColumnLength &&
                   StringComparisonTranslations == other.StringComparisonTranslations &&
                   PrimitiveCollectionsSupport == other.PrimitiveCollectionsSupport;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((XGOptions)obj);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();

            hashCode.Add(ConnectionSettings);
            hashCode.Add(DataSource?.ConnectionString);
            hashCode.Add(ServerVersion);
            hashCode.Add(DefaultCharSet);
            hashCode.Add(NationalCharSet);
            hashCode.Add(DefaultGuidCollation);
            hashCode.Add(NoBackslashEscapes);
            hashCode.Add(ReplaceLineBreaksWithCharFunction);
            hashCode.Add(DefaultDataTypeMappings);
            hashCode.Add(SchemaNameTranslator);
            hashCode.Add(IndexOptimizedBooleanColumns);
            hashCode.Add(JsonChangeTrackingOptions);
            hashCode.Add(LimitKeyedOrIndexedStringColumnLength);
            hashCode.Add(StringComparisonTranslations);
            hashCode.Add(PrimitiveCollectionsSupport);

            return hashCode.ToHashCode();
        }

        public virtual XGConnectionSettings ConnectionSettings { get; private set; }

        /// <summary>
        /// If null, there might still be a `DbDataSource` in the ApplicationServiceProvider.
        /// </summary>
        public virtual DbDataSource DataSource { get; private set; }

        public virtual ServerVersion ServerVersion { get; private set; }
        public virtual CharSet DefaultCharSet { get; private set; }
        public virtual CharSet NationalCharSet { get; }
        public virtual string DefaultGuidCollation { get; private set; }
        public virtual bool NoBackslashEscapes { get; private set; }
        public virtual bool ReplaceLineBreaksWithCharFunction { get; private set; }
        public virtual XGDefaultDataTypeMappings DefaultDataTypeMappings { get; private set; }
        public virtual XGSchemaNameTranslator SchemaNameTranslator { get; private set; }
        public virtual bool IndexOptimizedBooleanColumns { get; private set; }
        public virtual XGJsonChangeTrackingOptions JsonChangeTrackingOptions { get; private set; }
        public virtual bool LimitKeyedOrIndexedStringColumnLength { get; private set; }
        public virtual bool StringComparisonTranslations { get; private set; }
        public virtual bool PrimitiveCollectionsSupport { get; private set; }
    }
}
