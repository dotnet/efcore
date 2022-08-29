// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.StoredProcedureUpdateModel;

public class TphChild2 : TphParent
{
    public int Child2InputProperty { get; set; }
    public int Child2OutputParameterProperty { get; set; }
    public int Child2ResultColumnProperty { get; set; }
}
