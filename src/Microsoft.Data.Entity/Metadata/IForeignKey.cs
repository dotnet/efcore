// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.Data.Entity.Metadata
{
    public interface IForeignKey : IMetadata
    {
        IReadOnlyList<IProperty> DependentProperties { get; }
        IReadOnlyList<IProperty> PrincipalProperties { get; }
        IEntityType PrincipalType { get; }
        IEntityType DependentType { get; }
        bool IsUnique { get; }
        bool IsRequired { get; }
    }
}
