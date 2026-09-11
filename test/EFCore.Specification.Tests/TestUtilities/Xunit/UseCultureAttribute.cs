// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Globalization;
using System.Reflection;
using Xunit.v3;

namespace Microsoft.EntityFrameworkCore.TestUtilities.Xunit;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class UseCultureAttribute(string culture, string uiCulture) : BeforeAfterTestAttribute
{
    private CultureInfo _originalCulture = null!;
    private CultureInfo _originalUiCulture = null!;

    public UseCultureAttribute(string culture)
        : this(culture, culture)
    {
    }

    public CultureInfo Culture { get; } = new(culture);
    public CultureInfo UiCulture { get; } = new(uiCulture);

    public override void Before(MethodInfo methodUnderTest, IXunitTest test)
    {
        _originalCulture = CultureInfo.CurrentCulture;
        _originalUiCulture = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentCulture = Culture;
        CultureInfo.CurrentUICulture = UiCulture;
    }

    public override void After(MethodInfo methodUnderTest, IXunitTest test)
    {
        CultureInfo.CurrentCulture = _originalCulture;
        CultureInfo.CurrentUICulture = _originalUiCulture;
    }
}
