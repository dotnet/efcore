// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.XuGu.Storage;

namespace Microsoft.EntityFrameworkCore.XuGu.Scaffolding.Internal
{
    internal class XGCodeGenerationServerVersionCreation
    {
        public ServerVersion ServerVersion { get; }

        public XGCodeGenerationServerVersionCreation(ServerVersion serverVersion)
        {
            ServerVersion = serverVersion;
        }
    }
}
