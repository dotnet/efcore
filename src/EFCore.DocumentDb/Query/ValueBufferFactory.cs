// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Azure.Documents;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class ValueBufferFactory
    {
        private readonly Func<Document, object[]> _valueBufferCreator;

        public ValueBufferFactory(Func<Document, object[]> valueBufferCreator)
        {
            _valueBufferCreator = valueBufferCreator;
        }

        public ValueBuffer CreateValueBuffer(Document document)
        {
            return new ValueBuffer(_valueBufferCreator(document));
        }
    }
}
