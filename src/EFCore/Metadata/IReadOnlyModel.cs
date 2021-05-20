// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Metadata about the shape of entities, the relationships between them, and how they map to
    ///     the database. A model is typically created by overriding the
    ///     <see cref="DbContext.OnModelCreating(ModelBuilder)" /> method on a derived <see cref="DbContext" />.
    /// </summary>
    public interface IReadOnlyModel : IReadOnlyAnnotatable
    {
        /// <summary>
        ///     Gets the default change tracking strategy being used for entities in the model. This strategy indicates how the
        ///     context detects changes to properties for an instance of an entity type.
        /// </summary>
        /// <returns> The change tracking strategy. </returns>
        ChangeTrackingStrategy GetChangeTrackingStrategy();

        /// <summary>
        ///     <para>
        ///         Gets the <see cref="PropertyAccessMode" /> being used for properties of entity types in this model.
        ///     </para>
        ///     <para>
        ///         Note that individual entity types can override this access mode, and individual properties of
        ///         entity types can override the access mode set on the entity type. The value returned here will
        ///         be used for any property for which no override has been specified.
        ///     </para>
        /// </summary>
        /// <returns> The access mode being used. </returns>
        [DebuggerStepThrough]
        PropertyAccessMode GetPropertyAccessMode();

        /// <summary>
        ///     Gets the EF Core assembly version used to build this model.
        /// </summary>
        string? GetProductVersion()
            => this[CoreAnnotationNames.ProductVersion] as string;

        /// <summary>
        ///     Gets a value indicating whether the CLR type is used by shared type entities in the model.
        /// </summary>
        /// <param name="type"> The CLR type. </param>
        /// <returns> Whether the CLR type is used by shared type entities in the model. </returns>
        bool IsShared(Type type);

        /// <summary>
        ///     Gets all entity types defined in the model.
        /// </summary>
        /// <returns> All entity types defined in the model. </returns>
        IEnumerable<IReadOnlyEntityType> GetEntityTypes();

        /// <summary>
        ///     Gets the entity type with the given name. Returns <see langword="null"/> if no entity type with the given name is found
        ///     or the given CLR type is being used by shared type entity type
        ///     or the entity type has a defining navigation.
        /// </summary>
        /// <param name="name"> The name of the entity type to find. </param>
        /// <returns> The entity type, or <see langword="null"/> if none is found. </returns>
        IReadOnlyEntityType? FindEntityType(string name);

        /// <summary>
        ///     Gets the entity type for the given base name, defining navigation name
        ///     and the defining entity type. Returns <see langword="null"/> if no matching entity type is found.
        /// </summary>
        /// <param name="name"> The name of the entity type to find. </param>
        /// <param name="definingNavigationName"> The defining navigation of the entity type to find. </param>
        /// <param name="definingEntityType"> The defining entity type of the entity type to find. </param>
        /// <returns> The entity type, or <see langword="null"/> if none is found. </returns>
        IReadOnlyEntityType? FindEntityType(
            string name,
            string definingNavigationName,
            IReadOnlyEntityType definingEntityType);

        /// <summary>
        ///     Gets the entity that maps the given entity class. Returns <see langword="null" /> if no entity type with
        ///     the given CLR type is found or the given CLR type is being used by shared type entity type
        ///     or the entity type has a defining navigation.
        /// </summary>
        /// <param name="type"> The type to find the corresponding entity type for. </param>
        /// <returns> The entity type, or <see langword="null" /> if none is found. </returns>
        IReadOnlyEntityType? FindEntityType(Type type);

        /// <summary>
        ///     Gets the entity type for the given type, defining navigation name
        ///     and the defining entity type. Returns <see langword="null" /> if no matching entity type is found.
        /// </summary>
        /// <param name="type"> The type of the entity type to find. </param>
        /// <param name="definingNavigationName"> The defining navigation of the entity type to find. </param>
        /// <param name="definingEntityType"> The defining entity type of the entity type to find. </param>
        /// <returns> The entity type, or <see langword="null" /> if none is found. </returns>
        IReadOnlyEntityType? FindEntityType(
            Type type,
            string definingNavigationName,
            IReadOnlyEntityType definingEntityType);

        /// <summary>
        ///     Gets the entity types matching the given type.
        /// </summary>
        /// <param name="type"> The type of the entity type to find. </param>
        /// <returns> The entity types found. </returns>
        IEnumerable<IReadOnlyEntityType> FindEntityTypes(Type type);

        /// <summary>
        ///     Returns the entity types corresponding to the least derived types from the given.
        /// </summary>
        /// <param name="type"> The base type. </param>
        /// <param name="condition"> An optional condition for filtering entity types. </param>
        /// <returns> List of entity types corresponding to the least derived types from the given. </returns>
        IEnumerable<IReadOnlyEntityType> FindLeastDerivedEntityTypes(
            Type type,
            Func<IReadOnlyEntityType, bool>? condition = null)
        {
            var derivedLevels = new Dictionary<Type, int> { [type] = 0 };

            var leastDerivedTypesGroups = GetEntityTypes()
                .GroupBy(t => GetDerivedLevel(t.ClrType, derivedLevels))
                .Where(g => g.Key != int.MaxValue)
                .OrderBy(g => g.Key);

            foreach (var leastDerivedTypes in leastDerivedTypesGroups)
            {
                if (condition == null)
                {
                    return leastDerivedTypes.ToList();
                }

                var filteredTypes = leastDerivedTypes.Where(condition).ToList();
                if (filteredTypes.Count > 0)
                {
                    return filteredTypes;
                }
            }

            return Enumerable.Empty<IReadOnlyEntityType>();
        }

        private static int GetDerivedLevel(Type? derivedType, Dictionary<Type, int> derivedLevels)
        {
            if (derivedType?.BaseType == null)
            {
                return int.MaxValue;
            }

            if (derivedLevels.TryGetValue(derivedType, out var level))
            {
                return level;
            }

            var baseType = derivedType.BaseType;
            level = GetDerivedLevel(baseType, derivedLevels);
            level += level == int.MaxValue ? 0 : 1;
            derivedLevels.Add(derivedType, level);
            return level;
        }

        /// <summary>
        ///     <para>
        ///         Creates a human-readable representation of the given metadata.
        ///     </para>
        ///     <para>
        ///         Warning: Do not rely on the format of the returned string.
        ///         It is designed for debugging only and may change arbitrarily between releases.
        ///     </para>
        /// </summary>
        /// <param name="options"> Options for generating the string. </param>
        /// <param name="indent"> The number of indent spaces to use before each new line. </param>
        /// <returns> A human-readable representation. </returns>
        string ToDebugString(MetadataDebugStringOptions options = MetadataDebugStringOptions.ShortDefault, int indent = 0)
        {
            var builder = new StringBuilder();
            var indentString = new string(' ', indent);

            builder.Append(indentString).Append("Model: ");

            if (this is Model
                && GetChangeTrackingStrategy() != ChangeTrackingStrategy.Snapshot)
            {
                builder.Append(" ChangeTrackingStrategy.").Append(GetChangeTrackingStrategy());
            }

            foreach (var entityType in GetEntityTypes())
            {
                builder.AppendLine().Append(entityType.ToDebugString(options, indent + 2));
            }

            if ((options & MetadataDebugStringOptions.IncludeAnnotations) != 0)
            {
                builder.Append(AnnotationsToDebugString(indent));
            }

            return builder.ToString();
        }
    }
}
