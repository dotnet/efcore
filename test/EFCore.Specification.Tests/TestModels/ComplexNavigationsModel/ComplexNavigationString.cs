// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel
{
    public class ComplexNavigationString
    {
        public string DefaultText { get; set; }
        public IList<ComplexNavigationGlobalization> Globalizations { get; set; }
    }
}
