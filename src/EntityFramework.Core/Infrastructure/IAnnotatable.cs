// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Infrastructure
{
    public interface IAnnotatable
    {
        object this[[NotNull] string annotationName] { get; }

        Annotation GetAnnotation([NotNull] string annotationName);

        IEnumerable<IAnnotation> Annotations { get; }
    }
}
