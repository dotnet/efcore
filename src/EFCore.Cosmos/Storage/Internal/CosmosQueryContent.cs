// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal
{
    public class CosmosQueryContent : HttpContent
    {
        public CosmosQueryContent(CosmosSqlQuery query)
        {
            Query = query;
            Headers.ContentType = new MediaTypeHeaderValue("application/query+json");
        }

        public CosmosSqlQuery Query { get; }

        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            using (var writer = new StreamWriter(stream, new UTF8Encoding(), bufferSize: 1024, leaveOpen: true))
            {
                using (var jsonWriter = new JsonTextWriter(writer))
                {
                    JsonSerializer.Create().Serialize(jsonWriter, Query);

                    await jsonWriter.FlushAsync();
                }
            }
        }

        protected override bool TryComputeLength(out long length)
        {
            length = -1;
            return false;
        }
    }
}
