// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal
{
    public class JsonStringContent : StringContent
    {
        public JsonStringContent(object value)
            : base(JsonConvert.SerializeObject(value), Encoding.UTF8, "application/json")
        {
        }
    }
}
