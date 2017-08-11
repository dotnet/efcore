// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

namespace Microsoft.EntityFrameworkCore.Design
{
    public class OperationResultHandlerTest
    {
        [Fact]
        public void Version_is_zero()
        {
            Assert.Equal(0, new OperationResultHandler().Version);
        }

        [Fact]
        public void HasResult_defaults_to_false()
        {
            Assert.False(new OperationResultHandler().HasResult);
        }

        [Fact]
        public void ErrorType_defaults_to_null()
        {
            Assert.Null(new OperationResultHandler().ErrorType);
        }

        [Fact]
        public void OnResult_works()
        {
            var handler = new OperationResultHandler();
            var result = "Twilight Sparkle";

            handler.OnResult(result);

            Assert.True(handler.HasResult);
            Assert.Equal(result, handler.Result);
        }

        [Fact]
        public void OnError_works()
        {
            var handler = new OperationResultHandler();
            var type = "System.ArgumentOutOfRangeException";
            var message = "Needs to be about 20% more cool.";
            var stackTrace = "The Coolest Trace Yet";

            handler.OnError(type, message, stackTrace);

            Assert.Equal(type, handler.ErrorType);
            Assert.Equal(message, handler.ErrorMessage);
            Assert.Equal(stackTrace, handler.ErrorStackTrace);
        }
    }
}
