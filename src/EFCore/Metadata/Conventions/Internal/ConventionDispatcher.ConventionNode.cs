// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;

public partial class ConventionDispatcher
{
    private abstract class ConventionNode
    {
        public abstract void Run(ConventionDispatcher dispatcher);
    }
}
