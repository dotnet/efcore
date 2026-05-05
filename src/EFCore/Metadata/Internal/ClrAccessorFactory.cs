// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.ExceptionServices;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public abstract class ClrAccessorFactory<TAccessor>
    where TAccessor : class
{
    private static readonly MethodInfo GenericCreate
        = typeof(ClrAccessorFactory<TAccessor>).GetMethod(nameof(CreateGeneric), BindingFlags.Instance | BindingFlags.NonPublic)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public abstract TAccessor Create(IPropertyBase property);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual TAccessor Create(MemberInfo memberInfo)
        => CreateBase(memberInfo);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual TAccessor CreateBase(MemberInfo memberInfo)
    {
        var boundMethod = GenericCreate.MakeGenericMethod(
            memberInfo.DeclaringType!,
            memberInfo.DeclaringType!,
            memberInfo.GetMemberType());

        try
        {
            return (TAccessor)boundMethod.Invoke(this, new object?[] { memberInfo, null })!;
        }
        catch (TargetInvocationException e) when (e.InnerException != null)
        {
            ExceptionDispatchInfo.Capture(e.InnerException).Throw();
            throw;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual TAccessor CreateBase(IPropertyBase propertyBase)
    {
        var boundMethod = GenericCreate.MakeGenericMethod(
            propertyBase.DeclaringType.ContainingEntityType.ClrType,
            propertyBase.DeclaringType.ClrType,
            propertyBase.ClrType);

        try
        {
            return (TAccessor)boundMethod.Invoke(this, new object?[] { GetMemberInfo(propertyBase), propertyBase })!;
        }
        catch (TargetInvocationException e) when (e.InnerException != null)
        {
            ExceptionDispatchInfo.Capture(e.InnerException).Throw();
            throw;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected abstract TAccessor CreateGeneric<TEntity, TStructuralType, TValue>(
        MemberInfo memberInfo,
        IPropertyBase? propertyBase)
        where TEntity : class;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected abstract MemberInfo GetMemberInfo(IPropertyBase propertyBase);
}
