// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Scaffolding
{
    public class TypeScaffoldingInfo
    {
        public TypeScaffoldingInfo([NotNull] Type clrType, bool inferred, bool? scaffoldUnicode, int? scaffoldMaxLength)
        {
            Check.NotNull(clrType, nameof(clrType));

            IsInferred = inferred;
            ScaffoldUnicode = scaffoldUnicode;
            ScaffoldMaxLength = scaffoldMaxLength;
            ClrType = clrType;
        }

        public virtual Type ClrType { get; }
        public virtual bool IsInferred { get; }
        public virtual bool? ScaffoldUnicode { get; }
        public virtual int? ScaffoldMaxLength { get; }
    }
}
