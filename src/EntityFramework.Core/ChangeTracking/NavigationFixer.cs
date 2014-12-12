// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class NavigationFixer : IEntityStateListener, IRelationshipListener
    {
        private readonly ClrPropertySetterSource _setterSource;
        private readonly ClrPropertyGetterSource _getterSource;
        private readonly ClrCollectionAccessorSource _collectionAccessorSource;
        private readonly DbContextService<IModel> _model;
        private bool _inFixup;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected NavigationFixer()
        {
        }

        public NavigationFixer(
            [NotNull] ClrPropertyGetterSource getterSource,
            [NotNull] ClrPropertySetterSource setterSource,
            [NotNull] ClrCollectionAccessorSource collectionAccessorSource,
            [NotNull] DbContextService<IModel> model)
        {
            Check.NotNull(getterSource, "getterSource");
            Check.NotNull(setterSource, "setterSource");
            Check.NotNull(collectionAccessorSource, "collectionAccessorSource");
            Check.NotNull(model, "model");

            _getterSource = getterSource;
            _setterSource = setterSource;
            _collectionAccessorSource = collectionAccessorSource;
            _model = model;
        }

        public virtual void ForeignKeyPropertyChanged(StateEntry entry, IProperty property, object oldValue, object newValue)
        {
            Check.NotNull(entry, "entry");
            Check.NotNull(property, "property");

            PerformFixup(() => ForeignKeyPropertyChangedAction(entry, property, oldValue, newValue));
        }

        private void ForeignKeyPropertyChangedAction(StateEntry entry, IProperty property, object oldValue, object newValue)
        {
            foreach (var foreignKey in entry.EntityType.ForeignKeys.Where(p => p.Properties.Contains(property)).Distinct())
            {
                var navigations = _model.Service.GetNavigations(foreignKey).ToList();

                var oldPrincipalEntry = entry.StateManager.GetPrincipal(entry.RelationshipsSnapshot, foreignKey);
                if (oldPrincipalEntry != null)
                {
                    Unfixup(navigations, oldPrincipalEntry, entry);
                }

                var principalEntry = entry.StateManager.GetPrincipal(entry, foreignKey);
                if (principalEntry != null)
                {
                    if (foreignKey.IsUnique)
                    {
                        var oldDependents = entry.StateManager.GetDependents(principalEntry, foreignKey).Where(e => e != entry).ToList();

                        // TODO: Decide how to handle case where multiple values found (negative case)
                        // Issue #739
                        if (oldDependents.Count > 0)
                        {
                            StealReference(foreignKey, oldDependents[0]);
                        }
                    }

                    DoFixup(navigations, principalEntry, new[] { entry });
                }
            }
        }

        public virtual void NavigationReferenceChanged(StateEntry entry, INavigation navigation, object oldValue, object newValue)
        {
            Check.NotNull(entry, "entry");
            Check.NotNull(navigation, "navigation");

            PerformFixup(() => NavigationReferenceChangedAction(entry, navigation, oldValue, newValue));
        }

        private void NavigationReferenceChangedAction(StateEntry entry, INavigation navigation, object oldValue, object newValue)
        {
            var foreignKey = navigation.ForeignKey;
            var dependentProperties = foreignKey.Properties;
            var principalProperties = foreignKey.ReferencedProperties;

            // TODO: What if the other entry is not yet being tracked?
            // Issue #323
            if (navigation.PointsToPrincipal)
            {
                if (newValue != null)
                {
                    SetForeignKeyValue(foreignKey, entry, entry.StateManager.GetOrCreateEntry(newValue));
                }
                else
                {
                    SetNullForeignKey(entry, dependentProperties);
                }
            }
            else
            {
                Debug.Assert(foreignKey.IsUnique);

                if (newValue != null)
                {
                    SetForeignKeyValue(foreignKey, entry.StateManager.GetOrCreateEntry(newValue), entry);
                }

                if (oldValue != null)
                {
                    ConditionallySetNullForeignKey(entry.StateManager.GetOrCreateEntry(oldValue), dependentProperties, entry, principalProperties);
                }
            }

            if (oldValue != null)
            {
                ConditionallyClearInverse(entry, navigation, oldValue);
            }

            if (newValue != null)
            {
                SetInverse(entry, navigation, newValue);
            }
        }

        public virtual void NavigationCollectionChanged(StateEntry entry, INavigation navigation, ISet<object> added, ISet<object> removed)
        {
            Check.NotNull(entry, "entry");
            Check.NotNull(navigation, "navigation");
            Check.NotNull(added, "added");
            Check.NotNull(removed, "removed");

            PerformFixup(() => NavigationCollectionChangedAction(entry, navigation, added, removed));
        }

        private void NavigationCollectionChangedAction(
            StateEntry entry, INavigation navigation, IEnumerable<object> added, IEnumerable<object> removed)
        {
            Debug.Assert(navigation.IsCollection());

            var principalProperties = navigation.ForeignKey.ReferencedProperties;
            var dependentProperties = navigation.ForeignKey.Properties;
            var principalValues = principalProperties.Select(p => entry[p]).ToList();

            // TODO: What if the entity is not yet being tracked?
            // Issue #323
            foreach (var entity in removed)
            {
                ConditionallySetNullForeignKey(entry.StateManager.GetOrCreateEntry(entity), dependentProperties, principalValues);
                ConditionallyClearInverse(entry, navigation, entity);
            }

            foreach (var entity in added)
            {
                SetForeignKeyValue(navigation.ForeignKey, entry.StateManager.GetOrCreateEntry(entity), principalValues);
                SetInverse(entry, navigation, entity);
            }
        }

        public virtual void PrincipalKeyPropertyChanged(StateEntry entry, IProperty property, object oldValue, object newValue)
        {
            Check.NotNull(entry, "entry");
            Check.NotNull(property, "property");

            // We don't prevent recursive entry here because changed of principal key can have cascading effects
            // when principal key is also foreign key.

            foreach (var foreignKey in _model.Service.EntityTypes.SelectMany(
                e => e.ForeignKeys.Where(f => f.ReferencedProperties.Contains(property))))
            {
                var newKeyValues = foreignKey.ReferencedProperties.Select(p => entry[p]).ToList();
                var oldKey = entry.RelationshipsSnapshot.GetPrincipalKeyValue(foreignKey);

                foreach (var dependent in entry.StateManager.StateEntries.Where(
                    e => e.EntityType == foreignKey.EntityType
                         && oldKey.Equals(e.GetDependentKeyValue(foreignKey))).ToList())
                {
                    SetForeignKeyValue(foreignKey, dependent, newKeyValues);
                }
            }
        }

        public virtual void StateChanging(StateEntry entry, EntityState newState)
        {
        }

        public virtual void StateChanged(StateEntry entry, EntityState oldState)
        {
            Check.NotNull(entry, "entry");
            Check.IsDefined(oldState, "oldState");

            if (oldState != EntityState.Unknown)
            {
                return;
            }

            PerformFixup(() => InitialFixup(entry, oldState));
        }

        private void InitialFixup(StateEntry entry, EntityState oldState)
        {
            var entityType = entry.EntityType;

            foreach (var navigation in entityType.Navigations)
            {
                var navigationValue = entry[navigation];

                if (navigationValue != null)
                {
                    if (navigation.IsCollection())
                    {
                        NavigationCollectionChangedAction(
                            entry,
                            navigation,
                            ((IEnumerable)navigationValue).Cast<object>().ToList(),
                            Enumerable.Empty<object>());
                    }
                    else
                    {
                        NavigationReferenceChangedAction(
                            entry,
                            navigation,
                            null,
                            navigationValue);
                    }
                }
            }

            var stateEntries = entry.StateManager.StateEntries.ToList();

            // TODO: Perf on this state manager query
            foreach (var navigation in _model.Service.EntityTypes
                .SelectMany(e => e.Navigations)
                .Where(n => n.GetTargetType() == entityType))
            {
                IClrCollectionAccessor collectionAccessor = null;
                if (navigation.IsCollection())
                {
                    collectionAccessor = _collectionAccessorSource.GetAccessor(navigation);
                }

                var navigationEntityType = navigation.EntityType;

                foreach (var relatedEntry in stateEntries)
                {
                    if (relatedEntry.EntityType != navigationEntityType
                        || relatedEntry == entry)
                    {
                        continue;
                    }

                    if (collectionAccessor != null)
                    {
                        if (collectionAccessor.Contains(relatedEntry.Entity, entry.Entity))
                        {
                            NavigationCollectionChangedAction(
                                relatedEntry,
                                navigation,
                                new[] { entry.Entity },
                                Enumerable.Empty<object>());
                        }
                    }
                    else
                    {
                        var navigationValue = relatedEntry[navigation];

                        if (navigationValue != null)
                        {
                            if (ReferenceEquals(navigationValue, entry.Entity))
                            {
                                NavigationReferenceChangedAction(
                                    relatedEntry,
                                    navigation,
                                    null,
                                    navigationValue);
                            }
                        }
                    }
                }
            }

            foreach (var foreignKey in entityType.ForeignKeys)
            {
                var principalEntry = entry.StateManager.GetPrincipal(entry.RelationshipsSnapshot, foreignKey);
                if (principalEntry != null)
                {
                    DoFixup(foreignKey, principalEntry, new[] { entry });
                }
            }

            foreach (var foreignKey in _model.Service.GetReferencingForeignKeys(entityType))
            {
                var dependents = entry.StateManager.GetDependents(entry, foreignKey).ToArray();

                if (dependents.Length > 0)
                {
                    DoFixup(foreignKey, entry, dependents);
                }
            }
        }

        private void PerformFixup(Action fixupAction)
        {
            if (_inFixup)
            {
                return;
            }

            try
            {
                _inFixup = true;

                fixupAction();
            }
            finally
            {
                _inFixup = false;
            }
        }

        private void DoFixup(IForeignKey foreignKey, StateEntry principalEntry, StateEntry[] dependentEntries)
        {
            DoFixup(_model.Service.GetNavigations(foreignKey).ToList(), principalEntry, dependentEntries);
        }

        private void DoFixup(IEnumerable<INavigation> navigations, StateEntry principalEntry, StateEntry[] dependentEntries)
        {
            foreach (var navigation in navigations)
            {
                if (navigation.PointsToPrincipal)
                {
                    var setter = _setterSource.GetAccessor(navigation);

                    foreach (var dependent in dependentEntries)
                    {
                        setter.SetClrValue(dependent.Entity, principalEntry.Entity);
                        dependent.RelationshipsSnapshot.TakeSnapshot(navigation);
                    }
                }
                else
                {
                    if (navigation.IsCollection())
                    {
                        var collectionAccessor = _collectionAccessorSource.GetAccessor(navigation);

                        foreach (var dependent in dependentEntries)
                        {
                            if (!collectionAccessor.Contains(principalEntry.Entity, dependent.Entity))
                            {
                                collectionAccessor.Add(principalEntry.Entity, dependent.Entity);
                            }
                        }
                    }
                    else
                    {
                        // TODO: Decide how to handle case where multiple values match non-collection nav prop
                        // Issue #739
                        _setterSource.GetAccessor(navigation).SetClrValue(principalEntry.Entity, dependentEntries.Single().Entity);
                    }
                    principalEntry.RelationshipsSnapshot.TakeSnapshot(navigation);
                }
            }
        }

        private void Unfixup(IEnumerable<INavigation> navigations, StateEntry oldPrincipalEntry, StateEntry dependentEntry)
        {
            foreach (var navigation in navigations)
            {
                Unfixup(navigation, oldPrincipalEntry, dependentEntry);
                oldPrincipalEntry.RelationshipsSnapshot.TakeSnapshot(navigation);
            }
        }

        private void Unfixup(INavigation navigation, StateEntry oldPrincipalEntry, StateEntry dependentEntry)
        {
            if (navigation.PointsToPrincipal)
            {
                _setterSource.GetAccessor(navigation).SetClrValue(dependentEntry.Entity, null);
                dependentEntry.RelationshipsSnapshot.TakeSnapshot(navigation);
            }
            else
            {
                if (navigation.IsCollection())
                {
                    var collectionAccessor = _collectionAccessorSource.GetAccessor(navigation);
                    if (collectionAccessor.Contains(oldPrincipalEntry.Entity, dependentEntry.Entity))
                    {
                        collectionAccessor.Remove(oldPrincipalEntry.Entity, dependentEntry.Entity);
                    }
                }
                else
                {
                    _setterSource.GetAccessor(navigation).SetClrValue(oldPrincipalEntry.Entity, null);
                }
            }
        }

        private void StealReference(IForeignKey foreignKey, StateEntry dependentEntry)
        {
            foreach (var navigation in dependentEntry.EntityType.Navigations.Where(n => n.ForeignKey == foreignKey))
            {
                if (navigation.PointsToPrincipal)
                {
                    _setterSource.GetAccessor(navigation).SetClrValue(dependentEntry.Entity, null);
                    dependentEntry.RelationshipsSnapshot.TakeSnapshot(navigation);
                }
            }

            var nullableProperties = foreignKey.Properties.Where(p => p.IsNullable).ToList();
            if (nullableProperties.Count > 0)
            {
                foreach (var property in nullableProperties)
                {
                    dependentEntry[property] = null;
                }
            }
        }

        private static void SetForeignKeyValue(
            IForeignKey foreignKey,
            StateEntry dependentEntry,
            StateEntry principalEntry)
        {
            SetForeignKeyValue(foreignKey, dependentEntry, foreignKey.ReferencedProperties.Select(p => principalEntry[p]).ToList());
        }

        private static void SetForeignKeyValue(IForeignKey foreignKey, StateEntry dependentEntry, IReadOnlyList<object> principalValues)
        {
            for (var i = 0; i < foreignKey.Properties.Count; i++)
            {
                if (!foreignKey.FindRootValueGenerationProperty(i).GenerateValueOnAdd
                    || !foreignKey.ReferencedProperties[i].PropertyType.IsDefaultValue(principalValues[i]))
                {
                    // TODO: Consider nullable/non-nullable assignment issues
                    // Issue #740
                    var dependentProperty = foreignKey.Properties[i];
                    dependentEntry[dependentProperty] = principalValues[i];
                    dependentEntry.RelationshipsSnapshot.TakeSnapshot(dependentProperty);
                }
            }
        }

        private static void ConditionallySetNullForeignKey(
            StateEntry dependentEntry, IReadOnlyList<IProperty> dependentProperties,
            StateEntry principalEntry, IReadOnlyList<IProperty> principalProperties)
        {
            ConditionallySetNullForeignKey(dependentEntry, dependentProperties, principalProperties.Select(p => principalEntry[p]).ToList());
        }

        private static void ConditionallySetNullForeignKey(
            StateEntry dependentEntry, IReadOnlyList<IProperty> dependentProperties, IReadOnlyList<object> principalValues)
        {
            // Don't null out the FK if it has already be set to point to a different principal
            if (dependentProperties.Select(p => dependentEntry[p]).StructuralSequenceEqual(principalValues))
            {
                SetNullForeignKey(dependentEntry, dependentProperties);
            }
        }

        private static void SetNullForeignKey(StateEntry dependentEntry, IReadOnlyList<IProperty> dependentProperties)
        {
            foreach (var dependentProperty in dependentProperties)
            {
                // TODO: Conceptual nulls
                // Issue #323
                dependentEntry[dependentProperty] = null;
                dependentEntry.RelationshipsSnapshot.TakeSnapshot(dependentProperty);
            }
        }

        private void SetInverse(StateEntry entry, INavigation navigation, object entity)
        {
            var inverse = navigation.TryGetInverse();

            if (inverse != null)
            {
                if (inverse.IsCollection())
                {
                    var collectionAccessor = _collectionAccessorSource.GetAccessor(inverse);

                    if (!collectionAccessor.Contains(entity, entry.Entity))
                    {
                        collectionAccessor.Add(entity, entry.Entity);
                    }
                }
                else
                {
                    var oldEntity = _getterSource.GetAccessor(inverse).GetClrValue(entity);
                    if (oldEntity != null && oldEntity != entry.Entity)
                    {
                        Unfixup(navigation, entry.StateManager.GetOrCreateEntry(oldEntity), entry.StateManager.GetOrCreateEntry(entity));
                    }

                    _setterSource.GetAccessor(inverse).SetClrValue(entity, entry.Entity);
                }

                entry.StateManager.GetOrCreateEntry(entity).RelationshipsSnapshot.TakeSnapshot(inverse);
            }
        }

        private void ConditionallyClearInverse(StateEntry entry, INavigation navigation, object entity)
        {
            var inverse = navigation.TryGetInverse();

            if (inverse != null)
            {
                if (inverse.IsCollection())
                {
                    _collectionAccessorSource.GetAccessor(inverse).Remove(entity, entry.Entity);
                }
                else
                {
                    if (ReferenceEquals(_getterSource.GetAccessor(inverse).GetClrValue(entity), entry.Entity))
                    {
                        _setterSource.GetAccessor(inverse).SetClrValue(entity, null);
                    }
                }

                entry.StateManager.GetOrCreateEntry(entity).RelationshipsSnapshot.TakeSnapshot(inverse);
            }
        }
    }
}
