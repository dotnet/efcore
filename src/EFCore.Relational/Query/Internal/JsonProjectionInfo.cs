// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public readonly struct JsonProjectionInfo
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public JsonProjectionInfo(
        int jsonColumnIndex,
        List<(IProperty?, int?, int?)> keyAccessInfo)
    {
        JsonColumnIndex = jsonColumnIndex;
        KeyAccessInfo = keyAccessInfo;
    }

    /// <summary>
    ///     Projection index for json column name.
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    public int JsonColumnIndex { get; }

    /// <summary>
    ///     Information needed to construct key values for the initial JSON entity:
    ///     - for key properties of the owner entity we store IProperty under KeyProperty
    ///     and projection index of the key in the KeyProjectionIndex, ConstantKeyValue is null,
    ///     - for constant array element access we store the value directly in ConstantKeyValue
    ///     KeyProperty and KeyProjectionIndex are null,
    ///     - for non-constant array element access we store it's projection index in KeyProjectionIndex
    ///     KeyProperty and ConstantKeyValue are null.
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    public List<(IProperty? KeyProperty, int? ConstantKeyValue, int? KeyProjectionIndex)> KeyAccessInfo { get; }
}
