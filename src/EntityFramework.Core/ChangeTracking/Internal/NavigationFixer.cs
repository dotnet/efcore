// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Update;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public class NavigationFixer : INavigationFixer
    {
        private readonly IModel _model;
        private bool _inFixup;

        public NavigationFixer([NotNull] IModel model)
        {
            _model = model;
        }

        public virtual void ForeignKeyPropertyChanged(InternalEntityEntry entry, IProperty property, object oldValue, object newValue)
            => PerformFixup(() => ForeignKeyPropertyChangedAction(entry, property));

        private void ForeignKeyPropertyChangedAction(InternalEntityEntry entry, IProperty property)
        {
            foreach (var foreignKey in entry.EntityType.GetForeignKeys().Where(p => p.Properties.Contains(property)).Distinct())
            {
                var navigations = foreignKey.GetNavigations().ToList();

                var oldPrincipalEntry = entry.StateManager.GetPrincipal(entry, foreignKey, ValueSource.RelationshipSnapshot);
                if (oldPrincipalEntry != null)
                {
                    Unfixup(navigations, oldPrincipalEntry, entry);
                }

                var principalEntry = entry.StateManager.GetPrincipal(entry, foreignKey, ValueSource.Current);
                if (principalEntry != null)
                {
                    if (foreignKey.IsUnique)
                    {
                        foreach (var oldDependentEntry in 
                            (entry.StateManager.GetDependentsFromNavigation(principalEntry, foreignKey)
                             ?? entry.StateManager.GetDependents(principalEntry, foreignKey))
                                .Where(e => e != entry)
                                .ToList())
                        {
                            StealReference(foreignKey, oldDependentEntry);
                        }
                    }

                    DoFixup(navigations, principalEntry, new[] { entry });
                }
            }
        }

        public virtual void NavigationReferenceChanged(InternalEntityEntry entry, INavigation navigation, object oldValue, object newValue)
            => PerformFixup(() => NavigationReferenceChangedAction(entry, navigation, oldValue, newValue));

        private void NavigationReferenceChangedAction(InternalEntityEntry entry, INavigation navigation, object oldValue, object newValue)
        {
            var foreignKey = navigation.ForeignKey;
            var dependentProperties = foreignKey.Properties;
            var principalProperties = foreignKey.PrincipalKey.Properties;

            if (navigation.IsDependentToPrincipal())
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
                    var dependentEntry = entry.StateManager.GetOrCreateEntry(newValue);

                    // Avoid eagerly setting FKs (which may be PKs) in un-tracked entities so as not to mess up
                    // Attach behavior that is based on key values.
                    if (dependentEntry.EntityState != EntityState.Detached)
                    {
                        SetForeignKeyValue(foreignKey, dependentEntry, entry);
                    }
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

        public virtual void NavigationCollectionChanged(InternalEntityEntry entry, INavigation navigation, ISet<object> added, ISet<object> removed)
            => PerformFixup(() => NavigationCollectionChangedAction(entry, navigation, added, removed));

        private void NavigationCollectionChangedAction(
            InternalEntityEntry entry, INavigation navigation, IEnumerable<object> added, IEnumerable<object> removed)
        {
            var principalProperties = navigation.ForeignKey.PrincipalKey.Properties;
            var dependentProperties = navigation.ForeignKey.Properties;
            var principalValues = principalProperties.Select(p => entry[p]).ToList();

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

        public virtual void KeyPropertyChanged(InternalEntityEntry entry, IProperty property, object oldValue, object newValue)
        {
            // We don't prevent recursive entry here because changes of principal key can have cascading effects
            // when principal key is also foreign key.

            if (entry.EntityState == EntityState.Detached)
            {
                return;
            }

            foreach (var foreignKey in _model.GetEntityTypes().SelectMany(
                e => e.GetForeignKeys().Where(f => f.PrincipalKey.Properties.Contains(property))))
            {
                var newKeyValues = foreignKey.PrincipalKey.Properties.Select(p => entry[p]).ToList();

                var oldKey = entry.GetPrincipalKeyValue(foreignKey, ValueSource.RelationshipSnapshot);
                if (!oldKey.IsInvalid)
                {
                    foreach (var dependent in entry.StateManager.Entries.Where(
                        e => foreignKey.DeclaringEntityType.IsAssignableFrom(e.EntityType)
                             && oldKey.Equals(e.GetDependentKeyValue(foreignKey))).ToList())
                    {
                        SetForeignKeyValue(foreignKey, dependent, newKeyValues);
                    }
                }
            }
        }

        public virtual void StateChanging(InternalEntityEntry entry, EntityState newState)
        {
        }

        public virtual void StateChanged(InternalEntityEntry entry, EntityState oldState, bool skipInitialFixup)
        {
            if ((oldState == EntityState.Detached)
                && !skipInitialFixup)
            {
                PerformFixup(() => InitialFixup(entry));
            }

            else if ((entry.EntityState == EntityState.Detached)
                     && (oldState == EntityState.Deleted))
            {
                PerformFixup(() => DeleteFixup(entry));
            }
        }

        private void DeleteFixup(InternalEntityEntry entry)
        {
            var entityType = entry.EntityType;
            var entries = entry.StateManager.Entries.ToList();

            // TODO: Perf on this state manager query
            foreach (var navigation in _model.GetEntityTypes()
                .SelectMany(e => e.GetNavigations())
                .Where(n => n.GetTargetType().IsAssignableFrom(entityType)))
            {
                var collectionAccessor = navigation.IsCollection()
                    ? navigation.GetCollectionAccessor()
                    : null;

                var navigationEntityType = navigation.DeclaringEntityType;

                foreach (var relatedEntry in entries)
                {
                    if (!navigationEntityType.IsAssignableFrom(relatedEntry.EntityType))
                    {
                        continue;
                    }

                    if (collectionAccessor != null)
                    {
                        collectionAccessor.Remove(relatedEntry.Entity, entry.Entity);
                    }
                    else if (relatedEntry[navigation] == entry.Entity)
                    {
                        relatedEntry[navigation] = null;
                    }
                }
            }
        }

        private void InitialFixup(InternalEntityEntry entry)
        {
            var entityType = entry.EntityType;

            // If the new state is unchanged (such as from a query or Attach) then we are going
            // to assume that the FK value is the source of truth and not attempt to ascertain
            // relationships from navigation properties
            if (entry.EntityState != EntityState.Unchanged)
            {
                foreach (var navigation in entityType.GetNavigations())
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

                var entries = entry.StateManager.Entries.ToList();

                // TODO: Perf on this state manager query
                foreach (var navigation in _model.GetEntityTypes()
                    .SelectMany(e => e.GetNavigations())
                    .Where(n => n.GetTargetType().IsAssignableFrom(entityType)))
                {
                    var collectionAccessor = navigation.IsCollection()
                        ? navigation.GetCollectionAccessor()
                        : null;

                    var navigationEntityType = navigation.DeclaringEntityType;

                    foreach (var relatedEntry in entries)
                    {
                        if (!navigationEntityType.IsAssignableFrom(relatedEntry.EntityType)
                            || (relatedEntry == entry))
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
            }

            foreach (var foreignKey in entityType.GetForeignKeys())
            {
                var principalEntry = entry.StateManager.GetPrincipal(entry, foreignKey, ValueSource.RelationshipSnapshot);
                if (principalEntry != null)
                {
                    DoFixup(foreignKey, principalEntry, new[] { entry });
                }
            }

            foreach (var foreignKey in entityType.GetReferencingForeignKeys())
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

        private void DoFixup(IForeignKey foreignKey, InternalEntityEntry principalEntry, InternalEntityEntry[] dependentEntries)
            => DoFixup(foreignKey.GetNavigations().ToList(), principalEntry, dependentEntries);

        private static void DoFixup(IEnumerable<INavigation> navigations, InternalEntityEntry principalEntry, InternalEntityEntry[] dependentEntries)
        {
            foreach (var navigation in navigations)
            {
                if (navigation.IsDependentToPrincipal())
                {
                    var setter = navigation.GetSetter();

                    foreach (var dependent in dependentEntries)
                    {
                        setter.SetClrValue(dependent.Entity, principalEntry.Entity);
                        dependent.SetValue(navigation, principalEntry.Entity, ValueSource.RelationshipSnapshot);
                    }
                }
                else
                {
                    if (navigation.IsCollection())
                    {
                        var collectionAccessor = navigation.GetCollectionAccessor();

                        foreach (var dependent in dependentEntries)
                        {
                            var dependentEntity = dependent.Entity;
                            if (!collectionAccessor.Contains(principalEntry.Entity, dependentEntity))
                            {
                                collectionAccessor.Add(principalEntry.Entity, dependentEntity);
                                principalEntry.AddToCollectionSnapshot(navigation, dependentEntity);
                            }
                        }
                    }
                    else
                    {
                        // TODO: Decide how to handle case where multiple values match non-collection nav prop
                        // Issue #739
                        var value = dependentEntries.Single().Entity;
                        navigation.GetSetter().SetClrValue(principalEntry.Entity, value);
                        principalEntry.SetValue(navigation, value, ValueSource.RelationshipSnapshot);
                    }
                }
            }
        }

        private void Unfixup(IEnumerable<INavigation> navigations, InternalEntityEntry oldPrincipalEntry, InternalEntityEntry dependentEntry)
        {
            foreach (var navigation in navigations)
            {
                Unfixup(navigation, oldPrincipalEntry, dependentEntry);
            }
        }

        private static void Unfixup(INavigation navigation, InternalEntityEntry oldPrincipalEntry, InternalEntityEntry dependentEntry)
        {
            var dependentEntity = dependentEntry.Entity;

            if (navigation.IsDependentToPrincipal())
            {
                navigation.GetSetter().SetClrValue(dependentEntity, null);

                dependentEntry.SetValue(navigation, null, ValueSource.RelationshipSnapshot);
            }
            else
            {
                if (navigation.IsCollection())
                {
                    var collectionAccessor = navigation.GetCollectionAccessor();
                    if (collectionAccessor.Contains(oldPrincipalEntry.Entity, dependentEntity))
                    {
                        collectionAccessor.Remove(oldPrincipalEntry.Entity, dependentEntity);
                        oldPrincipalEntry.RemoveFromCollectionSnapshot(navigation, dependentEntity);
                    }
                }
                else
                {
                    navigation.GetSetter().SetClrValue(oldPrincipalEntry.Entity, null);
                    oldPrincipalEntry.SetValue(navigation, null, ValueSource.RelationshipSnapshot);
                }
            }
        }

        private static void StealReference(IForeignKey foreignKey, InternalEntityEntry dependentEntry)
        {
            var navigation = foreignKey.DependentToPrincipal;
            if (navigation != null)
            {
                navigation.GetSetter().SetClrValue(dependentEntry.Entity, null);
                dependentEntry.SetValue(navigation, null, ValueSource.RelationshipSnapshot);
            }

            foreach (var property in foreignKey.Properties.Where(p => p.IsNullable).ToList())
            {
                dependentEntry[property] = null;
            }
        }

        private static void SetForeignKeyValue(
            IForeignKey foreignKey,
            InternalEntityEntry dependentEntry,
            InternalEntityEntry principalEntry)
            => SetForeignKeyValue(foreignKey, dependentEntry, foreignKey.PrincipalKey.Properties.Select(p => principalEntry[p]).ToList());

        private static void SetForeignKeyValue(IForeignKey foreignKey, InternalEntityEntry dependentEntry, IReadOnlyList<object> principalValues)
        {
            for (var i = 0; i < foreignKey.Properties.Count; i++)
            {
                var principalValue = principalValues[i];
                if ((foreignKey.Properties[i].GetGenerationProperty() == null)
                    || !foreignKey.PrincipalKey.Properties[i].ClrType.IsDefaultValue(principalValue))
                {
                    var dependentProperty = foreignKey.Properties[i];
                    dependentEntry[dependentProperty] = principalValue;
                    dependentEntry.SetValue(dependentProperty, principalValue, ValueSource.RelationshipSnapshot);
                }
            }
        }

        private static void ConditionallySetNullForeignKey(
            InternalEntityEntry dependentEntry, IReadOnlyList<IProperty> dependentProperties,
            InternalEntityEntry principalEntry, IReadOnlyList<IProperty> principalProperties)
            => ConditionallySetNullForeignKey(dependentEntry, dependentProperties, principalProperties.Select(p => principalEntry[p]).ToList());

        private static void ConditionallySetNullForeignKey(
            InternalEntityEntry dependentEntry, IReadOnlyList<IProperty> dependentProperties, IReadOnlyList<object> principalValues)
        {
            // Don't null out the FK if it has already be set to point to a different principal
            if (dependentProperties.Select(p => dependentEntry[p]).StructuralSequenceEqual(principalValues))
            {
                SetNullForeignKey(dependentEntry, dependentProperties);
            }
        }

        private static void SetNullForeignKey(InternalEntityEntry dependentEntry, IReadOnlyList<IProperty> dependentProperties)
        {
            foreach (var dependentProperty in dependentProperties)
            {
                dependentEntry[dependentProperty] = null;
                dependentEntry.SetValue(dependentProperty, null, ValueSource.RelationshipSnapshot);
            }
        }

        private void SetInverse(InternalEntityEntry entry, INavigation navigation, object entity)
        {
            var inverse = navigation.FindInverse();

            if (inverse != null)
            {
                var inverseEntry = entry.StateManager.GetOrCreateEntry(entity);

                if (inverse.IsCollection())
                {
                    var collectionAccessor = inverse.GetCollectionAccessor();

                    if (!collectionAccessor.Contains(entity, entry.Entity))
                    {
                        collectionAccessor.Add(entity, entry.Entity);
                        inverseEntry.AddToCollectionSnapshot(inverse, entry.Entity);
                    }
                }
                else
                {
                    var oldEntity = inverse.GetGetter().GetClrValue(entity);
                    if ((oldEntity != null)
                        && (oldEntity != entry.Entity))
                    {
                        var oldEntry = entry.StateManager.GetOrCreateEntry(oldEntity);
                        if (navigation.IsDependentToPrincipal())
                        {
                            Unfixup(navigation, inverseEntry, oldEntry);
                            SetNullForeignKey(oldEntry, navigation.ForeignKey.Properties);
                        }
                        else
                        {
                            Unfixup(navigation, oldEntry, inverseEntry);
                        }
                    }

                    inverse.GetSetter().SetClrValue(entity, entry.Entity);
                    inverseEntry.SetValue(inverse, entry.Entity, ValueSource.RelationshipSnapshot);
                }
            }
        }

        private static void ConditionallyClearInverse(InternalEntityEntry entry, INavigation navigation, object entity)
        {
            var inverse = navigation.FindInverse();

            if (inverse != null)
            {
                if (inverse.IsCollection())
                {
                    inverse.GetCollectionAccessor().Remove(entity, entry.Entity);
                    entry.StateManager.GetOrCreateEntry(entity).RemoveFromCollectionSnapshot(inverse, entry.Entity);
                }
                else
                {
                    if (ReferenceEquals(inverse.GetGetter().GetClrValue(entity), entry.Entity))
                    {
                        inverse.GetSetter().SetClrValue(entity, null);
                        entry.StateManager.GetOrCreateEntry(entity).SetValue(inverse, null, ValueSource.RelationshipSnapshot);
                    }
                }
            }
        }
    }
}
