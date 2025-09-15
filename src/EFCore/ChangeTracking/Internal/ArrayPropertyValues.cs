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
public class ArrayPropertyValues : PropertyValues
{
    private readonly object?[] _values;
    private readonly List<ArrayPropertyValues?>?[] _complexCollectionValues;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public ArrayPropertyValues(InternalEntryBase internalEntry, object?[] values)
        : base(internalEntry)
    {
        _values = values;
        _complexCollectionValues = new List<ArrayPropertyValues?>?[ComplexCollectionProperties.Count];
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override object ToObject()
    {
        var structuralObject = StructuralType.GetOrCreateMaterializer(MaterializerSource)(
            new MaterializationContext(new ValueBuffer(_values), InternalEntry.Context));

        for (var i = 0; i < _complexCollectionValues.Length; i++)
        {
            var propertyValuesList = _complexCollectionValues[i];
            if (propertyValuesList == null)
            {
                continue;
            }

            var complexProperty = ComplexCollectionProperties[i];
            Check.DebugAssert(
                !complexProperty.IsShadowProperty(),
                $"Shadow complex property {complexProperty.Name} is not supported. Issue #31243");
            var list = (IList)((IRuntimeComplexProperty)complexProperty).GetIndexedCollectionAccessor().Create(propertyValuesList.Count);
            structuralObject = ((IRuntimeComplexProperty)complexProperty).GetSetter().SetClrValue(structuralObject, list);

            foreach (var propertyValues in propertyValuesList)
            {
                list.Add(propertyValues?.ToObject() ?? complexProperty.ComplexType.ClrType.GetDefaultValue());
            }
        }

        return structuralObject;
    }

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
            for (var i = 0; i < _values.Length; i++)
            {
                var property = Properties[i];
                if (!property.IsShadowProperty())
                {
                    SetValue(i, property.GetGetter().GetClrValueUsingContainingEntity(obj));
                }
            }

            foreach (var complexProperty in ComplexCollectionProperties)
            {
                if (complexProperty.IsShadowProperty())
                {
                    continue;
                }

                var list = (IList?)complexProperty.GetGetter().GetClrValueUsingContainingEntity(obj);
                _complexCollectionValues[complexProperty.GetIndex()] = GetComplexCollectionPropertyValues(complexProperty, list);
            }
        }
        else
        {
            for (var i = 0; i < _values.Length; i++)
            {
                var getter = obj.GetType().GetAnyProperty(Properties[i].Name)?.FindGetterProperty();
                if (getter != null)
                {
                    SetValue(i, getter.GetValue(obj));
                }
            }

            foreach (var complexProperty in ComplexCollectionProperties)
            {
                var getter = obj.GetType().GetAnyProperty(complexProperty.Name)?.FindGetterProperty();
                if (getter != null)
                {
                    var complexCollection = (IList?)getter.GetValue(obj);
                    var propertyValuesList = GetComplexCollectionPropertyValues(complexProperty, complexCollection);
                    _complexCollectionValues[complexProperty.GetIndex()] = propertyValuesList;
                }
            }
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override PropertyValues Clone()
    {
        var copies = new object[_values.Length];
        Array.Copy(_values, copies, _values.Length);

        var clone = new ArrayPropertyValues(InternalEntry, copies);
        for (var i = 0; i < _complexCollectionValues.Length; i++)
        {
            var list = _complexCollectionValues[i];
            if (list == null)
            {
                continue;
            }

            var clonedList = new List<ArrayPropertyValues?>();
            foreach (var propertyValues in list)
            {
                clonedList.Add((ArrayPropertyValues?)propertyValues?.Clone());
            }

            clone._complexCollectionValues[i] = clonedList;
        }

        return clone;
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

        for (var i = 0; i < _values.Length; i++)
        {
            SetValue(i, propertyValues[Properties[i]]);
        }

        for (var i = 0; i < _complexCollectionValues.Length; i++)
        {
            var list = propertyValues[ComplexCollectionProperties[i]];
            _complexCollectionValues[i] = GetComplexCollectionPropertyValues(ComplexCollectionProperties[i], list);
        }
    }

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
                return _values[property.GetIndex()];
            }

            if (StructuralType.FindComplexProperty(propertyName) is { } complexProperty)
            {
                return this[complexProperty];
            }

            // If neither found this will throw an appropriate exception
            return _values[StructuralType.GetProperty(propertyName).GetIndex()];
        }
        set
        {
            if (StructuralType.FindProperty(propertyName) is { } property)
            {
                SetValue(property.GetIndex(), value);
                return;
            }

            if (StructuralType.FindComplexProperty(propertyName) is { } complexProperty)
            {
                CheckCollection(complexProperty);

                switch (value)
                {
                    case null:
                        _complexCollectionValues[complexProperty.GetIndex()] = null;
                        break;
                    case List<ArrayPropertyValues?> propertyValuesList:
                        _complexCollectionValues[complexProperty.GetIndex()] = propertyValuesList;
                        break;
                    case IList complexCollection:
                        this[complexProperty] = complexCollection;
                        break;
                    default:
                        throw new InvalidOperationException(
                            CoreStrings.ComplexPropertyValueNotList(
                                complexProperty.Name, complexProperty.ClrType, value.GetType().ShortDisplayName()));
                }
            }

            // If neither found this will throw an appropriate exception
            SetValue(StructuralType.GetProperty(propertyName).GetIndex(), value);
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
        get => _values[StructuralType.CheckContains(property).GetIndex()];
        set => SetValue(StructuralType.CheckContains(property).GetIndex(), value);
    }

    /// <summary>
    ///     Gets or sets the value of the complex collection.
    /// </summary>
    /// <param name="complexProperty">The complex collection property.</param>
    /// <returns>A list of complex objects, not PropertyValues.</returns>
    public override IList? this[IComplexProperty complexProperty]
    {
        get
        {
            CheckCollection(complexProperty);

            var propertyValuesList = _complexCollectionValues?[complexProperty.GetIndex()];
            if (propertyValuesList == null)
            {
                return null;
            }

            var complexObjectsList = (IList)((IRuntimePropertyBase)complexProperty).GetIndexedCollectionAccessor()
                .Create(propertyValuesList.Count);
            foreach (var propertyValues in propertyValuesList)
            {
                complexObjectsList.Add(propertyValues?.ToObject());
            }

            return complexObjectsList;
        }

        set => SetComplexCollectionValue(CheckCollection(complexProperty), GetComplexCollectionPropertyValues(complexProperty, value));
    }

    /// <inheritdoc />
    public override void SetValues<TProperty>(IDictionary<string, TProperty> values)
        => SetValuesFromDictionary((IRuntimeTypeBase)StructuralType, Check.NotNull(values));

    private void SetValuesFromDictionary<TProperty>(IRuntimeTypeBase structuralType, IDictionary<string, TProperty> values)
    {
        foreach (var property in structuralType.GetProperties())
        {
            if (values.TryGetValue(property.Name, out var value))
            {
                this[property] = value;
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

                SetComplexCollectionValue(
                    complexProperty, CreatePropertyValuesFromDictionaryList<TProperty>(complexProperty, dictionaryList));
            }
            else
            {
                var complexDict = complexValue as IDictionary<string, TProperty>;
                if (complexValue != null && complexDict == null)
                {
                    throw new InvalidOperationException(
                        CoreStrings.ComplexPropertyValueNotDictionary(complexProperty.Name, complexValue.GetType().ShortDisplayName()));
                }

                if (complexDict != null)
                {
                    SetValuesFromDictionary((IRuntimeTypeBase)complexProperty.ComplexType, complexDict);
                }
            }
        }
    }

    private List<ArrayPropertyValues?>? CreatePropertyValuesFromDictionaryList<TProperty>(
        IComplexProperty complexProperty,
        IList? collection)
    {
        if (collection == null)
        {
            return null;
        }

        var propertyValuesList = new List<ArrayPropertyValues?>();
        for (var i = 0; i < collection.Count; i++)
        {
            var item = collection[i];
            if (item == null)
            {
                propertyValuesList.Add(null);
            }
            else
            {
                if (item is not IDictionary<string, TProperty> itemDict)
                {
                    throw new InvalidOperationException(
                        CoreStrings.ComplexCollectionValueNotDictionaryList(complexProperty.Name, item.GetType().ShortDisplayName()));
                }

                var complexEntry = new InternalComplexEntry((IRuntimeComplexType)complexProperty.ComplexType, InternalEntry, i);
                var complexType = complexEntry.StructuralType;
                var values = new object?[complexType.GetFlattenedProperties().Count()];
                var complexPropertyValues = new ArrayPropertyValues(complexEntry, values);
                complexPropertyValues.SetValues(itemDict);

                propertyValuesList.Add(complexPropertyValues);
            }
        }

        return propertyValuesList;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override TValue GetValue<TValue>(string propertyName)
        => (TValue)this[propertyName]!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override TValue GetValue<TValue>(IProperty property)
        => (TValue)this[property]!;

    private void SetValue(int index, object? value)
    {
        var property = Properties[index];
        Check.DebugAssert(
            index == property.GetIndex(),
            $"Property index {property.GetIndex()} does not match the index {index} for the property {property.Name} in the structural type {StructuralType.DisplayName()}");

        if (value != null)
        {
            if (!property.ClrType.IsInstanceOfType(value))
            {
                throw new InvalidCastException(
                    CoreStrings.InvalidType(
                        property.Name,
                        property.DeclaringType.DisplayName(),
                        value.GetType().DisplayName(),
                        property.ClrType.ShortDisplayName()));
            }
        }
        else
        {
            if (!property.ClrType.IsNullableType())
            {
                throw new InvalidOperationException(
                    CoreStrings.ValueCannotBeNull(
                        property.Name,
                        property.DeclaringType.DisplayName(),
                        property.ClrType.ShortDisplayName()));
            }
        }

        _values[index] = value;
    }

    private IStructuralTypeMaterializerSource MaterializerSource
        => InternalEntry.StateManager.EntityMaterializerSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    internal void SetComplexCollectionValue(IComplexProperty complexProperty, List<ArrayPropertyValues?>? propertyValuesList)
        => _complexCollectionValues[complexProperty.GetIndex()] = propertyValuesList;

    private List<ArrayPropertyValues?>? GetComplexCollectionPropertyValues(IComplexProperty complexProperty, IList? collection)
    {
        if (collection == null)
        {
            return null;
        }

        var propertyValuesList = new List<ArrayPropertyValues?>();
        for (var i = 0; i < collection.Count; i++)
        {
            var item = collection[i];
            if (item == null)
            {
                propertyValuesList.Add(null);
            }
            else
            {
                var complexPropertyValues = CreateComplexPropertyValues(
                    item,
                    new InternalComplexEntry((IRuntimeComplexType)complexProperty.ComplexType, InternalEntry, i));
                propertyValuesList.Add(complexPropertyValues);
            }
        }

        return propertyValuesList;

        ArrayPropertyValues CreateComplexPropertyValues(object complexObject, InternalComplexEntry entry)
        {
            var complexType = entry.StructuralType;
            var properties = complexType.GetFlattenedProperties().AsList();
            var values = new object?[properties.Count];

            for (var i = 0; i < properties.Count; i++)
            {
                var property = properties[i];
                var getter = property.GetGetter();
                values[i] = getter.GetClrValue(complexObject);
            }

            var complexPropertyValues = new ArrayPropertyValues(entry, values);

            foreach (var nestedComplexProperty in complexPropertyValues.ComplexCollectionProperties)
            {
                var nestedCollection = (IList?)nestedComplexProperty.GetGetter().GetClrValue(complexObject);
                var propertyValuesList = GetComplexCollectionPropertyValues(nestedComplexProperty, nestedCollection);
                complexPropertyValues.SetComplexCollectionValue(nestedComplexProperty, propertyValuesList);
            }

            return complexPropertyValues;
        }
    }
}
