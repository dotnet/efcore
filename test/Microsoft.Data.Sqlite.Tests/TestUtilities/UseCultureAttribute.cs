// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Globalization;
using System.Reflection;
using Xunit.Sdk;

namespace Microsoft.Data.Sqlite.TestUtilities
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class UseCultureAttribute : BeforeAfterTestAttribute
    {
        private CultureInfo? _originalCulture;
        private CultureInfo? _originalUICulture;

        public UseCultureAttribute(string culture)
            : this(culture, culture)
        {
        }

        public UseCultureAttribute(string culture, string uiCulture)
        {
            Culture = new CultureInfo(culture);
            UICulture = new CultureInfo(uiCulture);
        }

        public CultureInfo Culture { get; }

        public CultureInfo UICulture { get; }

        public override void Before(MethodInfo methodUnderTest)
        {
            _originalCulture = CultureInfo.CurrentCulture;
            _originalUICulture = CultureInfo.CurrentUICulture;
            CultureInfo.CurrentCulture = Culture;
            CultureInfo.CurrentUICulture = UICulture;
        }

        public override void After(MethodInfo methodUnderTest)
        {
            CultureInfo.CurrentCulture = _originalCulture!;
            CultureInfo.CurrentUICulture = _originalUICulture!;
        }
    }
}
