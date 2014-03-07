// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Materialization
{
    public interface IMaterializer
    {
        object Materialize([CanBeNull] /* PERF: No arg checks */ object[] values);
        object[] Shred([CanBeNull] /* PERF: No arg checks */ object entity);
    }
}
