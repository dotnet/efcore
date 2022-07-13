// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels;

public class SimpleEntity
{
    public static string ShadowPropertyName = "ShadowStringProperty";

    public virtual int Id { get; set; }

    public virtual string StringProperty { get; set; }
}
