// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.XuGu.Json.Newtonsoft.Storage.ValueConversion.Internal
{
    public class XGJsonNewtonsoftJTokenValueConverter : ValueConverter<JToken, string>
    {
        public XGJsonNewtonsoftJTokenValueConverter()
            : base(
                v => ConvertToProviderCore(v),
                v => ConvertFromProviderCore(v))
        {
        }

        private static string ConvertToProviderCore(JToken v)
            => v.ToString(Formatting.None);

        private static JToken ConvertFromProviderCore(string v)
            => JToken.Parse(v);
    }
}
