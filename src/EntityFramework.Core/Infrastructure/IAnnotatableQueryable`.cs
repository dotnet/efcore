// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Infrastructure
{
    public interface IAnnotatableQueryable<out TEntity>
        where TEntity : class
    {
        IQueryable<TEntity> AnnotateQuery([NotNull] object annotation);
    }
}
