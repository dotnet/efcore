// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public abstract class EntryPropertyValues : PropertyValues
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected EntryPropertyValues(InternalEntryBase internalEntry)
        : base(internalEntry)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override object ToObject()
        => Clone().ToObject();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override void SetValues(object obj)
    {
        Check.NotNull(obj);
        if (obj.GetType() == StructuralType.ClrType)
        {
            SetValuesFromInstance(InternalEntry, (IRuntimeTypeBase)StructuralType, obj);
        }
        else if (obj is Dictionary<string, object> dictionary)
        {
            SetValues(dictionary);
        }
        else
        {
            SetValuesFromDto(InternalEntry, InternalEntry.StructuralType, obj);
        }
    }

    private void SetValuesFromInstance(InternalEntryBase entry, IRuntimeTypeBase structuralType, object obj)
    {
        foreach (var property in structuralType.GetProperties())
        {
            if (property.IsShadowProperty())
            {
                continue;
            }

            SetValueInternal(entry, property, property.GetGetter().GetClrValue(obj));
        }

        foreach (var complexProperty in structuralType.GetComplexProperties())
        {
            if (complexProperty.IsShadowProperty())
            {
                continue;
            }

            if (complexProperty.IsCollection)
            {
                var complexList = (IList?)complexProperty.GetGetter().GetClrValue(obj);
                SetValueInternal(entry, complexProperty, complexList);

                for (var i = 0; i < complexList?.Count; i++)
                {
                    var complexObject = complexList[i];
                    if (complexObject == null)
                    {
                        continue;
                    }

                    var complexEntry = GetComplexCollectionEntry(entry, complexProperty, i);
                    SetValuesFromInstance(complexEntry, complexEntry.StructuralType, complexObject);
                }
            }
            else
            {
                var complexObject = complexProperty.GetGetter().GetClrValue(obj);
                if (complexObject != null)
                {
                    SetValuesFromInstance(entry, (IRuntimeTypeBase)complexProperty.ComplexType, complexObject);
                }
            }
        }
    }

    private void SetValuesFromDto(InternalEntryBase entry, IRuntimeTypeBase structuralType, object obj)
    {
        foreach (var property in structuralType.GetProperties())
        {
            var getter = obj.GetType().GetAnyProperty(property.Name)?.FindGetterProperty();
            if (getter != null)
            {
                SetValueInternal(entry, property, getter.GetValue(obj));
            }
        }

        foreach (var complexProperty in structuralType.GetComplexProperties())
        {
            if (complexProperty.IsCollection)
            {
                var getter = obj.GetType().GetAnyProperty(complexProperty.Name)?.FindGetterProperty();
                if (getter == null)
                {
                    continue;
                }

                var dtoList = (IList?)getter.GetValue(obj);
                if (dtoList == null)
                {
                    SetValueInternal(entry, complexProperty, null);
                    continue;
                }

                var complexList = (IList)((IRuntimePropertyBase)complexProperty).GetIndexedCollectionAccessor().Create(dtoList.Count);
                for (var i = 0; i < dtoList.Count; i++)
                {
                    var item = dtoList[i];
                    if (item == null)
                    {
                        complexList.Add(null);
                    }
                    else
                    {
                        var complexObject = CreateComplexObjectFromDto((IRuntimeComplexType)complexProperty.ComplexType, item);
                        complexList.Add(complexObject);
                    }
                }

                SetValueInternal(entry, complexProperty, complexList);

                for (var i = 0; i < dtoList.Count; i++)
                {
                    var item = dtoList[i];
                    if (item != null)
                    {
                        SetValuesFromDto(
                            GetComplexCollectionEntry(entry, complexProperty, i), (IRuntimeComplexType)complexProperty.ComplexType, item);
                    }
                }
            }
            else
            {
                var getter = obj.GetType().GetAnyProperty(complexProperty.Name)?.FindGetterProperty();
                if (getter == null)
                {
                    continue;
                }

                var dtoComplexValue = getter.GetValue(obj);
                if (dtoComplexValue != null)
                {
                    var complexObject = CreateComplexObjectFromDto((IRuntimeComplexType)complexProperty.ComplexType, dtoComplexValue);
                    SetValueInternal(entry, complexProperty, complexObject);
                    SetValuesFromDto(entry, (IRuntimeComplexType)complexProperty.ComplexType, dtoComplexValue);
                }
            }
        }
    }

    [return: NotNullIfNotNull(nameof(dto))]
    private object? CreateComplexObjectFromDto(IRuntimeComplexType complexType, object? dto)
    {
        if (dto == null)
        {
            return null;
        }

        var values = new object?[complexType.PropertyCount];
        foreach (var property in complexType.GetProperties())
        {
            if (!property.IsShadowProperty())
            {
                var dtoGetter = dto.GetType().GetAnyProperty(property.Name)?.FindGetterProperty();
                if (dtoGetter != null && property.PropertyInfo != null && property.PropertyInfo.CanWrite)
                {
                    values[property.GetIndex()] = dtoGetter.GetValue(dto);
                }
            }
        }

        var complexObject = complexType.GetOrCreateMaterializer(MaterializerSource)(
            new MaterializationContext(new ValueBuffer(values), InternalEntry.Context));

        foreach (var nestedComplexProperty in complexType.GetComplexProperties())
        {
            if (nestedComplexProperty.IsCollection)
            {
                var dtoGetter = dto.GetType().GetAnyProperty(nestedComplexProperty.Name)?.FindGetterProperty();
                if (dtoGetter == null
                    || nestedComplexProperty.IsShadowProperty())
                {
                    continue;
                }

                var nestedList = (IList?)dtoGetter.GetValue(dto);
                if (nestedList == null)
                {
                    continue;
                }

                var nestedCollection = (IList)((IRuntimePropertyBase)nestedComplexProperty).GetIndexedCollectionAccessor()
                    .Create(nestedList.Count);
                for (var i = 0; i < nestedList.Count; i++)
                {
                    var nestedDtoItem = nestedList[i];
                    if (nestedDtoItem == null)
                    {
                        nestedCollection.Add(null);
                    }
                    else
                    {
                        var nestedComplexObject = CreateComplexObjectFromDto(
                            (IRuntimeComplexType)nestedComplexProperty.ComplexType, nestedDtoItem);
                        nestedCollection.Add(nestedComplexObject);
                    }
                }

                var propertyInfo = nestedComplexProperty.PropertyInfo;
                if (propertyInfo != null && propertyInfo.CanWrite)
                {
                    propertyInfo.SetValue(complexObject, nestedCollection);
                }
            }
            else if (!nestedComplexProperty.IsShadowProperty())
            {
                var dtoGetter = dto.GetType().GetAnyProperty(nestedComplexProperty.Name)?.FindGetterProperty();
                if (dtoGetter == null)
                {
                    continue;
                }

                var nestedDto = dtoGetter.GetValue(dto);
                if (nestedDto == null)
                {
                    continue;
                }

                var nestedComplexObject = CreateComplexObjectFromDto((IRuntimeComplexType)nestedComplexProperty.ComplexType, nestedDto);
                var propertyInfo = nestedComplexProperty.PropertyInfo;
                if (propertyInfo != null && propertyInfo.CanWrite)
                {
                    propertyInfo.SetValue(complexObject, nestedComplexObject);
                }
            }
        }

        return complexObject;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override PropertyValues Clone()
    {
        var values = new object?[Properties.Count];
        for (var i = 0; i < values.Length; i++)
        {
            values[i] = GetValueInternal(InternalEntry, Properties[i]);
        }

        var cloned = new ArrayPropertyValues(InternalEntry, values);
        foreach (var complexProperty in ComplexCollectionProperties)
        {
            var collection = (IList?)GetValueInternal(InternalEntry, complexProperty);
            if (collection != null)
            {
                cloned[complexProperty] = collection;
            }
        }

        return cloned;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override void SetValues(PropertyValues propertyValues)
    {
        Check.NotNull(propertyValues);

        foreach (var property in Properties)
        {
            SetValueInternal(InternalEntry, property, propertyValues[property]);
        }

        foreach (var complexProperty in ComplexCollectionProperties)
        {
            SetValueInternal(InternalEntry, complexProperty, propertyValues[complexProperty]);
        }
    }

    /// <inheritdoc />
    public override void SetValues<TProperty>(IDictionary<string, TProperty> values)
        => SetValuesFromDictionary(InternalEntry, (IRuntimeTypeBase)StructuralType, Check.NotNull(values));

    private void SetValuesFromDictionary<TProperty>(
        InternalEntryBase entry,
        IRuntimeTypeBase structuralType,
        IDictionary<string, TProperty> values)
    {
        foreach (var property in structuralType.GetProperties())
        {
            if (values.TryGetValue(property.Name, out var value))
            {
                SetValueInternal(entry, property, value);
            }
        }

        foreach (var complexProperty in structuralType.GetComplexProperties())
        {
            if (!values.TryGetValue(complexProperty.Name, out var complexValue))
            {
                continue;
            }

            if (complexProperty.IsCollection)
            {
                var dictionaryList = complexValue as IList;
                if (complexValue != null && dictionaryList == null)
                {
                    throw new InvalidOperationException(
                        CoreStrings.ComplexCollectionValueNotDictionaryList(
                            complexProperty.Name, complexValue.GetType().ShortDisplayName()));
                }

                IList? complexList = null;
                if (dictionaryList != null)
                {
                    complexList = (IList)((IRuntimePropertyBase)complexProperty).GetIndexedCollectionAccessor()
                        .Create(dictionaryList.Count);
                    for (var i = 0; i < dictionaryList.Count; i++)
                    {
                        var item = dictionaryList[i];
                        var itemDict = item as IDictionary<string, TProperty>;
                        if (item != null && itemDict == null)
                        {
                            throw new InvalidOperationException(
                                CoreStrings.ComplexCollectionValueNotDictionaryList(
                                    complexProperty.Name, item.GetType().ShortDisplayName()));
                        }

                        complexList.Add(CreateComplexObjectFromDictionary((IRuntimeComplexType)complexProperty.ComplexType, itemDict));
                    }
                }

                SetValueInternal(entry, complexProperty, complexList);

                if (dictionaryList != null)
                {
                    for (var i = 0; i < dictionaryList.Count; i++)
                    {
                        if (dictionaryList[i] is IDictionary<string, TProperty> itemDict)
                        {
                            var complexEntry = GetComplexCollectionEntry(entry, complexProperty, i);
                            SetValuesFromDictionary(complexEntry, complexEntry.StructuralType, itemDict);
                        }
                    }
                }
            }
            else
            {
                var complexDict = complexValue as IDictionary<string, TProperty>;
                if (complexValue != null && complexDict == null)
                {
                    throw new InvalidOperationException(
                        CoreStrings.ComplexPropertyValueNotDictionary(complexProperty.Name, complexValue.GetType().ShortDisplayName()));
                }

                var complexObject = CreateComplexObjectFromDictionary((IRuntimeComplexType)complexProperty.ComplexType, complexDict);
                SetValueInternal(entry, complexProperty, complexObject);

                if (complexDict != null)
                {
                    SetValuesFromDictionary(entry, (IRuntimeTypeBase)complexProperty.ComplexType, complexDict);
                }
            }
        }
    }

    private IStructuralTypeMaterializerSource MaterializerSource
        => InternalEntry.StateManager.EntityMaterializerSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected abstract InternalComplexEntry GetComplexCollectionEntry(InternalEntryBase entry, IComplexProperty complexProperty, int i);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override object? this[string propertyName]
    {
        get
        {
            if (StructuralType.FindProperty(propertyName) is { } property)
            {
                return GetValueInternal(InternalEntry, property);
            }

            if (StructuralType.FindComplexProperty(propertyName) is { } complexProperty)
            {
                return GetValueInternal(InternalEntry, complexProperty);
            }

            // If neither found, this will throw an appropriate exception
            return GetValueInternal(InternalEntry, StructuralType.GetProperty(propertyName));
        }
        set
        {
            if (StructuralType.FindProperty(propertyName) is { } property)
            {
                SetValueInternal(InternalEntry, property, value);
                return;
            }

            if (StructuralType.FindComplexProperty(propertyName) is { } complexProperty)
            {
                SetValueInternal(InternalEntry, complexProperty, value);
                return;
            }

            // If neither found, this will throw an appropriate exception
            SetValueInternal(InternalEntry, StructuralType.GetProperty(propertyName), value);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override object? this[IProperty property]
    {
        get => GetValueInternal(InternalEntry, StructuralType.CheckContains(property));
        set => SetValueInternal(InternalEntry, StructuralType.CheckContains(property), value);
    }

    /// <summary>
    ///     Gets or sets the value of the complex collection.
    /// </summary>
    /// <param name="complexProperty">The complex collection property.</param>
    /// <returns>A list of complex objects, not PropertyValues.</returns>
    public override IList? this[IComplexProperty complexProperty]
    {
        get => (IList?)GetValueInternal(InternalEntry, CheckCollection(complexProperty));
        set => SetValueInternal(InternalEntry, CheckCollection(complexProperty), value);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected abstract void SetValueInternal(IInternalEntry entry, IPropertyBase property, object? value);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected abstract object? GetValueInternal(IInternalEntry entry, IPropertyBase property);

    /// <summary>
    ///     Creates a complex object from a dictionary of property values using EF's property accessors.
    /// </summary>
    private object? CreateComplexObjectFromDictionary<TProperty>(
        IRuntimeComplexType complexType,
        IDictionary<string, TProperty>? dictionary)
    {
        if (dictionary == null)
        {
            return null;
        }

        var values = new object?[complexType.PropertyCount];
        foreach (var property in complexType.GetProperties())
        {
            if (dictionary.TryGetValue(property.Name, out var value)
                && !property.IsShadowProperty())
            {
                values[property.GetIndex()] = value;
            }
        }

        var complexObject = complexType.GetOrCreateMaterializer(MaterializerSource)(
            new MaterializationContext(new ValueBuffer(values), InternalEntry.Context));

        foreach (var nestedComplexProperty in complexType.GetComplexProperties())
        {
            if (nestedComplexProperty.IsShadowProperty())
            {
                continue;
            }

            if (dictionary.TryGetValue(nestedComplexProperty.Name, out var nestedValue))
            {
                if (nestedComplexProperty.IsCollection && nestedValue is IList nestedList)
                {
                    var nestedCollection = (IList)((IRuntimePropertyBase)nestedComplexProperty).GetIndexedCollectionAccessor()
                        .Create(nestedList.Count);

                    foreach (var nestedItem in nestedList)
                    {
                        nestedCollection.Add(
                            nestedItem switch
                            {
                                null => null,
                                IDictionary<string, TProperty> nestedItemDict
                                    => CreateComplexObjectFromDictionary(
                                        (IRuntimeComplexType)nestedComplexProperty.ComplexType, nestedItemDict),
                                _ => throw new InvalidOperationException(
                                    CoreStrings.ComplexCollectionValueNotDictionaryList(
                                        nestedComplexProperty.Name, nestedList.GetType().ShortDisplayName()))
                            });
                    }

                    var propertyInfo = nestedComplexProperty.PropertyInfo;
                    if (propertyInfo != null && propertyInfo.CanWrite)
                    {
                        propertyInfo.SetValue(complexObject, nestedCollection);
                    }
                }
                else if (nestedComplexProperty.IsCollection && nestedValue != null)
                {
                    throw new InvalidOperationException(
                        CoreStrings.ComplexCollectionValueNotDictionaryList(
                            nestedComplexProperty.Name, nestedValue.GetType().ShortDisplayName()));
                }
                else if (!nestedComplexProperty.IsCollection)
                {
                    object? nestedComplexObject = null;
                    if (nestedValue is IDictionary<string, TProperty> nestedDict)
                    {
                        nestedComplexObject = CreateComplexObjectFromDictionary(
                            (IRuntimeComplexType)nestedComplexProperty.ComplexType, nestedDict);
                    }
                    else if (nestedValue != null)
                    {
                        throw new InvalidOperationException(
                            CoreStrings.ComplexPropertyValueNotDictionary(
                                nestedComplexProperty.Name, nestedValue.GetType().ShortDisplayName()));
                    }

                    var propertyInfo = nestedComplexProperty.PropertyInfo;
                    if (propertyInfo != null && propertyInfo.CanWrite)
                    {
                        propertyInfo.SetValue(complexObject, nestedComplexObject);
                    }
                }
            }
        }

        return complexObject;
    }
}
