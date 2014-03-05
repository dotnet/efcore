// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Metadata
{
    public interface IMetadata
    {
        string this[[NotNull] string annotationName] { get; }
        IReadOnlyList<IAnnotation> Annotations { get; }
    }
}
