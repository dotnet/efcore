// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if NET46
using Xunit;

namespace Microsoft.EntityFrameworkCore.Design.Tests.Design
{
    public class OperationReportHandlerTest
    {
        [Fact]
        public void Version_is_zero()
        {
            Assert.Equal(0, new OperationReportHandler().Version);
        }

        [Fact]
        public void On_methods_are_noops_when_null()
        {
            var handler = new OperationReportHandler();

            handler.OnWarning("Princess Celestia is in danger.");
            handler.OnInformation("Princess Celestia is on her way.");
            handler.OnVerbose("Princess Celestia is an alicorn.");
        }

        [Fact]
        public void OnWarning_works()
        {
            string result = null;
            var handler = new OperationReportHandler(warningHandler: m => result = m);
            var message = "Princess Celestia is in danger.";

            handler.OnWarning(message);

            Assert.Equal(message, result);
        }

        [Fact]
        public void OnInformation_works()
        {
            string result = null;
            var handler = new OperationReportHandler(informationHandler: m => result = m);
            var message = "Princess Celestia is on her way.";

            handler.OnInformation(message);

            Assert.Equal(message, result);
        }

        [Fact]
        public void OnVerbose_works()
        {
            string result = null;
            var handler = new OperationReportHandler(verboseHandler: m => result = m);
            var message = "Princess Celestia is an alicorn.";

            handler.OnVerbose(message);

            Assert.Equal(message, result);
        }
    }
}
#elif NETCOREAPP2_0
#else
#error target frameworks need to be updated.
#endif
