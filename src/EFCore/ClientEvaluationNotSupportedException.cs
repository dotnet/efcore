// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Runtime.CompilerServices;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    /// An exception that is thrown when a method intended for SQL translation by EF Core is evaluated by the client.
    /// </summary>
    public class ClientEvaluationNotSupportedException : NotSupportedException
    {
        private readonly string _callerMemberName;

        /// <inheritdoc />
        public override string Message
            => $"'{_callerMemberName}' is only intended for SQL translation by EF Core, but was evaluated by the client.";

        /// <inheritdoc />
        public ClientEvaluationNotSupportedException([CallerMemberName] string method = default)
            => _callerMemberName = method;
    }
}
