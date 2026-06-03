// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel;

#nullable disable

public class ComplexNavigationString
{
    public string DefaultText { get; set; }
    public IList<ComplexNavigationGlobalization> Globalizations { get; set; }
}
