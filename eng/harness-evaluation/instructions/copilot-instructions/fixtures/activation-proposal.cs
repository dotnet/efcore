namespace Microsoft.EntityFrameworkCore;

public partial class DbContext
{
    /// <summary>
    ///     Obtains and initializes a handler for this context.
    /// </summary>
    /// <typeparam name="THandler">The handler type.</typeparam>
    /// <param name="factory">The handler factory.</param>
    /// <param name="initialize">The initialization callback.</param>
    /// <returns>The initialized handler.</returns>
    public virtual async Task<THandler> ActivateAsync<THandler>(
        Func<THandler?> factory,
        Func<THandler, Task> initialize,
        bool areFallbacksEnabled = false)
        where THandler : class
    {
        CheckDisposed();
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(initialize);

        var handler = factory();
        if (handler is not null)
        {
            await initialize(handler).ConfigureAwait(false);
            return handler;
        }

        if (!areFallbacksEnabled)
        {
            throw new InvalidOperationException(CoreStrings.NotSupported);
        }

        return await ActivateFallbackAsync(initialize);
    }

    private static async Task<THandler> ActivateFallbackAsync<THandler>(Func<THandler, Task> initialize)
        where THandler : class
    {
        THandler handler;
        try
        {
            handler = (THandler)Activator.CreateInstance(typeof(THandler))!;
        }
        catch (MissingMethodException exception)
        {
            throw new InvalidOperationException($"Cannot activate handler '{typeof(THandler).Name}' without a public default constructor.", exception);
        }

        await initialize(handler);
        return handler;
    }
}