// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class CustomJsonIdDefinitionFactory : JsonIdDefinitionFactory
{
    public override IJsonIdDefinition Create(IEntityType entityType)
    {
        var baseDef = base.Create(entityType)!;
        return new CustomJsonIdDefinition(baseDef.Properties, entityType, baseDef.DiscriminatorIsRootType);
    }
}
