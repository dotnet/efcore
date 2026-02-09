// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;
using Newtonsoft.Json.Linq;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public static class CosmosSerializationUtilities
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly MethodInfo SerializeObjectToComplexPropertyMethod
        = typeof(CosmosSerializationUtilities).GetMethod(nameof(SerializeObjectToComplexProperty)) ?? throw new UnreachableException();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static JToken SerializeObjectToComplexProperty(IComplexType type, object? value, bool collection) // #34567
    {
        if (value is null)
        {
            return JValue.CreateNull();
        }

        if (collection)
        {
            var array = new JArray();
            foreach (var element in (IEnumerable)value)
            {
                array.Add(SerializeObjectToComplexProperty(type, element, false));
            }
            return array;
        }

        var obj = new JObject();
        foreach (var property in type.GetProperties())
        {
            var jsonPropertyName = property.GetJsonPropertyName();

            var propertyValue = property.GetGetter().GetClrValue(value);
            var providerValue = property.ConvertToProviderValue(propertyValue);
            if (providerValue is null)
            {
                if (!property.IsNullable)
                {
                    throw new InvalidOperationException(CoreStrings.PropertyConceptualNull(property.Name, type.DisplayName()));
                }

                obj[jsonPropertyName] = null;
            }
            else
            {
                obj[jsonPropertyName] = JToken.FromObject(providerValue, CosmosClientWrapper.Serializer);
            }
        }

        foreach (var complexProperty in type.GetComplexProperties())
        {
            var jsonPropertyName = complexProperty.Name;
            var propertyValue = complexProperty.GetGetter().GetClrValue(value);
            if (propertyValue is null)
            {
                if (!complexProperty.IsNullable)
                {
                    throw new InvalidOperationException(CoreStrings.PropertyConceptualNull(complexProperty.Name, type.DisplayName()));
                }

                obj[jsonPropertyName] = null;
            }
            else
            {
                obj[jsonPropertyName] = SerializeObjectToComplexProperty(complexProperty.ComplexType, propertyValue, complexProperty.IsCollection);
            }
        }

        return obj;
    }
}
