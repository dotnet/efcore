// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.JsonQuery;

#nullable disable

public class JsonOwnedConverters
{
    public bool BoolConvertedToIntZeroOne { get; set; }
    public bool BoolConvertedToStringTrueFalse { get; set; }
    public bool BoolConvertedToStringYN { get; set; }
    public int IntZeroOneConvertedToBool { get; set; }
    public string StringTrueFalseConvertedToBool { get; set; }
    public string StringYNConvertedToBool { get; set; }
}
