// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Migrations
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class MigrationAttribute : Attribute
    {
        public MigrationAttribute([NotNull] string id)
        {
            Check.NotEmpty(id, nameof(id));

            Id = id;
        }

        public string Id { get; }
    }
}
