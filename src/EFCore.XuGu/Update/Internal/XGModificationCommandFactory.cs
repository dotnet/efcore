// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Update;

namespace Microsoft.EntityFrameworkCore.XuGu.Update.Internal;

public class XGModificationCommandFactory : IModificationCommandFactory
{
    public virtual IModificationCommand CreateModificationCommand(
        in ModificationCommandParameters modificationCommandParameters)
        => new XGModificationCommand(modificationCommandParameters);

    public virtual INonTrackedModificationCommand CreateNonTrackedModificationCommand(
        in NonTrackedModificationCommandParameters modificationCommandParameters)
        => new XGModificationCommand(modificationCommandParameters);
}
