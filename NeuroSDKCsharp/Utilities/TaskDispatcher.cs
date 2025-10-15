using System.Collections.Concurrent;

namespace NeuroSDKCsharp.Utilities;

/// <summary>
/// This is here to replicate UniTask's SwitchToMainThread
/// </summary>
internal static class TaskDispatcher
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

	private static bool IsMainThread => Environment.CurrentManagedThreadId == _mainThreadId;

	private static void Post(Action action)
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