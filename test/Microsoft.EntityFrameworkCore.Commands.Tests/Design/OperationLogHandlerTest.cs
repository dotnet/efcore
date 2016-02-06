// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
#if NET451
using Xunit;

namespace Microsoft.EntityFrameworkCore.Design
{
    public class OperationLogHandlerTest
    {
        [Fact]
        public void Version_is_zero()
        {
            Assert.Equal(0, new OperationLogHandler().Version);
        }

        [Fact]
        public void Write_methods_are_noops_when_null()
        {
            var handler = new OperationLogHandler();

            handler.WriteError("Princess Celestia does not exist.");
            handler.WriteWarning("Princess Celestia is in danger.");
            handler.WriteInformation("Princess Celestia is on her way.");
            handler.WriteDebug("Princess Celestia is a princess.");
            handler.WriteTrace("Princess Celestia is an alicorn.");
        }

        [Fact]
        public void WriteError_works()
        {
            string result = null;
            var handler = new OperationLogHandler(writeError: m => result = m);
            var message = "Princess Celestia does not exist.";

            handler.WriteError(message);

            Assert.Equal(message, result);
        }

        [Fact]
        public void WriteWarning_works()
        {
            string result = null;
            var handler = new OperationLogHandler(writeWarning: m => result = m);
            var message = "Princess Celestia is in danger.";

            handler.WriteWarning(message);

            Assert.Equal(message, result);
        }

        [Fact]
        public void WriteInformation_works()
        {
            string result = null;
            var handler = new OperationLogHandler(writeInformation: m => result = m);
            var message = "Princess Celestia is on her way.";

            handler.WriteInformation(message);

            Assert.Equal(message, result);
        }

        [Fact]
        public void WriteDebug_works()
        {
            string result = null;
            var handler = new OperationLogHandler(writeDebug: m => result = m);
            var message = "Princess Celestia is a princess.";

            handler.WriteDebug(message);

            Assert.Equal(message, result);
        }

        [Fact]
        public void WriteTrace_works()
        {
            string result = null;
            var handler = new OperationLogHandler(writeTrace: m => result = m);
            var message = "Princess Celestia is an alicorn.";

            handler.WriteTrace(message);

            Assert.Equal(message, result);
        }
    }
}
#endif
