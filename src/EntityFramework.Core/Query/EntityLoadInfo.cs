// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.Query
{
    public struct EntityLoadInfo
    {
        private readonly Func<IValueReader, object> _materializer;

        public EntityLoadInfo(
            [NotNull] IValueReader valueReader, [NotNull] Func<IValueReader, object> materializer)
        {
            // hot path
            Debug.Assert(valueReader != null);
            Debug.Assert(materializer != null);

            ValueReader = valueReader;
            _materializer = materializer;
        }

        public IValueReader ValueReader { get; }

        public object Materialize() => _materializer(ValueReader);
    }
}
