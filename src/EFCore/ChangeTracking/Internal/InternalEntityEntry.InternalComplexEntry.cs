// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

public sealed partial class InternalEntityEntry
{
    private sealed class InternalComplexEntry : IInternalEntry
    {
        private readonly StateData _stateData;
        private OriginalValues _originalValues;
        private SidecarValues _temporaryValues;
        private SidecarValues _storeGeneratedValues;
        private object? _complexObject;
        private readonly ISnapshot _shadowValues;
        private readonly ComplexEntries _complexEntries;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public InternalComplexEntry(
            IStateManager stateManager,
            IComplexType complexType,
            IInternalEntry containingEntry,
            object? complexObject) // This works only for non-value types
        {
            Check.DebugAssert(complexObject == null || complexType.ClrType.IsAssignableFrom(complexObject.GetType()),
                $"Expected {complexType.ClrType}, got {complexObject?.GetType()}");
            StateManager = stateManager;
            ComplexType = (IRuntimeComplexType)complexType;
            ContainingEntry = containingEntry;
            ComplexObject = complexObject;
            _shadowValues = ComplexType.EmptyShadowValuesFactory();
            _stateData = new StateData(ComplexType.PropertyCount, ComplexType.NavigationCount);
            _complexEntries = new ComplexEntries(this);

            foreach (var property in complexType.GetProperties())
            {
                if (property.IsShadowProperty())
                {
                    _stateData.FlagProperty(property.GetIndex(), PropertyFlag.Unknown, true);
                }
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public InternalComplexEntry(
            IStateManager stateManager,
            IComplexType complexType,
            IInternalEntry containingEntry,
            object? complexObject,
            in ValueBuffer valueBuffer)
        {
            Check.DebugAssert(complexObject == null || complexType.ClrType.IsAssignableFrom(complexObject.GetType()),
                $"Expected {complexType.ClrType}, got {complexObject?.GetType()}");
            StateManager = stateManager;
            ComplexType = (IRuntimeComplexType)complexType;
            ContainingEntry = containingEntry;
            ComplexObject = complexObject;
            _shadowValues = ComplexType.ShadowValuesFactory(valueBuffer);
            _stateData = new StateData(ComplexType.PropertyCount, ComplexType.NavigationCount);
            _complexEntries = new ComplexEntries(this);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public IInternalEntry ContainingEntry { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public object? ComplexObject
        {
            get => _complexObject;
            set
            {
                Check.DebugAssert(value == null || ComplexType.ClrType.IsAssignableFrom(value.GetType()),
                    $"Expected {ComplexType.ClrType}, got {value?.GetType()}");
                _complexObject = value;
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public IRuntimeComplexType ComplexType { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public IStateManager StateManager { [DebuggerStepThrough] get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public void SetEntityState(
            EntityState entityState,
            bool acceptChanges = false,
            bool modifyProperties = true)
        {
            var oldState = _stateData.EntityState;
            PrepareForAdd(entityState);

            SetEntityState(oldState, entityState, acceptChanges, modifyProperties);
        }

        private bool PrepareForAdd(EntityState newState)
        {
            if (newState != EntityState.Added
                || EntityState == EntityState.Added)
            {
                return false;
            }

            if (EntityState == EntityState.Modified)
            {
                _stateData.FlagAllProperties(
                    ComplexType.PropertyCount, PropertyFlag.Modified,
                    flagged: false);
            }

            return true;
        }

        private void SetEntityState(EntityState oldState, EntityState newState, bool acceptChanges, bool modifyProperties)
        {
            var complexType = ComplexType;

            // Prevent temp values from becoming permanent values
            if (oldState == EntityState.Added
                && newState != EntityState.Added
                && newState != EntityState.Detached)
            {
                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (var property in complexType.GetProperties())
                {
                    if (property.IsKey() && HasTemporaryValue(property))
                    {
                        throw new InvalidOperationException(
                            CoreStrings.TempValuePersists(
                                property.Name,
                                complexType.DisplayName(), newState));
                    }
                }
            }

            // The entity state can be Modified even if some properties are not modified so always
            // set all properties to modified if the entity state is explicitly set to Modified.
            if (newState == EntityState.Modified
                && modifyProperties)
            {
                _stateData.FlagAllProperties(ComplexType.PropertyCount, PropertyFlag.Modified, flagged: true);

                // Hot path; do not use LINQ
                foreach (var property in complexType.GetProperties())
                {
                    if (property.GetAfterSaveBehavior() != PropertySaveBehavior.Save)
                    {
                        _stateData.FlagProperty(property.GetIndex(), PropertyFlag.Modified, isFlagged: false);
                    }
                }

                foreach (var complexEntry in _complexEntries)
                {
                    complexEntry.SetEntityState(EntityState.Modified, acceptChanges, modifyProperties);
                }
            }

            if (oldState == newState)
            {
                return;
            }

            if (newState == EntityState.Unchanged)
            {
                _stateData.FlagAllProperties(
                    ComplexType.PropertyCount, PropertyFlag.Modified,
                    flagged: false);

                foreach (var complexEntry in _complexEntries)
                {
                    complexEntry.SetEntityState(EntityState.Unchanged, acceptChanges, modifyProperties);
                }
            }

            if (_stateData.EntityState != oldState)
            {
                _stateData.EntityState = oldState;
            }

            if (newState == EntityState.Unchanged
                && oldState == EntityState.Modified)
            {
                if (acceptChanges)
                {
                    _originalValues.AcceptChanges(this);
                }
                else
                {
                    _originalValues.RejectChanges(this);
                }
            }

            _stateData.EntityState = newState;

            if (newState is EntityState.Deleted or EntityState.Detached
                && HasConceptualNull)
            {
                _stateData.FlagAllProperties(ComplexType.PropertyCount, PropertyFlag.Null, flagged: false);
            }

            if (oldState is EntityState.Detached or EntityState.Unchanged)
            {
                if (newState is EntityState.Added or EntityState.Deleted or EntityState.Modified)
                {
                    ContainingEntry.OnComplexPropertyModified(ComplexType.ComplexProperty, isModified: true);
                }
            }
            else if (newState is EntityState.Detached or EntityState.Unchanged)
            {
                ContainingEntry.OnComplexPropertyModified(ComplexType.ComplexProperty, isModified: false);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public void MarkUnchangedFromQuery()
            => _stateData.EntityState = EntityState.Unchanged;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public EntityState EntityState
            => _stateData.EntityState;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public bool IsModified(IProperty property)
        {
            var propertyIndex = property.GetIndex();

            return _stateData.EntityState == EntityState.Modified
                && _stateData.IsPropertyFlagged(propertyIndex, PropertyFlag.Modified)
                && !_stateData.IsPropertyFlagged(propertyIndex, PropertyFlag.Unknown);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public bool IsUnknown(IProperty property)
            => _stateData.IsPropertyFlagged(property.GetIndex(), PropertyFlag.Unknown);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public void SetPropertyModified(
            IProperty property,
            bool changeState = true,
            bool isModified = true,
            bool isConceptualNull = false,
            bool acceptChanges = false)
        {
            var propertyIndex = property.GetIndex();
            _stateData.FlagProperty(propertyIndex, PropertyFlag.Unknown, false);

            var currentState = _stateData.EntityState;

            if (currentState is EntityState.Added or EntityState.Detached
                || !changeState)
            {
                var index = property.GetOriginalValueIndex();
                if (index != -1 && !IsConceptualNull(property))
                {
                    SetOriginalValue(property, this[property], index);
                }

                if (currentState == EntityState.Added)
                {
                    if (FlaggedAsTemporary(propertyIndex)
                        && !FlaggedAsStoreGenerated(propertyIndex)
                        && !HasSentinelValue(property))
                    {
                        _stateData.FlagProperty(propertyIndex, PropertyFlag.IsTemporary, false);
                    }

                    return;
                }
            }

            if (changeState
                && !isConceptualNull
                && isModified
                && !StateManager.SavingChanges
                && property.IsKey()
                && property.GetAfterSaveBehavior() == PropertySaveBehavior.Throw)
            {
                throw new InvalidOperationException(CoreStrings.KeyReadOnly(property.Name, ComplexType.DisplayName()));
            }

            if (currentState == EntityState.Deleted)
            {
                return;
            }

            if (changeState)
            {
                if (!isModified
                    && currentState != EntityState.Detached
                    && property.GetOriginalValueIndex() != -1)
                {
                    if (acceptChanges)
                    {
                        SetOriginalValue(property, GetCurrentValue(property));
                    }

                    SetProperty(property, GetOriginalValue(property), isMaterialization: false, setModified: false);
                }

                _stateData.FlagProperty(propertyIndex, PropertyFlag.Modified, isModified);
            }

            if (isModified
                && currentState is EntityState.Unchanged or EntityState.Detached)
            {
                if (changeState)
                {
                    _stateData.EntityState = EntityState.Modified;
                    ContainingEntry.OnComplexPropertyModified(ComplexType.ComplexProperty, isModified);
                }
            }
            else if (currentState == EntityState.Modified
                     && changeState
                     && !isModified
                     && !_stateData.AnyPropertiesFlagged(PropertyFlag.Modified))
            {
                _stateData.EntityState = EntityState.Unchanged;
                ContainingEntry.OnComplexPropertyModified(ComplexType.ComplexProperty, isModified);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public void OnComplexPropertyModified(IComplexProperty property, bool isModified = true)
        {
            var currentState = _stateData.EntityState;
            if (currentState == EntityState.Deleted)
            {
                return;
            }

            if (isModified
                && currentState is EntityState.Unchanged or EntityState.Detached)
            {
                _stateData.EntityState = EntityState.Modified;
            }
            else if (currentState == EntityState.Modified
                     && !isModified
                     && !_stateData.AnyPropertiesFlagged(PropertyFlag.Modified)
                     && _complexEntries.All(e => e.EntityState == EntityState.Unchanged))
            {
                _stateData.EntityState = EntityState.Unchanged;
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public bool HasConceptualNull
            => _stateData.AnyPropertiesFlagged(PropertyFlag.Null);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public bool IsConceptualNull(IProperty property)
            => _stateData.IsPropertyFlagged(property.GetIndex(), PropertyFlag.Null);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public bool HasTemporaryValue(IProperty property)
            => GetValueType(property) == CurrentValueType.Temporary;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public void PropagateValue(
            InternalEntityEntry principalEntry,
            IProperty principalProperty,
            IProperty dependentProperty,
            bool isMaterialization = false,
            bool setModified = true)
        {
            var principalValue = principalEntry[principalProperty];
            if (principalEntry.HasTemporaryValue(principalProperty))
            {
                SetTemporaryValue(dependentProperty, principalValue);
            }
            else if (principalEntry.GetValueType(principalProperty) == CurrentValueType.StoreGenerated)
            {
                SetStoreGeneratedValue(dependentProperty, principalValue);
            }
            else
            {
                SetProperty(dependentProperty, principalValue, isMaterialization, setModified);
            }
        }

        private CurrentValueType GetValueType(IProperty property)
            => _stateData.IsPropertyFlagged(property.GetIndex(), PropertyFlag.IsStoreGenerated)
                ? CurrentValueType.StoreGenerated
                : _stateData.IsPropertyFlagged(property.GetIndex(), PropertyFlag.IsTemporary)
                    ? CurrentValueType.Temporary
                    : CurrentValueType.Normal;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public void SetTemporaryValue(IProperty property, object? value, bool setModified = true)
        {
            if (property.GetStoreGeneratedIndex() == -1)
            {
                throw new InvalidOperationException(
                    CoreStrings.TempValue(property.Name, ComplexType.DisplayName()));
            }

            SetProperty(property, value, isMaterialization: false, setModified, isCascadeDelete: false, CurrentValueType.Temporary);
            _stateData.FlagProperty(property.GetIndex(), PropertyFlag.IsTemporary, true);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public void MarkAsTemporary(IProperty property, bool temporary)
            => _stateData.FlagProperty(property.GetIndex(), PropertyFlag.IsTemporary, temporary);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public void SetStoreGeneratedValue(IProperty property, object? value, bool setModified = true)
        {
            if (property.GetStoreGeneratedIndex() == -1)
            {
                throw new InvalidOperationException(
                    CoreStrings.StoreGenValue(property.Name, ComplexType.DisplayName()));
            }

            SetProperty(
                property,
                value,
                isMaterialization: false,
                setModified,
                isCascadeDelete: false,
                CurrentValueType.StoreGenerated);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public void MarkUnknown(IProperty property)
            => _stateData.FlagProperty(property.GetIndex(), PropertyFlag.Unknown, true);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public T ReadShadowValue<T>(int shadowIndex)
            => _shadowValues.GetValue<T>(shadowIndex);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public T ReadOriginalValue<T>(IProperty property, int originalValueIndex)
            => _originalValues.GetValue<T>(this, property, originalValueIndex);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public T ReadStoreGeneratedValue<T>(int storeGeneratedIndex)
            => _storeGeneratedValues.GetValue<T>(storeGeneratedIndex);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public T ReadTemporaryValue<T>(int storeGeneratedIndex)
            => _temporaryValues.GetValue<T>(storeGeneratedIndex);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public TProperty GetCurrentValue<TProperty>(IPropertyBase propertyBase)
            => ((Func<IInternalEntry, TProperty>)propertyBase.GetPropertyAccessors().CurrentValueGetter)(this);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public TProperty GetOriginalValue<TProperty>(IProperty property)
            => ((Func<IInternalEntry, TProperty>)property.GetPropertyAccessors().OriginalValueGetter!)(this);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public object? ReadPropertyValue(IPropertyBase propertyBase)
        {
            Check.DebugAssert(ComplexObject != null || ComplexType.ComplexProperty.IsNullable,
                $"Unexpected null for {ComplexType.DisplayName()}");
            return ComplexObject == null
                        ? null
                        : propertyBase.IsShadowProperty()
                            ? _shadowValues[propertyBase.GetShadowIndex()]
                            : propertyBase.GetGetter().GetClrValue(ComplexObject);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        private void WritePropertyValue(
            IPropertyBase propertyBase,
            object? value,
            bool forMaterialization)
        {
            Check.DebugAssert(ComplexObject != null, "null object for " + ComplexType.DisplayName());
            if (propertyBase.IsShadowProperty())
            {
                _shadowValues[propertyBase.GetShadowIndex()] = value;
            }
            else
            {
                var concretePropertyBase = (IRuntimePropertyBase)propertyBase;

                var setter = forMaterialization
                    ? concretePropertyBase.MaterializationSetter
                    : concretePropertyBase.GetSetter();

                setter.SetClrValue(ComplexObject, value);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public object? GetCurrentValue(IPropertyBase propertyBase)
            => propertyBase is not IProperty property || !IsConceptualNull(property)
                ? this[propertyBase]
                : null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public object? GetPreStoreGeneratedCurrentValue(IPropertyBase propertyBase)
            => propertyBase is not IProperty property || !IsConceptualNull(property)
                ? ReadPropertyValue(propertyBase)
                : null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public object? GetOriginalValue(IPropertyBase propertyBase)
        {
            Check.DebugAssert(ComplexObject != null || ComplexType.ComplexProperty.IsNullable,
                $"Unexpected null for {ComplexType.DisplayName()}");
            return _originalValues.GetValue(this, (IProperty)propertyBase);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public void SetOriginalValue(
            IPropertyBase propertyBase,
            object? value,
            int index = -1)
        {
            Check.DebugAssert(ComplexObject != null, "null object for " + ComplexType.DisplayName());
            EnsureOriginalValues();

            var property = (IProperty)propertyBase;

            _originalValues.SetValue(property, value, index);

            // If setting the original value results in the current value being different from the
            // original value, then mark the property as modified.
            if ((EntityState == EntityState.Unchanged
                    || (EntityState == EntityState.Modified && !IsModified(property)))
                && !_stateData.IsPropertyFlagged(property.GetIndex(), PropertyFlag.Unknown))
            {
                //((StateManager as StateManager)?.ChangeDetector as ChangeDetector)?.DetectValueChange(this, property);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public void EnsureOriginalValues()
        {
            if (_originalValues.IsEmpty)
            {
                _originalValues = new OriginalValues(this);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public void EnsureTemporaryValues()
        {
            if (_temporaryValues.IsEmpty)
            {
                _temporaryValues = new SidecarValues(ComplexType.TemporaryValuesFactory(this));
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public void EnsureStoreGeneratedValues()
        {
            if (_storeGeneratedValues.IsEmpty)
            {
                _storeGeneratedValues = new SidecarValues(ComplexType.StoreGeneratedValuesFactory());
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public bool HasOriginalValuesSnapshot
            => !_originalValues.IsEmpty;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public IInternalEntry GetComplexPropertyEntry(IComplexProperty property)
            => _complexEntries.GetEntry(this, property);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public object? this[IPropertyBase propertyBase]
        {
            get
            {
                var storeGeneratedIndex = propertyBase.GetStoreGeneratedIndex();
                if (storeGeneratedIndex != -1)
                {
                    var property = (IProperty)propertyBase;
                    var propertyIndex = property.GetIndex();

                    if (FlaggedAsStoreGenerated(propertyIndex))
                    {
                        return _storeGeneratedValues.GetValue(storeGeneratedIndex);
                    }

                    if (FlaggedAsTemporary(propertyIndex)
                        && HasSentinelValue(property))
                    {
                        return _temporaryValues.GetValue(storeGeneratedIndex);
                    }
                }

                return ReadPropertyValue(propertyBase);
            }

            set => SetProperty(propertyBase, value, isMaterialization: false);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public bool FlaggedAsStoreGenerated(int propertyIndex)
            => !_storeGeneratedValues.IsEmpty
                && _stateData.IsPropertyFlagged(propertyIndex, PropertyFlag.IsStoreGenerated);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public bool FlaggedAsTemporary(int propertyIndex)
            => !_temporaryValues.IsEmpty
                && _stateData.IsPropertyFlagged(propertyIndex, PropertyFlag.IsTemporary);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public void SetProperty(
            IPropertyBase propertyBase,
            object? value,
            bool isMaterialization,
            bool setModified = true,
            bool isCascadeDelete = false)
            => SetProperty(propertyBase, value, isMaterialization, setModified, isCascadeDelete, CurrentValueType.Normal);

        private void SetProperty(
            IPropertyBase propertyBase,
            object? value,
            bool isMaterialization,
            bool setModified,
            bool isCascadeDelete,
            CurrentValueType valueType)
        {
            Check.DebugAssert(ComplexObject != null, "null object for " + ComplexType.DisplayName());
            var currentValue = ReadPropertyValue(propertyBase);

            var asProperty = propertyBase as IProperty;
            int propertyIndex;
            CurrentValueType currentValueType;
            int storeGeneratedIndex;
            bool valuesEqual;

            if (asProperty != null)
            {
                propertyIndex = asProperty.GetIndex();
                valuesEqual = AreEqual(currentValue, value, asProperty);
                currentValueType = GetValueType(asProperty);
                storeGeneratedIndex = asProperty.GetStoreGeneratedIndex();
            }
            else
            {
                propertyIndex = -1;
                valuesEqual = ReferenceEquals(currentValue, value);
                currentValueType = CurrentValueType.Normal;
                storeGeneratedIndex = -1;
            }

            if (!valuesEqual
                || (propertyIndex != -1
                    && (_stateData.IsPropertyFlagged(propertyIndex, PropertyFlag.Unknown)
                        || _stateData.IsPropertyFlagged(propertyIndex, PropertyFlag.Null)
                        || valueType != currentValueType)))
            {
                var writeValue = true;

                if (asProperty != null
                    && valueType == CurrentValueType.Normal
                    && (!asProperty.ClrType.IsNullableType()
                        || asProperty.GetContainingForeignKeys().Any(
                            fk => fk is { IsRequired: true, DeleteBehavior: DeleteBehavior.Cascade or DeleteBehavior.ClientCascade }
                                && fk.DeclaringEntityType.IsAssignableFrom(ComplexType))))
                {
                    if (value == null)
                    {
                        HandleNullForeignKey(asProperty, setModified, isCascadeDelete);
                        writeValue = false;
                    }
                    else
                    {
                        _stateData.FlagProperty(propertyIndex, PropertyFlag.Null, isFlagged: false);
                    }
                }

                if (writeValue)
                {
                    //StateManager.InternalEntityEntryNotifier.PropertyChanging(this, propertyBase);

                    if (storeGeneratedIndex == -1)
                    {
                        WritePropertyValue(propertyBase, value, isMaterialization);
                    }
                    else
                    {
                        switch (valueType)
                        {
                            case CurrentValueType.Normal:
                                WritePropertyValue(propertyBase, value, isMaterialization);
                                _stateData.FlagProperty(propertyIndex, PropertyFlag.IsTemporary, isFlagged: false);
                                _stateData.FlagProperty(propertyIndex, PropertyFlag.IsStoreGenerated, isFlagged: false);
                                break;
                            case CurrentValueType.StoreGenerated:
                                EnsureStoreGeneratedValues();
                                _storeGeneratedValues.SetValue(asProperty!, value, storeGeneratedIndex);
                                _stateData.FlagProperty(propertyIndex, PropertyFlag.IsStoreGenerated, isFlagged: true);
                                break;
                            case CurrentValueType.Temporary:
                                EnsureTemporaryValues();
                                _temporaryValues.SetValue(asProperty!, value, storeGeneratedIndex);
                                _stateData.FlagProperty(propertyIndex, PropertyFlag.IsTemporary, isFlagged: true);
                                _stateData.FlagProperty(propertyIndex, PropertyFlag.IsStoreGenerated, isFlagged: false);
                                if (!HasSentinelValue(asProperty!))
                                {
                                    WritePropertyValue(propertyBase, value, isMaterialization);
                                }

                                break;
                            default:
                                Check.DebugFail($"Bad value type {valueType}");
                                break;
                        }
                    }

                    if (propertyIndex != -1)
                    {
                        if (_stateData.IsPropertyFlagged(propertyIndex, PropertyFlag.Unknown))
                        {
                            if (!_originalValues.IsEmpty)
                            {
                                SetOriginalValue(propertyBase, value);
                            }

                            _stateData.FlagProperty(propertyIndex, PropertyFlag.Unknown, isFlagged: false);
                        }
                    }

                    if (propertyBase is IComplexProperty complexProperty)
                    {
                        _complexEntries.SetValue(value, this, complexProperty);
                    }

                    //StateManager.InternalEntityEntryNotifier.PropertyChanged(this, propertyBase, setModified);
                }
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public void HandleNullForeignKey(
            IProperty property,
            bool setModified = false,
            bool isCascadeDelete = false)
        {
            if (EntityState != EntityState.Deleted
                && EntityState != EntityState.Detached)
            {
                _stateData.FlagProperty(property.GetIndex(), PropertyFlag.Null, isFlagged: true);

                if (setModified)
                {
                    SetPropertyModified(
                        property, changeState: true, isModified: true,
                        isConceptualNull: true);
                }

                if (!isCascadeDelete
                    && StateManager.DeleteOrphansTiming == CascadeTiming.Immediate)
                {
                    ContainingEntry.HandleConceptualNulls(
                        StateManager.SensitiveLoggingEnabled,
                        force: false,
                        isCascadeDelete: false);
                }
            }
        }

        private static bool AreEqual(object? value, object? otherValue, IProperty property)
            => property.GetValueComparer().Equals(value, otherValue);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public void AcceptChanges()
        {
            if (!_storeGeneratedValues.IsEmpty)
            {
                foreach (var property in ComplexType.GetProperties())
                {
                    var storeGeneratedIndex = property.GetStoreGeneratedIndex();
                    if (storeGeneratedIndex != -1
                        && _stateData.IsPropertyFlagged(property.GetIndex(), PropertyFlag.IsStoreGenerated)
                        && _storeGeneratedValues.TryGetValue(storeGeneratedIndex, out var value))
                    {
                        this[property] = value;
                    }
                }

                _storeGeneratedValues = new SidecarValues();
                _temporaryValues = new SidecarValues();
            }

            _stateData.FlagAllProperties(ComplexType.PropertyCount, PropertyFlag.IsStoreGenerated, false);
            _stateData.FlagAllProperties(ComplexType.PropertyCount, PropertyFlag.IsTemporary, false);
            _stateData.FlagAllProperties(ComplexType.PropertyCount, PropertyFlag.Unknown, false);

            foreach (var complexEntry in _complexEntries)
            {
                complexEntry.AcceptChanges();
            }

            var currentState = EntityState;
            switch (currentState)
            {
                case EntityState.Unchanged:
                case EntityState.Detached:
                    return;
                case EntityState.Added:
                case EntityState.Modified:
                    _originalValues.AcceptChanges(this);

                    SetEntityState(EntityState.Unchanged, true);
                    break;
                case EntityState.Deleted:
                    SetEntityState(EntityState.Detached);
                    break;
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public IInternalEntry PrepareToSave()
        {
            var entityType = ComplexType;

            if (EntityState == EntityState.Added)
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.GetBeforeSaveBehavior() == PropertySaveBehavior.Throw
                        && !HasTemporaryValue(property)
                        && HasExplicitValue(property))
                    {
                        throw new InvalidOperationException(
                            CoreStrings.PropertyReadOnlyBeforeSave(
                                property.Name,
                                ComplexType.DisplayName()));
                    }

                    if (property.IsKey()
                        && property.IsForeignKey()
                        && _stateData.IsPropertyFlagged(property.GetIndex(), PropertyFlag.Unknown)
                        && !IsStoreGenerated(property))
                    {
                        if (property.GetContainingForeignKeys().Any(fk => fk.IsOwnership))
                        {
                            throw new InvalidOperationException(CoreStrings.SaveOwnedWithoutOwner(entityType.DisplayName()));
                        }

                        throw new InvalidOperationException(CoreStrings.UnknownKeyValue(entityType.DisplayName(), property.Name));
                    }
                }
            }
            else if (EntityState == EntityState.Modified)
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.GetAfterSaveBehavior() == PropertySaveBehavior.Throw
                        && IsModified(property))
                    {
                        throw new InvalidOperationException(
                            CoreStrings.PropertyReadOnlyAfterSave(
                                property.Name,
                                ComplexType.DisplayName()));
                    }

                    CheckForUnknownKey(property);
                }
            }
            else if (EntityState == EntityState.Deleted)
            {
                foreach (var property in entityType.GetProperties())
                {
                    CheckForUnknownKey(property);
                }
            }

            DiscardStoreGeneratedValues();

            return this;

            void CheckForUnknownKey(IProperty property)
            {
                if (property.IsKey()
                    && _stateData.IsPropertyFlagged(property.GetIndex(), PropertyFlag.Unknown))
                {
                    throw new InvalidOperationException(CoreStrings.UnknownShadowKeyValue(entityType.DisplayName(), property.Name));
                }
            }
        }
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public void HandleConceptualNulls(bool sensitiveLoggingEnabled, bool force, bool isCascadeDelete)
            => ContainingEntry.HandleConceptualNulls(sensitiveLoggingEnabled, force, isCascadeDelete); 

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public void DiscardStoreGeneratedValues()
        {
            if (!_storeGeneratedValues.IsEmpty)
            {
                _storeGeneratedValues = new SidecarValues();
                _stateData.FlagAllProperties(ComplexType.PropertyCount, PropertyFlag.IsStoreGenerated, false);
            }

            foreach (var complexEntry in _complexEntries)
            {
                complexEntry.DiscardStoreGeneratedValues();
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public bool IsStoreGenerated(IProperty property)
            => (property.ValueGenerated.ForAdd()
                    && EntityState == EntityState.Added
                    && (property.GetBeforeSaveBehavior() == PropertySaveBehavior.Ignore
                        || HasTemporaryValue(property)
                        || !HasExplicitValue(property)))
                || (property.ValueGenerated.ForUpdate()
                    && (EntityState is EntityState.Modified or EntityState.Deleted)
                    && (property.GetAfterSaveBehavior() == PropertySaveBehavior.Ignore
                        || !IsModified(property)));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasExplicitValue(IProperty property)
            => !HasSentinelValue(property)
                || _stateData.IsPropertyFlagged(property.GetIndex(), PropertyFlag.IsStoreGenerated)
                || _stateData.IsPropertyFlagged(property.GetIndex(), PropertyFlag.IsTemporary);

        private bool HasSentinelValue(IProperty property)
            => property.IsShadowProperty()
                ? AreEqual(_shadowValues[property.GetShadowIndex()], property.Sentinel, property)
                : property.GetGetter().HasSentinelValue(ComplexObject!);

        IRuntimeTypeBase IInternalEntry.StructuralType
            => ComplexType;

        object IInternalEntry.Object
            => ComplexObject!;
    }
}
