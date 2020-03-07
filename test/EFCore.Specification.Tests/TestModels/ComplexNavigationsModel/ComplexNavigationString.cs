// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel
{
    public class ComplexNavigationString
    {
        public string DefaultText { get; set; }
        public IList<ComplexNavigationGlobalization> Globalizations { get; set; }
    }
}
