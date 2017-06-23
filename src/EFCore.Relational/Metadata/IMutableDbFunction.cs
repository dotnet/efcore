// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public interface IMutableDbFunction : IDbFunction
    {
        /// <summary>
        ///     The schema where the function lives in the underlying datastore.
        /// </summary>
        new string Schema { get; [param: CanBeNull] set; }

        /// <summary>
        ///     The name of the function in the underlying datastore.
        /// </summary>
        new string FunctionName { get; [param: NotNull] set;}

        /// <summary>
        ///    A method for converting a method call into a sql function
        /// </summary>
        new Func<IReadOnlyCollection<Expression>, Expression> Translation { get; [param: CanBeNull] set; }
    }
}
