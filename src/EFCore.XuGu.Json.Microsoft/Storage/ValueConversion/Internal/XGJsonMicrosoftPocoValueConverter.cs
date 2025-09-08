// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.XuGu.Json.Microsoft.Storage.ValueConversion.Internal
{
    public class XGJsonMicrosoftPocoValueConverter<T> : ValueConverter<T, string>
    {
        public XGJsonMicrosoftPocoValueConverter()
            : base(
                v => ConvertToProviderCore(v),
                v => ConvertFromProviderCore(v))
        {
        }

        public static string ConvertToProviderCore(T v)
            => JsonSerializer.Serialize(v);

        public static T ConvertFromProviderCore(string v)
            => JsonSerializer.Deserialize<T>(v);
    }
}
