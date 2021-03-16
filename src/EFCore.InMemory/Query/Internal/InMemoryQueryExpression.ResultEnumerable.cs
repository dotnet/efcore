// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.InMemory.Query.Internal
{
    public partial class InMemoryQueryExpression
    {
        private sealed class ResultEnumerable : IEnumerable<ValueBuffer>
        {
            private readonly Func<ValueBuffer> _getElement;

            public ResultEnumerable(Func<ValueBuffer> getElement)
            {
                _getElement = getElement;
            }

            public IEnumerator<ValueBuffer> GetEnumerator()
                => new ResultEnumerator(_getElement());

            IEnumerator IEnumerable.GetEnumerator()
                => GetEnumerator();

            private sealed class ResultEnumerator : IEnumerator<ValueBuffer>
            {
                private readonly ValueBuffer _value;
                private bool _moved;

                public ResultEnumerator(ValueBuffer value)
                {
                    _value = value;
                    _moved = _value.IsEmpty;
                }

                public bool MoveNext()
                {
                    if (!_moved)
                    {
                        _moved = true;

                        return _moved;
                    }

                    return false;
                }

                public void Reset()
                {
                    _moved = false;
                }

                object IEnumerator.Current
                    => Current;

                public ValueBuffer Current
                    => !_moved ? ValueBuffer.Empty : _value;

                void IDisposable.Dispose()
                {
                }
            }
        }
    }
}
