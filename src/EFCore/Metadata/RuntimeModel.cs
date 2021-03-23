// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     <para>
    ///         Metadata about the shape of entities, the relationships between them, and how they map to
    ///         the database. A model is typically created by overriding the
    ///         <see cref="DbContext.OnModelCreating(ModelBuilder)" /> method on a derived
    ///         <see cref="DbContext" />.
    ///     </para>
    ///     <para>
    ///         This is a light-weight implementation that is constructed from a built model and is not meant to be used at design-time.
    ///     </para>
    /// </summary>
    public class RuntimeModel : AnnotatableBase, IRuntimeModel
    {
        private readonly SortedDictionary<string, RuntimeEntityType> _entityTypes = new(StringComparer.Ordinal);
        private readonly ConcurrentDictionary<Type, PropertyInfo?> _indexerPropertyInfoMap = new();
        private readonly ConcurrentDictionary<Type, string> _clrTypeNameMap = new();
        private readonly Dictionary<Type, SortedSet<RuntimeEntityType>> _sharedTypes = new();
        private bool _skipDetectChanges;

        /// <summary>
        ///     Creates a new instance of <seealso cref="SlimModel"/>
        /// </summary>
        public RuntimeModel()
        {
        }

        /// <summary>
        ///     Initializes this model instance.
        /// </summary>
        protected virtual void Initialize()
        {
        }

        /// <summary>
        ///     Sets a value indicating whether <see cref="ChangeTracker.DetectChanges" /> should be called.
        /// </summary>
        public virtual void SetSkipDetectChanges(bool skipDetectChanges)
        {
            _skipDetectChanges = skipDetectChanges;
        }

        /// <summary>
        ///     Adds an entity type with a defining navigation to the model.
        /// </summary>
        /// <param name="name"> The name of the entity type to be added. </param>
        /// <param name="type"> The CLR class that is used to represent instances of this type. </param>
        /// <param name="sharedClrType"> Whether this entity type can share its ClrType with other entities. </param>
        /// <param name="baseType"> The base type of this entity type. </param>
        /// <param name="discriminatorProperty"> The name of the property that will be used for storing a discriminator value. </param>
        /// <param name="changeTrackingStrategy"> The change tracking strategy for this entity type </param>
        /// <param name="indexerPropertyInfo"> The <seealso cref="PropertyInfo"/> for the indexer on the associated CLR type if one exists. </param>
        /// <param name="propertyBag">
        ///     A value indicating whether this entity type has an indexer which is able to contain arbitrary properties
        ///     and a method that can be used to determine whether a given indexer property contains a value.
        /// </param>
        /// <returns> The new entity type. </returns>
        public virtual RuntimeEntityType AddEntityType(
            string name,
            Type type,
            RuntimeEntityType? baseType = null,
            bool sharedClrType = false,
            string? discriminatorProperty = null,
            ChangeTrackingStrategy changeTrackingStrategy = ChangeTrackingStrategy.Snapshot,
            PropertyInfo? indexerPropertyInfo = null,
            bool propertyBag = false)
        {
            var entityType = new RuntimeEntityType(
                name,
                type,
                sharedClrType,
                this,
                baseType,
                discriminatorProperty,
                changeTrackingStrategy,
                indexerPropertyInfo,
                propertyBag);

            if (sharedClrType)
            {
                if (_sharedTypes.TryGetValue(type, out var existingTypes))
                {
                    existingTypes.Add(entityType);
                }
                else
                {
                    var types = new SortedSet<RuntimeEntityType>(EntityTypeFullNameComparer.Instance) { entityType };
                    _sharedTypes.Add(type, types);
                }
            }

            _entityTypes.Add(name, entityType);

            return entityType;
        }

        /// <summary>
        ///     Gets the entity type with the given name. Returns <see langword="null"/> if no entity type with the given name is found
        ///     or the given CLR type is being used by shared type entity type
        ///     or the entity type has a defining navigation.
        /// </summary>
        /// <param name="name"> The name of the entity type to find. </param>
        /// <returns> The entity type, or <see langword="null"/> if none is found. </returns>
        public virtual RuntimeEntityType? FindEntityType(string name)
            => _entityTypes.TryGetValue(name, out var entityType)
                ? entityType
                : null;

        private RuntimeEntityType? FindEntityType(Type type)
            => FindEntityType(GetDisplayName(type));

        private RuntimeEntityType? FindEntityType(
            string name,
            string definingNavigationName,
            IReadOnlyEntityType definingEntityType)
            => FindEntityType(definingEntityType.GetOwnedName(name, definingNavigationName));

        private IEnumerable<RuntimeEntityType> FindEntityTypes(Type type)
        {
            var entityType = FindEntityType(GetDisplayName(type));
            var result = entityType == null
                ? Array.Empty<RuntimeEntityType>()
                : new[] { entityType };

            return _sharedTypes.TryGetValue(type, out var sharedTypes)
                ? result.Concat(sharedTypes)
                : result;
        }

        private string GetDisplayName(Type type)
            => _clrTypeNameMap.GetOrAdd(type, t => t.DisplayName());

        private PropertyInfo? FindIndexerPropertyInfo(Type type)
            => _indexerPropertyInfoMap.GetOrAdd(type, type.FindIndexerProperty());

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        object? IRuntimeModel.RelationalModel
            => ((IAnnotatable)this).FindRuntimeAnnotationValue("Relational:RelationalModel");

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public virtual DebugView DebugView
            => new(
                () => ((IReadOnlyModel)this).ToDebugString(MetadataDebugStringOptions.ShortDefault),
                () => ((IReadOnlyModel)this).ToDebugString(MetadataDebugStringOptions.LongDefault));

        /// <inheritdoc/>
        bool IRuntimeModel.SkipDetectChanges
        {
            [DebuggerStepThrough]
            get => _skipDetectChanges;
        }

        /// <inheritdoc/>
        [DebuggerStepThrough]
        PropertyAccessMode IReadOnlyModel.GetPropertyAccessMode()
            => throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData);

        /// <inheritdoc/>
        [DebuggerStepThrough]
        ChangeTrackingStrategy IReadOnlyModel.GetChangeTrackingStrategy()
            => throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData);

        /// <inheritdoc/>
        [DebuggerStepThrough]
        bool IModel.IsIndexerMethod(MethodInfo methodInfo)
            => !methodInfo.IsStatic
                && methodInfo.IsSpecialName
                && methodInfo.DeclaringType != null
                && FindIndexerPropertyInfo(methodInfo.DeclaringType) is PropertyInfo indexerProperty
                && (methodInfo == indexerProperty.GetMethod || methodInfo == indexerProperty.SetMethod);

        /// <inheritdoc/>
        [DebuggerStepThrough]
        IReadOnlyEntityType? IReadOnlyModel.FindEntityType(string name)
            => FindEntityType(name);

        /// <inheritdoc/>
        [DebuggerStepThrough]
        IEntityType? IModel.FindEntityType(string name)
            => FindEntityType(name);

        /// <inheritdoc/>
        [DebuggerStepThrough]
        IReadOnlyEntityType? IReadOnlyModel.FindEntityType(Type type)
            => FindEntityType(type);

        /// <inheritdoc/>
        [DebuggerStepThrough]
        IEntityType? IModel.FindEntityType(Type type)
            => FindEntityType(type);

        /// <inheritdoc/>
        [DebuggerStepThrough]
        IReadOnlyEntityType? IReadOnlyModel.FindEntityType(string name, string definingNavigationName, IReadOnlyEntityType definingEntityType)
            => FindEntityType(name, definingNavigationName, (RuntimeEntityType)definingEntityType);

        /// <inheritdoc/>
        [DebuggerStepThrough]
        IEntityType? IModel.FindEntityType(
            string name,
            string definingNavigationName,
            IEntityType definingEntityType)
            => FindEntityType(name, definingNavigationName, (RuntimeEntityType)definingEntityType);

        /// <inheritdoc/>
        [DebuggerStepThrough]
        IReadOnlyEntityType? IReadOnlyModel.FindEntityType(
            Type type,
            string definingNavigationName,
            IReadOnlyEntityType definingEntityType)
            => FindEntityType(type.ShortDisplayName(), definingNavigationName, definingEntityType);

        /// <inheritdoc/>
        [DebuggerStepThrough]
        IEnumerable<IReadOnlyEntityType> IReadOnlyModel.GetEntityTypes()
            => _entityTypes.Values;

        /// <inheritdoc/>
        [DebuggerStepThrough]
        IEnumerable<IEntityType> IModel.GetEntityTypes()
            => _entityTypes.Values;

        /// <inheritdoc/>
        [DebuggerStepThrough]
        IEnumerable<IReadOnlyEntityType> IReadOnlyModel.FindEntityTypes(Type type)
            => FindEntityTypes(type);

        /// <inheritdoc/>
        [DebuggerStepThrough]
        IEnumerable<IEntityType> IModel.FindEntityTypes(Type type)
            => FindEntityTypes(type);

        /// <inheritdoc/>
        [DebuggerStepThrough]
        bool IReadOnlyModel.IsShared(Type type)
            => _sharedTypes.ContainsKey(type);
    }
}
