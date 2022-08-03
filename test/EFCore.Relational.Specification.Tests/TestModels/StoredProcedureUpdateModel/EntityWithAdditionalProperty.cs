// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.StoredProcedureUpdateModel;

public class EntityWithAdditionalProperty
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int AdditionalProperty { get; set; }
}
