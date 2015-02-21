// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class ModelItemNotFoundException : Exception
    {
        public ModelItemNotFoundException()
        {
        }

        public ModelItemNotFoundException([NotNull] string message)
            : base(message)
        {
            Check.NotEmpty(message, nameof(message));
        }

        public ModelItemNotFoundException([NotNull] string message, [CanBeNull] Exception innerException)
            : base(message, innerException)
        {
            Check.NotEmpty(message, nameof(message));
        }
    }
}
