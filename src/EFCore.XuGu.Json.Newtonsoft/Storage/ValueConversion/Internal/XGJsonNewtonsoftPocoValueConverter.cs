// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.XuGu.Json.Newtonsoft.Storage.ValueConversion.Internal
{
    public class XGJsonNewtonsoftPocoValueConverter<T> : ValueConverter<T, string>
    {
        public XGJsonNewtonsoftPocoValueConverter()
            : base(
                v => ConvertToProviderCore(v),
                v => ConvertFromProviderCore(v))
        {
        }

        private static string ConvertToProviderCore(T v)
            => JsonConvert.SerializeObject(v);

        private static T ConvertFromProviderCore(string v)
            => JsonConvert.DeserializeObject<T>(v);
    }
}
