// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.XuGu.Metadata.Internal
{
    public class XGAnnotationProvider : RelationalAnnotationProvider
    {
        [NotNull] private readonly IXGOptions _options;

        public XGAnnotationProvider(
            [NotNull] RelationalAnnotationProviderDependencies dependencies,
            [NotNull] IXGOptions options)
            : base(dependencies)
        {
            _options = options;
        }

        public override IEnumerable<IAnnotation> For(IRelationalModel model, bool designTime)
        {
            if (!designTime)
            {
                yield break;
            }

            if (GetActualModelCharSet(model.Model, DelegationModes.ApplyToDatabases) is string charSet)
            {
                yield return new Annotation(
                    XGAnnotationNames.CharSet,
                    charSet);
            }

            // If a collation delegation modes has been set, but does not contain DelegationModes.ApplyToDatabase, we reset the EF Core
            // handled Collation property in XGMigrationsModelDiffer.

            // Handle other annotations (including the delegation annotations).
            foreach (var annotation in model.Model.GetAnnotations()
                .Where(a => a.Name is XGAnnotationNames.CharSetDelegation or
                                      XGAnnotationNames.CollationDelegation))
            {
                yield return annotation;
            }
        }

        public override IEnumerable<IAnnotation> For(ITable table, bool designTime)
        {
            if (!designTime)
            {
                yield break;
            }

            // Model validation ensures that these facets are the same on all mapped entity types
            var entityType = (IEntityType)table.EntityTypeMappings.First().TypeBase;

            // Use an explicitly defined character set, if set.
            // Otherwise, explicitly use the model/database character set, if delegation is enabled.
            if (GetActualEntityTypeCharSet(entityType, DelegationModes.ApplyToTables) is string charSet)
            {
                yield return new Annotation(
                    XGAnnotationNames.CharSet,
                    charSet);
            }

            // Use an explicitly defined collation, if set.
            // Otherwise, explicitly use the model/database collation, if delegation is enabled.
            if (GetActualEntityTypeCollation(entityType, DelegationModes.ApplyToTables) is string collation)
            {
                yield return new Annotation(
                    RelationalAnnotationNames.Collation,
                    collation);
            }

            // Handle other annotations (including the delegation annotations).
            foreach (var annotation in entityType.GetAnnotations()
                .Where(a => a.Name is XGAnnotationNames.CharSetDelegation or
                                      XGAnnotationNames.CollationDelegation or
                                      XGAnnotationNames.StoreOptions))
            {
                yield return annotation;
            }
        }

        public override IEnumerable<IAnnotation> For(IUniqueConstraint constraint, bool designTime)
        {
            if (!designTime)
            {
                yield break;
            }

            // Model validation ensures that these facets are the same on all mapped indexes
            var key = constraint.MappedKeys.First();

            var prefixLength = key.PrefixLength();
            if (prefixLength != null &&
                prefixLength.Length > 0)
            {
                yield return new Annotation(
                    XGAnnotationNames.IndexPrefixLength,
                    prefixLength);
            }
        }

        public override IEnumerable<IAnnotation> For(ITableIndex index, bool designTime)
        {
            if (!designTime)
            {
                yield break;
            }

            // Model validation ensures that these facets are the same on all mapped indexes
            var modelIndex = index.MappedIndexes.First();

            var prefixLength = modelIndex.PrefixLength();
            if (prefixLength != null &&
                prefixLength.Length > 0)
            {
                yield return new Annotation(
                    XGAnnotationNames.IndexPrefixLength,
                    prefixLength);
            }

            var isFullText = modelIndex.IsFullText();
            if (isFullText.HasValue)
            {
                yield return new Annotation(
                    XGAnnotationNames.FullTextIndex,
                    isFullText.Value);
            }

            var fullTextParser = modelIndex.FullTextParser();
            if (!string.IsNullOrEmpty(fullTextParser))
            {
                yield return new Annotation(
                    XGAnnotationNames.FullTextParser,
                    fullTextParser);
            }

            var isSpatial = modelIndex.IsSpatial();
            if (isSpatial.HasValue)
            {
                yield return new Annotation(
                    XGAnnotationNames.SpatialIndex,
                    isSpatial.Value);
            }
        }

        public override IEnumerable<IAnnotation> For(IColumn column, bool designTime)
        {
            if (!designTime)
            {
                yield break;
            }

            var table = StoreObjectIdentifier.Table(column.Table.Name, column.Table.Schema);
            var properties = column.PropertyMappings.Select(m => m.Property).ToArray();

            if (column.PropertyMappings.Where(
                    m => (m.TableMapping.IsSharedTablePrincipal ?? true) &&
                         m.TableMapping.TypeBase == m.Property.DeclaringType)
                .Select(m => m.Property)
                .FirstOrDefault(p => p.GetValueGenerationStrategy(table) == XGValueGenerationStrategy.IdentityColumn) is IProperty identityProperty)
            {
                var valueGenerationStrategy = identityProperty.GetValueGenerationStrategy(table);
                yield return new Annotation(
                    XGAnnotationNames.ValueGenerationStrategy,
                    valueGenerationStrategy);
            }
            else if (properties.FirstOrDefault(
                p => p.GetValueGenerationStrategy(table) == XGValueGenerationStrategy.ComputedColumn) is IProperty computedProperty)
            {
                var valueGenerationStrategy = computedProperty.GetValueGenerationStrategy(table);
                yield return new Annotation(
                    XGAnnotationNames.ValueGenerationStrategy,
                    valueGenerationStrategy);
            }

            // Use an explicitly defined character set, if set.
            // Otherwise, explicitly use the entity/table or model/database character set, if delegation is enabled.
            if (GetActualPropertyCharSet(properties, DelegationModes.ApplyToColumns) is string charSet)
            {
                yield return new Annotation(
                    XGAnnotationNames.CharSet,
                    charSet);
            }

            // Use an explicitly defined collation, if set.
            // Otherwise, explicitly use the entity/table or model/database collation, if delegation is enabled.
            if (GetActualPropertyCollation(properties, DelegationModes.ApplyToColumns) is string collation)
            {
                yield return new Annotation(
                    RelationalAnnotationNames.Collation,
                    collation);
            }

            if (column.PropertyMappings.Select(m => m.Property.GetSpatialReferenceSystem())
                .FirstOrDefault(c => c != null) is int srid)
            {
                yield return new Annotation(
                    XGAnnotationNames.SpatialReferenceSystemId,
                    srid);
            }
        }

        protected virtual string GetActualModelCharSet(IModel model, DelegationModes currentLevel)
        {
            // If neither character set nor collation has been explicitly defined for the model, and no delegation has been setup, we use
            // Pomelo's universal fallback default character set (which is `utf8mb4`) and apply it to all database objects.
            return model.GetCharSet() is null &&
                   model.GetCharSetDelegation() is null &&
                   model.GetCollation() is null &&
                   model.GetCollationDelegation() is null
                ? _options.DefaultCharSet.Name
                : model.GetActualCharSetDelegation().HasFlag(currentLevel)
                    ? model.GetCharSet()
                    : null;
        }

        protected virtual string GetActualModelCollation(IModel model, DelegationModes currentLevel)
        {
            return model.GetActualCollationDelegation().HasFlag(currentLevel)
                ? model.GetCollation()
                : null;
        }

        protected virtual string GetActualEntityTypeCharSet(IEntityType entityType, DelegationModes currentLevel)
        {
            // 1. Use explicitly defined charset:
            //     entityTypeBuilder.HasCharSet(null, currentLevel)
            //     entityTypeBuilder.HasCharSet("latin1", null)
            //     entityTypeBuilder.HasCharSet("latin1", currentLevel)
            // 2. Check charset and delegation at the database level:
            //     entityTypeBuilder.HasCharSet(null, null) [or no call at all]
            //     entityTypeBuilder.HasCharSet(null, !currentLevel)
            //     entityTypeBuilder.HasCharSet("latin1", !currentLevel)
            // 3: Do not explicitly use any charset:
            //     all other cases

            var entityTypeCharSet = entityType.GetCharSet();
            var entityTypeCharSetDelegation = entityType.GetCharSetDelegation();
            var actualEntityTypeCharSetDelegation = entityType.GetActualCharSetDelegation();

            // Cases 1:
            // An explicitly set charset (which can be null) on the entity level.
            // We return it, if it also applies to the current level (which could be the property level, instead of the entity level).
            //     This enables users to set a default charset for properties of an entity, without having to also necessarily apply it to
            //     the entity itself.
            if (actualEntityTypeCharSetDelegation.HasFlag(currentLevel) &&
                (entityTypeCharSet is not null || entityTypeCharSetDelegation is not null))
            {
                return entityTypeCharSet;
            }

            //
            // At this point, no charset has been explicitly setup at the entity level, that applies to the current (entity/property) level.
            //

            // Case 2:
            // Use an explicitly set collation at the entity level, an inherited collation from the model level, or an inherited charset
            // from the model level.
            if (!actualEntityTypeCharSetDelegation.HasFlag(currentLevel) ||
                entityTypeCharSet is null && entityTypeCharSetDelegation is null)
            {
                var entityTypeCollation = entityType.GetCollation();
                var actualCollationDelegation = entityType.GetActualCollationDelegation();
                var actualModelCharSet = GetActualModelCharSet(entityType.Model, currentLevel);

                // An explicitly defined collation on the entity level takes precedence over an inherited charset from the model
                // level.
                if (entityTypeCollation is not null &&
                    actualCollationDelegation.HasFlag(currentLevel))
                {
                    // However, if the collation on the entity level is compatible with the inheritable charset from the model level, then
                    // we can apply the charset after all, since there should be no harm in doing so and it is probably the behavior that
                    // users expect.
                    return actualModelCharSet is not null &&
                           entityTypeCollation.StartsWith(actualModelCharSet, StringComparison.OrdinalIgnoreCase)
                        ? actualModelCharSet
                        : null;
                }

                var actualModelCollation = GetActualModelCollation(entityType.Model, currentLevel);

                // An inheritable collation from the model level takes precedence over an inheritable charset from the model level.
                if (actualModelCollation is not null)
                {
                    // However, if the inheritable collation from the model level is compatible with the inheritable charset from the model
                    // level (which is usually the case), then we can apply the charset after all, since there should be no harm in doing
                    // so and it is probably the behavior that users expect.
                    return actualModelCharSet is not null &&
                           actualModelCollation.StartsWith(actualModelCharSet, StringComparison.OrdinalIgnoreCase)
                        ? actualModelCharSet
                        : null;
                }

                // Return either the inherited model charset, or null if none applies to the current level.
                return actualModelCharSet;
            }

            // Case 3:
            // All remaining cases don't set a charset.
            return null;

            // return (entityType.GetCharSet() is not null || // 3abc
            //         entityType.GetCharSet() is null && entityType.GetCharSetDelegation() is not null) && // 2ab
            //        entityType.GetActualCharSetDelegation().HasFlag(currentLevel) // 3abc, 2ab
            //     ? entityType.GetCharSet()
            //     // An explicitly defined collation on the current entity level takes precedence over an inherited charset.
            //     : GetActualModelCharSet(entityType.Model, currentLevel) is string charSet && // 1
            //       (currentLevel != DelegationModes.ApplyToTables ||
            //        entityType.GetCollation() is not string collation ||
            //        !entityType.GetActualCollationDelegation().HasFlag(DelegationModes.ApplyToTables) ||
            //        collation.StartsWith(charSet, StringComparison.OrdinalIgnoreCase))
            //         ? charSet
            //         : null;
        }

        protected virtual string GetActualEntityTypeCollation(IEntityType entityType, DelegationModes currentLevel)
        {
            // 1. Use explicitly defined collation:
            //     entityTypeBuilder.HasCollation(null, currentLevel)
            //     entityTypeBuilder.HasCollation("latin1_general_ci", null)
            //     entityTypeBuilder.HasCollation("latin1_general_ci", currentLevel)
            // 2. Check collation and delegation at the database level:
            //     entityTypeBuilder.HasCollation(null, null) [or no call at all]
            //     entityTypeBuilder.HasCollation(null, !currentLevel)
            //     entityTypeBuilder.HasCollation("latin1_general_ci", !currentLevel)
            // 3: Do not explicitly use any collation:
            //     all other cases

            var entityTypeCollation = entityType.GetCollation();
            var entityTypeCollationDelegation = entityType.GetCollationDelegation();
            var actualEntityTypeCollationDelegation = entityType.GetActualCollationDelegation();

            // Cases 1:
            // An explicitly set collation (which can be null) on the entity level.
            // We return it, if it applies to the current level (which could be the property level, instead of the entity level).
            //     This enables users to set a default collation for properties of an entity, without having to also necessarily apply it to
            //     the entity itself.
            if (actualEntityTypeCollationDelegation.HasFlag(currentLevel) &&
                (entityTypeCollation is not null || entityTypeCollationDelegation is not null))
            {
                return entityTypeCollation;
            }

            //
            // At this point, no collation has been explicitly setup at the entity level, that applies to the current (entity/property) level.
            //

            // Case 2:
            // Use an explicitly set charset at the entity level, an inheritable collation from the model level, or an inheritable charset
            // from the model level.
            if (!actualEntityTypeCollationDelegation.HasFlag(currentLevel) ||
                entityTypeCollation is null && entityTypeCollationDelegation is null)
            {
                var entityTypeCharSet = entityType.GetCharSet();
                var actualCharSetDelegation = entityType.GetActualCharSetDelegation();
                var actualModelCollation = GetActualModelCollation(entityType.Model, currentLevel);

                // An explicitly defined charset on the entity level takes precedence over an inherited collation from the model
                // level.
                if (entityTypeCharSet is not null &&
                    actualCharSetDelegation.HasFlag(currentLevel))
                {
                    // However, if the charset on the entity level is compatible with the inheritable collation from the model level, then
                    // we can apply the collation after all, since there should be no harm in doing so and it is probably the behavior that
                    // users expect.
                    return actualModelCollation is not null &&
                           actualModelCollation.StartsWith(entityTypeCharSet, StringComparison.OrdinalIgnoreCase)
                        ? actualModelCollation
                        : null;
                }

                // Return either the inherited model collation, or null if none applies to the current level.
                return actualModelCollation;
            }

            // Case 3:
            // All remaining cases don't set a collation.
            return null;

            // return (entityType.GetCollation() is not null || // 3abc
            //         entityType.GetCollation() is null && entityType.GetCollationDelegation() is not null) && // 2ab
            //        entityType.GetActualCollationDelegation().HasFlag(currentLevel)
            //     ? entityType.GetCollation()
            //     // An explicitly defined charset on the current entity level takes precedence over an inherited collation.
            //     : GetActualModelCollation(entityType.Model, currentLevel) is string collation && // 1
            //       (currentLevel != DelegationModes.ApplyToTables ||
            //        entityType.GetCharSet() is not string charSet ||
            //        !entityType.GetActualCharSetDelegation().HasFlag(DelegationModes.ApplyToTables) ||
            //        collation.StartsWith(charSet, StringComparison.OrdinalIgnoreCase))
            //         ? collation
            //         : null;
        }

        protected virtual string GetActualPropertyCharSet(IProperty[] properties, DelegationModes currentLevel)
        {
            return properties.Select(p => p.GetCharSet()).FirstOrDefault(s => s is not null) ??
                   properties.Select(
                           p => p.FindTypeMapping() is XGStringTypeMapping {IsNationalChar: false}
                               // An explicitly defined collation on the current property level takes precedence over an inherited charset.
                               ? p.DeclaringType is IEntityType entityType &&
                                 GetActualEntityTypeCharSet(entityType, currentLevel) is string charSet &&
                                 (p.GetCollation() is not string collation ||
                                  collation.StartsWith(charSet, StringComparison.OrdinalIgnoreCase))
                                   ? charSet
                                   : null
                               : null)
                       .FirstOrDefault(s => s is not null);
        }

        protected virtual string GetActualPropertyCollation(IProperty[] properties, DelegationModes currentLevel)
        {
            Debug.Assert(currentLevel == DelegationModes.ApplyToColumns);

            // We have been using the `XG:Collation` annotation before EF Core added collation support.
            // Our `XGPropertyExtensions.GetXGLegacyCollation()` method handles the legacy case, so we explicitly
            // call it here and setup the relational annotation, even though EF Core sets it up as well.
            // This ensures, that from this point onwards, only the `Relational:Collation` annotation is being used.
            //
            // If no collation has been set, explicitly use the the model/database collation, if delegation is enabled.
            //
            // The exception are Guid properties when the GuidFormat has been set to a char-based format, in which case we will use the
            // default guid collation setup for the model, or the fallback default from our options if none has been set, to optimize space
            // and performance for those columns. (We ignore the binary format, because its charset and collation is always `binary`.)
            return properties.All(p => p.GetCollation() is null)
                ? properties.Select(p => p.GetXGLegacyCollation()).FirstOrDefault(c => c is not null) ??
                  properties.Select(
                          // An explicitly defined charset on the current property level takes precedence over an inherited collation.
                          p => (p.FindTypeMapping() is XGStringTypeMapping {IsNationalChar: false} &&
                                p.DeclaringType is IEntityType entityType
                                   ? GetActualEntityTypeCollation(entityType, currentLevel)
                                   : p.FindTypeMapping() is XGGuidTypeMapping
                                       ? p.DeclaringType.Model.GetActualGuidCollation(_options.DefaultGuidCollation)
                                       : null) is string collation &&
                               (p.GetCharSet() is not string charSet ||
                                collation.StartsWith(charSet, StringComparison.OrdinalIgnoreCase))
                              ? collation
                              : null)
                      .FirstOrDefault(s => s is not null)
                : null;
        }
    }
}
