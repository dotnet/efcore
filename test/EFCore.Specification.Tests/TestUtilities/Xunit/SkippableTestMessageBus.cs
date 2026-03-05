// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.TestUtilities.Xunit;

/// <summary>
/// Allows skipping tests by throwing SkipException.
/// </summary>
public class SkippableTestMessageBus : IMessageBus
{
    private readonly IMessageBus _inner;
    private bool _isDisposed;

    public SkippableTestMessageBus(IMessageBus inner)
    {
        _inner = inner;
    }

    public int SkippedCount { get; private set; }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public bool QueueMessage(IMessageSinkMessage message)
    {
        if (message is TestFailed failed)
        {
            var outerException = failed.ExceptionTypes.FirstOrDefault();
            var skipTest = false;
            string? skipReason = null;
            switch (outerException)
            {
                case string _ when ShouldSkipException(outerException):
                    skipTest = true;
                    skipReason = failed.Messages?.FirstOrDefault();
                    break;
                case "Xunit.Sdk.ThrowsException" when failed.ExceptionTypes.Length > 1:
                    outerException = failed.ExceptionTypes[1];
                    if (ShouldSkipException(outerException))
                    {
                        skipTest = true;
                        skipReason = failed.Messages?.Length > 1 ? failed.Messages[1] : null;
                    }

                    break;
                case "System.AggregateException" when failed.ExceptionTypes.Length > 1:
                    // Check if any inner exception in the AggregateException is a skip exception
                    for (var i = 1; i < failed.ExceptionTypes.Length; i++)
                    {
                        if (ShouldSkipException(failed.ExceptionTypes[i]))
                        {
                            skipTest = true;
                            skipReason = failed.Messages?.Length > i ? failed.Messages[i] : null;
                            break;
                        }
                    }

                    break;
            }

            if (skipTest)
            {
                SkippedCount++;
                return _inner.QueueMessage(new TestSkipped(failed.Test, skipReason));
            }
        }

        return _inner.QueueMessage(message);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed)
        {
            return;
        }

        if (disposing)
        {
            _inner.Dispose();
        }

        _isDisposed = true;
    }

    private bool ShouldSkipException(string exceptionType) =>
        exceptionType == "Xunit.Sdk.SkipException";
}
