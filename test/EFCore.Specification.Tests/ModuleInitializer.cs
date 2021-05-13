// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Globalization;
using System.Runtime.CompilerServices;

namespace Microsoft.EntityFrameworkCore
{
    public class ModuleInitializer
    {
        [ModuleInitializer]
        internal static void Initialize()
        {
            var enUsCulture = CultureInfo.CreateSpecificCulture("en-US");
            CultureInfo.CurrentCulture = enUsCulture;
            CultureInfo.DefaultThreadCurrentCulture = enUsCulture;
        }
    }
}
