// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
#pragma warning disable EF1001 // AdHocMapper should be made public
public class RelationalAdHocMapper : AdHocMapper
{
    private static readonly bool UseOldBehavior32680 =
        AppContext.TryGetSwitch("Microsoft.EntityFrameworkCore.Issue32680", out var enabled32680) && enabled32680;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public RelationalAdHocMapper(
        IModel model,
        ModelCreationDependencies modelCreationDependencies)
        : base(model, modelCreationDependencies)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override ConventionSet BuildConventionSet()
    {
        if (UseOldBehavior32680)
        {
            return base.BuildConventionSet();
        }

        var conventionSet = base.BuildConventionSet();
        conventionSet.Remove(typeof(RelationalDbFunctionAttributeConvention));
        conventionSet.Remove(typeof(TableNameFromDbSetConvention));
        conventionSet.Remove(typeof(TableValuedDbFunctionConvention));
        return conventionSet;
    }
}
#pragma warning restore EF1001
