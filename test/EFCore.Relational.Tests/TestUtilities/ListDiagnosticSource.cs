// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class ListDiagnosticSource : DiagnosticSource
    {
        public ListDiagnosticSource(List<Tuple<string, object>> diagnosticList)
        {
            DiagnosticList = diagnosticList;
        }

        public List<Tuple<string, object>> DiagnosticList { get; }

        public override void Write(string diagnosticName, object parameters)
            => DiagnosticList?.Add(new Tuple<string, object>(diagnosticName, parameters));

        public override bool IsEnabled(string diagnosticName) => true;
    }
}
