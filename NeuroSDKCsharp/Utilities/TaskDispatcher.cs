using System.Collections.Concurrent;

/// <summary>
/// This is here to replicate UniTask's SwitchToMainThread
/// </summary>
public static class TaskDispatcher
{
	private static readonly ConcurrentQueue<Action> Actions = new();
	private static int _mainThreadId;

	public static void Initialize()
	{
		_mainThreadId = Thread.CurrentThread.ManagedThreadId;
	}

	public static void RunPending()
	{
		while (Actions.TryDequeue(out var action)) action();
	}

	public static bool IsMainThread => Environment.CurrentManagedThreadId == _mainThreadId;

	public static void Post(Action action)
	{
		Actions.Enqueue(action);
	}

	public static Task SwitchToMainThread()
	{
		if (IsMainThread) return Task.CompletedTask;

		var tcs = new TaskCompletionSource();
		Post(() => tcs.SetResult());
		return tcs.Task;
	}
}