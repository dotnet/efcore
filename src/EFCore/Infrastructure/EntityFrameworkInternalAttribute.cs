// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Infrastructure;

/// <summary>
///     Marks an API as internal to Entity Framework Core. These APIs are not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use such APIs directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
[AttributeUsage(
    AttributeTargets.Enum
    | AttributeTargets.Class
    | AttributeTargets.Struct
    | AttributeTargets.Interface
    | AttributeTargets.Event
    | AttributeTargets.Field
    | AttributeTargets.Method
    | AttributeTargets.Delegate
    | AttributeTargets.Property
    | AttributeTargets.Constructor)]
public sealed class EntityFrameworkInternalAttribute : Attribute;
