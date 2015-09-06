// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Query.Annotations;
using Remotion.Linq;

namespace Microsoft.Data.Entity.Query
{
    public interface IQueryAnnotationExtractor
    {
        IReadOnlyCollection<QueryAnnotationBase> ExtractQueryAnnotations([NotNull] QueryModel queryModel);
    }
}
