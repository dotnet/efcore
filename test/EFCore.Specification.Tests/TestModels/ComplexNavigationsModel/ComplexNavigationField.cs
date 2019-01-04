// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel
{
    public class ComplexNavigationField
    {
        public string Name { get; set; }
        public ComplexNavigationString Label { get; set; }
        public ComplexNavigationString Placeholder { get; set; }
    }
}
