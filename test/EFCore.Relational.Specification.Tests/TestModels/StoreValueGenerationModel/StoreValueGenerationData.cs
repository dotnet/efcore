// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.StoreValueGenerationModel;

public class StoreValueGenerationData : IEquatable<StoreValueGenerationData>
{
    // Generated on add (except for WithNoDatabaseGenerated2)
    public int Id { get; set; }

    // Generated on update (except for WithNoDatabaseGenerated2)
    public int Data1 { get; set; }

    // Not generated, except for for WithAllDatabaseGenerated
    public int Data2 { get; set; }

    public bool Equals(StoreValueGenerationData? other)
        => other is not null
            && (ReferenceEquals(this, other)
                || (Id == other.Id
                    && Data1 == other.Data1
                    && Data2 == other.Data2));
}
