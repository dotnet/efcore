// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Identity;

namespace Microsoft.Data.SqlServer
{
    public class SequentialGuidIdentityGenerator : IIdentityGenerator<Guid>
    {
        private static long _counter;

        static SequentialGuidIdentityGenerator()
        {
            _counter = DateTime.UtcNow.Ticks;
        }

        public Task<Guid> NextAsync()
        {
            var guidBytes = Guid.NewGuid().ToByteArray();
            var counterBytes = BitConverter.GetBytes(Interlocked.Increment(ref _counter));

            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(counterBytes);
            }

            guidBytes[08] = counterBytes[1];
            guidBytes[09] = counterBytes[0];
            guidBytes[10] = counterBytes[7];
            guidBytes[11] = counterBytes[6];
            guidBytes[12] = counterBytes[5];
            guidBytes[13] = counterBytes[4];
            guidBytes[14] = counterBytes[3];
            guidBytes[15] = counterBytes[2];

            return Task.FromResult(new Guid(guidBytes));
        }
    }
}
