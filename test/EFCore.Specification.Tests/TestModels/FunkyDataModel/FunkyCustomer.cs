// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.FunkyDataModel;

#nullable disable

public class FunkyCustomer
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }

    public bool? NullableBool { get; set; }
}
