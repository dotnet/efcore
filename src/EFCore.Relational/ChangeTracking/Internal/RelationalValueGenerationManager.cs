// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
#pragma warning disable EF1001 // Internal EF Core API usage.

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class RelationalValueGenerationManager : ValueGenerationManager
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public RelationalValueGenerationManager(
            IValueGeneratorSelector valueGeneratorSelector,
            IKeyPropagator keyPropagator,
            IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> logger,
            ILoggingOptions loggingOptions) : base(valueGeneratorSelector, keyPropagator, logger, loggingOptions)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override void Generate(InternalEntityEntry entry, bool includePrimaryKey = true)
        {
            var entityEntry = new EntityEntry(entry);

            foreach (var property in entry.EntityType.GetValueGeneratingProperties())
            {
                if (entry.EntityState == EntityState.Added || entry.EntityState == EntityState.Detached)
                {
                    if (
                        !entry.HasDefaultValue(property) ||
                        (!includePrimaryKey && property.IsPrimaryKey()) ||
                        (!property.IsKey() && !property.ValueGenerated.ForAdd() && property.GetValueGeneratorFactory() == null)
                        )
                    {
                        continue;
                    }
                }
                else if (entry.EntityState == EntityState.Modified || entry.EntityState == EntityState.Unchanged)
                {
                    if (
                        entry.HasDefaultValue(property) ||
                        property.GetAnnotations().Any(m => m.Name == RelationalAnnotationNames.DefaultValueSql || m.Name == RelationalAnnotationNames.DefaultValue || m.Name == RelationalAnnotationNames.ComputedColumnSql) ||
                        (!includePrimaryKey && (property.IsPrimaryKey() || property.IsForeignKey())) ||
                        !property.ValueGenerated.ForUpdate()
                        )
                    {
                        continue;
                    }
                }

                var valueGenerator = GetValueGenerator(entry, property);

                var generatedValue = valueGenerator.Next(entityEntry);
                var temporary = valueGenerator.GeneratesTemporaryValues;

                Log(entry, property, generatedValue, temporary);

                SetGeneratedValue(entry, property, generatedValue, temporary);

                MarkKeyUnknown(entry, includePrimaryKey, property, valueGenerator);
            }
        }


        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override async Task GenerateAsync(
            InternalEntityEntry entry,
            bool includePrimaryKey = true,
            CancellationToken cancellationToken = default)
        {
            var entityEntry = new EntityEntry(entry);

            foreach (var property in entry.EntityType.GetValueGeneratingProperties())
            {
                if (entry.EntityState == EntityState.Added || entry.EntityState == EntityState.Detached)
                {
                    if (
                        !entry.HasDefaultValue(property) ||
                        (!includePrimaryKey && property.IsPrimaryKey()) ||
                        (!property.IsKey() && !property.ValueGenerated.ForAdd() && property.GetValueGeneratorFactory() == null)
                        )
                    {
                        continue;
                    }
                }
                if (entry.EntityState == EntityState.Modified || entry.EntityState == EntityState.Unchanged)
                {
                    if (
                        entry.HasDefaultValue(property) ||
                        property.GetAnnotations().Any(m => m.Name == RelationalAnnotationNames.DefaultValueSql || m.Name == RelationalAnnotationNames.DefaultValue || m.Name == RelationalAnnotationNames.ComputedColumnSql) ||
                        (!includePrimaryKey && (property.IsPrimaryKey() || property.IsForeignKey())) ||
                        !property.ValueGenerated.ForUpdate()
                        )
                    {
                        continue;
                    }
                }

                var valueGenerator = GetValueGenerator(entry, property);
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
    }

#pragma warning restore EF1001 // Internal EF Core API usage.
}
