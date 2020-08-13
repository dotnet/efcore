// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class TestDiagnosticSource : DiagnosticSource
    {
        public string EnableFor { get; set; }
        public string LoggedEventName { get; set; }
        public string LoggedMessage { get; set; }

        public override void Write(string name, object value)
        {
            LoggedEventName = name;

            Assert.IsAssignableFrom<EventData>(value);

            LoggedMessage = value.ToString();

            var exceptionProperty = value.GetType().GetTypeInfo().GetDeclaredProperty("Exception");
            if (exceptionProperty != null)
            {
                Assert.IsAssignableFrom<IErrorEventData>(value);
            }
        }

        public override bool IsEnabled(string name) => name == EnableFor;
    }
}
