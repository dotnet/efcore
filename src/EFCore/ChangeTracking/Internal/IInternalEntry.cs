// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// using Microsoft.EntityFrameworkCore.Metadata.Internal;
//
// namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
//
// /// <summary>
// ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
// ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
// ///     any release. You should only use it directly in your code with extreme caution and knowing that
// ///     doing so can result in application failures when updating to a new Entity Framework Core release.
// /// </summary>
// public interface IInternalEntry
// {
//     /// <summary>
//     ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//     ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//     ///     any release. You should only use it directly in your code with extreme caution and knowing that
//     ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//     /// </summary>
//     object? this[IPropertyBase propertyBase] { get; set; }
//
//     /// <summary>
//     ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//     ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//     ///     any release. You should only use it directly in your code with extreme caution and knowing that
//     ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//     /// </summary>
//     IRuntimeEntityType EntityType { get; }
//
//     /// <summary>
//     ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//     ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//     ///     any release. You should only use it directly in your code with extreme caution and knowing that
//     ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//     /// </summary>
//     bool HasConceptualNull { get; }
//
//     /// <summary>
//     ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//     ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//     ///     any release. You should only use it directly in your code with extreme caution and knowing that
//     ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//     /// </summary>
//     IStateManager StateManager { get; }
//
//     /// <summary>
//     ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//     ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//     ///     any release. You should only use it directly in your code with extreme caution and knowing that
//     ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//     /// </summary>
//     void AcceptChanges();
//
//     /// <summary>
//     ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//     ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//     ///     any release. You should only use it directly in your code with extreme caution and knowing that
//     ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//     /// </summary>
//     void DiscardStoreGeneratedValues();
//
//     /// <summary>
//     ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//     ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//     ///     any release. You should only use it directly in your code with extreme caution and knowing that
//     ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//     /// </summary>
//     object? GetCurrentValue(IPropertyBase propertyBase);
//
//     /// <summary>
//     ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//     ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//     ///     any release. You should only use it directly in your code with extreme caution and knowing that
//     ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//     /// </summary>
//     TProperty GetCurrentValue<TProperty>(IPropertyBase propertyBase);
//
//     /// <summary>
//     ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//     ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//     ///     any release. You should only use it directly in your code with extreme caution and knowing that
//     ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//     /// </summary>
//     object? GetOriginalValue(IPropertyBase propertyBase);
//
//     /// <summary>
//     ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//     ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//     ///     any release. You should only use it directly in your code with extreme caution and knowing that
//     ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//     /// </summary>
//     TProperty GetOriginalValue<TProperty>(IProperty property);
//
//     /// <summary>
//     ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//     ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//     ///     any release. You should only use it directly in your code with extreme caution and knowing that
//     ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//     /// </summary>
//     object? GetPreStoreGeneratedCurrentValue(IPropertyBase propertyBase);
//
//     /// <summary>
//     ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//     ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//     ///     any release. You should only use it directly in your code with extreme caution and knowing that
//     ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//     /// </summary>
//     bool HasExplicitValue(IProperty property);
//
//     /// <summary>
//     ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//     ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//     ///     any release. You should only use it directly in your code with extreme caution and knowing that
//     ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//     /// </summary>
//     bool HasTemporaryValue(IProperty property);
//
//     /// <summary>
//     ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//     ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//     ///     any release. You should only use it directly in your code with extreme caution and knowing that
//     ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//     /// </summary>
//     bool IsConceptualNull(IProperty property);
//
//     /// <summary>
//     ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//     ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//     ///     any release. You should only use it directly in your code with extreme caution and knowing that
//     ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//     /// </summary>
//     bool IsModified(IProperty property);
//
//     /// <summary>
//     ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//     ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//     ///     any release. You should only use it directly in your code with extreme caution and knowing that
//     ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//     /// </summary>
//     bool FlaggedAsStoreGenerated(int propertyIndex);
//
//     /// <summary>
//     ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//     ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//     ///     any release. You should only use it directly in your code with extreme caution and knowing that
//     ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//     /// </summary>
//     bool FlaggedAsTemporary(int propertyIndex);
//
//     /// <summary>
//     ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//     ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//     ///     any release. You should only use it directly in your code with extreme caution and knowing that
//     ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//     /// </summary>
//     bool IsStoreGenerated(IProperty property);
//
//     /// <summary>
//     ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//     ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//     ///     any release. You should only use it directly in your code with extreme caution and knowing that
//     ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//     /// </summary>
//     bool IsUnknown(IProperty property);
//
//     /// <summary>
//     ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//     ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//     ///     any release. You should only use it directly in your code with extreme caution and knowing that
//     ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//     /// </summary>
//     void MarkAsTemporary(IProperty property, bool temporary);
//
//     /// <summary>
//     ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//     ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//     ///     any release. You should only use it directly in your code with extreme caution and knowing that
//     ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//     /// </summary>
//     void MarkUnchangedFromQuery();
//
//     /// <summary>
//     ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//     ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//     ///     any release. You should only use it directly in your code with extreme caution and knowing that
//     ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//     /// </summary>
//     void MarkUnknown(IProperty property);
//
//     /// <summary>
//     ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//     ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//     ///     any release. You should only use it directly in your code with extreme caution and knowing that
//     ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//     /// </summary>
//     IInternalEntry PrepareToSave();
//
//     /// <summary>
//     ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//     ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//     ///     any release. You should only use it directly in your code with extreme caution and knowing that
//     ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//     /// </summary>
//     public object Object { get; } // This won't work for value types
//
//     /// <summary>
//     ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//     ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//     ///     any release. You should only use it directly in your code with extreme caution and knowing that
//     ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//     /// </summary>
//     public void HandleConceptualNulls(bool sensitiveLoggingEnabled, bool force, bool isCascadeDelete);
//
//     /// <summary>
//     ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//     ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//     ///     any release. You should only use it directly in your code with extreme caution and knowing that
//     ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//     /// </summary>
//     void PropagateValue(InternalEntityEntry principalEntry, IProperty principalProperty, IProperty dependentProperty, bool isMaterialization = false, bool setModified = true);
//
//     /// <summary>
//     ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//     ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//     ///     any release. You should only use it directly in your code with extreme caution and knowing that
//     ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//     /// </summary>
//     T ReadOriginalValue<T>(IProperty property, int originalValueIndex);
//
//     /// <summary>
//     ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//     ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//     ///     any release. You should only use it directly in your code with extreme caution and knowing that
//     ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//     /// </summary>
//     object? ReadPropertyValue(IPropertyBase propertyBase);
//
//     /// <summary>
//     ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//     ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//     ///     any release. You should only use it directly in your code with extreme caution and knowing that
//     ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//     /// </summary>
//     T ReadStoreGeneratedValue<T>(int storeGeneratedIndex);
//
//     /// <summary>
//     ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//     ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//     ///     any release. You should only use it directly in your code with extreme caution and knowing that
//     ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//     /// </summary>
//     T ReadTemporaryValue<T>(int storeGeneratedIndex);
//
//     /// <summary>
//     ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//     ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//     ///     any release. You should only use it directly in your code with extreme caution and knowing that
//     ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//     /// </summary>
//     T ReadShadowValue<T>(int shadowIndex);
//
//     /// <summary>
//     ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//     ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//     ///     any release. You should only use it directly in your code with extreme caution and knowing that
//     ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//     /// </summary>
//     void SetOriginalValue(IPropertyBase propertyBase, object? value, int index = -1);
//
//     /// <summary>
//     ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//     ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//     ///     any release. You should only use it directly in your code with extreme caution and knowing that
//     ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//     /// </summary>
//     void SetProperty(IPropertyBase propertyBase, object? value, bool isMaterialization, bool setModified = true, bool isCascadeDelete = false);
//
//     /// <summary>
//     ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//     ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//     ///     any release. You should only use it directly in your code with extreme caution and knowing that
//     ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//     /// </summary>
//     void SetPropertyModified(IProperty property, bool changeState = true, bool isModified = true, bool isConceptualNull = false, bool acceptChanges = false);
//
//     /// <summary>
//     ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//     ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//     ///     any release. You should only use it directly in your code with extreme caution and knowing that
//     ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//     /// </summary>
//     void SetEntityState(
//         EntityState entityState,
//         bool acceptChanges = false,
//         bool modifyProperties = true);
//
//     /// <summary>
//     ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//     ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//     ///     any release. You should only use it directly in your code with extreme caution and knowing that
//     ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//     /// </summary>
//     void OnComplexPropertyModified(IComplexProperty property, bool isModified = true);
//
//     /// <summary>
//     ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//     ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//     ///     any release. You should only use it directly in your code with extreme caution and knowing that
//     ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//     /// </summary>
//     void SetStoreGeneratedValue(IProperty property, object? value, bool setModified = true);
//
//     /// <summary>
//     ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//     ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//     ///     any release. You should only use it directly in your code with extreme caution and knowing that
//     ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//     /// </summary>
//     void SetTemporaryValue(IProperty property, object? value, bool setModified = true);
// }


