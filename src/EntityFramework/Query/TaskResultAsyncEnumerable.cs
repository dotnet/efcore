// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Query
{
    public sealed class TaskResultAsyncEnumerable<T> : IAsyncEnumerable<T>
    {
        private readonly Task<T> _task;

        public TaskResultAsyncEnumerable([NotNull] Task<T> task)
        {
            Check.NotNull(task, "task");

            _task = task;
        }

        public IAsyncEnumerator<T> GetEnumerator()
        {
            return new Enumerator(_task);
        }

        private sealed class Enumerator : IAsyncEnumerator<T>
        {
            private readonly Task<T> _task;
            private bool _moved;

            public Enumerator(Task<T> task)
            {
                _task = task;
            }

            public async Task<bool> MoveNext(CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!_moved)
                {
                    await _task;

                    _moved = true;

                    return true;
                }

                return false;
            }

            public T Current
            {
                get { return !_moved ? default(T) : _task.Result; }
            }

            void IDisposable.Dispose()
            {
            }
        }
    }
}
