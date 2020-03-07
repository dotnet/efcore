// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Design
{
    public class OperationExecutorTest
    {
        [ConditionalFact]
        public void Ctor_validates_arguments()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new OperationExecutor(null, null));
            Assert.Equal("reportHandler", ex.ParamName);

            ex = Assert.Throws<ArgumentNullException>(() => new OperationExecutor(new OperationReportHandler(), null));
            Assert.Equal("args", ex.ParamName);
        }

        public class OperationBaseTests
        {
            [ConditionalFact]
            public void Execute_catches_exceptions()
            {
                var handler = new OperationResultHandler();
                var error = new ArgumentOutOfRangeException("Needs to be about 20% more cool.");

                new MockOperation<string>(handler, (Action)(() => throw error));

                Assert.Equal(error.GetType().FullName, handler.ErrorType);
                Assert.Equal(error.Message, handler.ErrorMessage);
                Assert.NotEmpty(handler.ErrorStackTrace);
            }

            [ConditionalFact]
            public void Execute_sets_results()
            {
                var handler = new OperationResultHandler();
                var result = "Twilight Sparkle";

                new MockOperation<string>(handler, () => result);

                Assert.Equal(result, handler.Result);
            }

            [ConditionalFact]
            public void Execute_enumerates_results()
            {
                var handler = new OperationResultHandler();

                new MockOperation<string>(handler, () => YieldResults());

                Assert.IsType<string[]>(handler.Result);
                Assert.Equal(new[] { "Twilight Sparkle", "Princess Celestia" }, handler.Result);
            }

            private IEnumerable<string> YieldResults()
            {
                yield return "Twilight Sparkle";
                yield return "Princess Celestia";
            }

            private class MockOperation<T> : OperationExecutor.OperationBase
            {
                public MockOperation(IOperationResultHandler resultHandler, Action action)
                    : base(resultHandler)
                    => Execute(action);

                public MockOperation(IOperationResultHandler resultHandler, Func<T> action)
                    : base(resultHandler)
                    => Execute(action);

                public MockOperation(IOperationResultHandler resultHandler, Func<IEnumerable<T>> action)
                    : base(resultHandler)
                    => Execute(action);
            }
        }
    }
}
