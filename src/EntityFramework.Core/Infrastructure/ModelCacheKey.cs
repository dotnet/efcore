// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Infrastructure
{
    public class ModelCacheKey
    {
        public ModelCacheKey([NotNull] DbContext context)
        {
            _dbContextType = context.GetType();
        }

        private readonly Type _dbContextType;

        protected virtual bool Equals([NotNull] ModelCacheKey other) => _dbContextType == other._dbContextType;

        public override bool Equals(object obj)
        {
            var otherAsKey = obj as ModelCacheKey;
            return (otherAsKey != null) && Equals(otherAsKey);
        } 
        public override int GetHashCode() => _dbContextType?.GetHashCode() ?? 0;
    }
}