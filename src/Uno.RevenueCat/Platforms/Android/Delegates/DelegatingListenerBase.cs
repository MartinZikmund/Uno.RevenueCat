namespace Uno.RevenueCat.Platforms.Android.Delegates;

internal abstract class DelegatingListenerBase<TResult> : Java.Lang.Object
{
    // RunContinuationsAsynchronously: without it the awaiter's continuation (arbitrary app code)
    // runs inline on the RevenueCat callback thread.
    private readonly TaskCompletionSource<TResult> _taskCompletionSource =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    private readonly CancellationTokenRegistration _registration;

    protected DelegatingListenerBase(CancellationToken cancellationToken) =>
        _registration = cancellationToken.Register(
            static state => ((TaskCompletionSource<TResult>)state!).TrySetCanceled(),
            _taskCompletionSource);

    public Task<TResult> Task => _taskCompletionSource.Task;

    protected void ReportSuccess(TResult result)
    {
        Unregister();
        _taskCompletionSource.TrySetResult(result);
    }

    protected void ReportException(Exception exception)
    {
        Unregister();
        _taskCompletionSource.TrySetException(exception);
    }

    // Unregister rather than Dispose: Dispose blocks until a concurrently-running cancellation
    // callback finishes, and we are on the native callback thread here.
    private void Unregister() => _registration.Unregister();
}
