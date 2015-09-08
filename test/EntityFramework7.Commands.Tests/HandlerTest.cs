// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

namespace Microsoft.Data.Entity.Commands.Tests
{
    public class HandlerTest
    {
        [Fact]
        public void HasResult_defaults_to_false()
        {
            Assert.False(new ResultHandler().HasResult);
        }

        [Fact]
        public void ErrorType_defaults_to_null()
        {
            Assert.Null(new ResultHandler().ErrorType);
        }

        [Fact]
        public void OnResult_works()
        {
            var handler = new ResultHandler();
            var result = "Twilight Sparkle";

            handler.OnResult(result);

            Assert.True(handler.HasResult);
            Assert.Equal(result, handler.Result);
        }

        [Fact]
        public void OnError_works()
        {
            var handler = new ResultHandler();
            var type = "System.ArgumentOutOfRangeException";
            var message = "Needs to be about 20% more cool.";
            var stackTrace = "The Coolest Trace Yet";

            handler.OnError(type, message, stackTrace);

            Assert.Equal(type, handler.ErrorType);
            Assert.Equal(message, handler.ErrorMessage);
            Assert.Equal(stackTrace, handler.ErrorStackTrace);
        }

        [Fact]
        public void Write_methods_are_noops_when_null()
        {
            var handler = new LogHandler();

            handler.WriteWarning("Princess Celestia is in danger.");
            handler.WriteInformation("Princess Celestia is on her way.");
            handler.WriteVerbose("Princess Celestia is an alicorn.");
        }

        [Fact]
        public void WriteWarning_works()
        {
            string result = null;
            var handler = new LogHandler(writeWarning: m => result = m);
            var message = "Princess Celestia is in danger.";

            handler.WriteWarning(message);

            Assert.Equal(message, result);
        }

        [Fact]
        public void WriteInformation_works()
        {
            string result = null;
            var handler = new LogHandler(writeInformation: m => result = m);
            var message = "Princess Celestia is on her way.";

            handler.WriteInformation(message);

            Assert.Equal(message, result);
        }

        [Fact]
        public void WriteVerbose_works()
        {
            string result = null;
            var handler = new LogHandler(writeVerbose: m => result = m);
            var message = "Princess Celestia is an alicorn.";

            handler.WriteVerbose(message);

            Assert.Equal(message, result);
        }
    }
}
