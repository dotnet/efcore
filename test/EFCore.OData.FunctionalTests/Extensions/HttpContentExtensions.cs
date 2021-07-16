// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Microsoft.EntityFrameworkCore.Extensions
{
    public static class HttpContentExtensions
    {
        public static async Task<T> ReadAsObject<T>(this HttpContent content)
        {
            var json = await content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
