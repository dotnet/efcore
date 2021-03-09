// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
