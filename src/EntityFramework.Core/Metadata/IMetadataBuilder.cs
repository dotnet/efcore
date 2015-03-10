// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;

namespace Microsoft.Data.Entity.Metadata
{
    public interface IMetadataBuilder<out TMetadata, out TMetadataBuilder>
        where TMetadataBuilder : IMetadataBuilder<TMetadata, TMetadataBuilder>
        where TMetadata : Annotatable
    {
        TMetadataBuilder Annotation([NotNull] string annotation, [NotNull] string value);
        TMetadata Metadata { get; }
        Model Model { get; }
    }
}
