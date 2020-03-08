/// Copyright (c) 2011, Ken Rockot  <k-e-n-oz.gs>.  All rights reserved.
/// Everyone is granted non-exclusive license to do anything at all with this code.
/// Feat. 2019, Demétrius Nunes Alves.
using System;
using System.Collections;

namespace Game
{
	/// <summary>
	/// A Task object represents a coroutine.
	/// <para>Tasks can be started, paused, and stopped.</para>
	/// It is an error to attempt to start a task that has been stopped or which has
	/// naturally terminated.
	/// </summary>
	public class UnityTask
	{
		/// <summary>
		/// Returns true if and only if the coroutine is running.
		/// <para>Paused tasks are considered to be running.</para>
		/// </summary>
		public bool Running
		{
			get
			{
				return task.Running;
			}
		}

		/// <summary>
		/// Returns true if and only if the coroutine is currently paused.
		/// </summary>
		public bool Paused
		{
			get
			{
				return task.Paused;
			}
		}

		/// <summary>
		/// Delegate for termination subscribers.
		/// <para>Manual is true if and only if the coroutine was stopped with an explicit call to <see cref="Stop"/>.</para>
		/// </summary>
		/// <param name="manual">Returns true if the coroutine was stopped and false if naturaly completed.</param>
		public delegate void FinishedHandler(bool manual);

		/// <summary>
		/// Termination event.  
		/// <para>Triggered when the coroutine completes execution.</para>
		/// </summary>
		public event FinishedHandler Finished;

		/// <summary>
		/// Creates a new Task object for the given coroutine.
		/// <para>If autoStart is true (default) the task is automatically started upon construction.</para>
		/// </summary>
		/// <param name="coroutine">The coroutine method to run.</param>
		/// <param name="autoStart">If the coroutine should start right after the creation.</param>
		public UnityTask(IEnumerator coroutine, bool autoStart = true)
		{
			task = UnityTaskManager.CreateTask(coroutine);
			task.Finished += TaskFinished;
			if (autoStart)
			{
				Start();
			}
		}

		/// <summary>
		/// Waits for a number of frame then runs an action.
		/// </summary>
		/// <param name="delay">Number of frames to wait.</param>
		/// <param name="action">Action to execute.</param>
		public static void DelayedAction(int delay, Action action)
		{
			new UnityTask(WaitAndDo(delay, action));
		}

		/// <summary>
		/// Waits for a number of seconds then runs an action.
		/// </summary>
		/// <param name="seconds">Number of seconds to wait.</param>
		/// <param name="action">Action to execute.</param>
		public static void DelayedAction(float seconds, Action action)
		{
			new UnityTask(WaitAndDo(seconds, action));
		}

		/// <summary>
		/// Waits for a number of frame then run an action.
		/// </summary>
		/// <param name="delay">Number of frames to wait.</param>
		/// <param name="action">Action to execute.</param>
		/// <returns>IEnumerator needed for a coroutine.</returns>
		private static IEnumerator WaitAndDo(int delay, Action action)
		{
			while (delay > 0)
			{
				--delay;
				yield return null;
			}
			action();
		}

		/// <summary>
		/// Waits for a number of seconds then run an action.
		/// </summary>
		/// <param name="seconds">Delay in seconds. </param>
		/// <param name="action">Action to execute.</param>
		/// <returns>IEnumerator needed for a coroutine.</returns>
		private static IEnumerator WaitAndDo(float seconds, Action action)
		{
			yield return new UnityEngine.WaitForSeconds(seconds);
			action();
		}

		/// <summary>
		/// Makes the coroutine run.
		/// </summary>
		public void Start()
		{
			task.Start();
		}

		/// <summary>
		/// Discontinues execution of the coroutine at its next yield.
		/// </summary>
		public void Stop()
		{
			task.Stop();
		}

		/// <summary>
		/// Pauses the execution of a coroutine.
		/// </summary>
		public void Pause()
		{
			task.Pause();
		}

		/// <summary>
		/// Unpauses a coroutine.
		/// </summary>
		public void Unpause()
		{
			task.Unpause();
		}

		/// <summary>
		/// Pass the finish delegate to the Task Manager.
		/// </summary>
		/// <param name="manual">If the task was forced to finish.</param>
		private void TaskFinished(bool manual)
		{
			FinishedHandler handler = Finished;
			if (handler != null)
			{
				handler(manual);
			}
		}

		private UnityTaskManager.TaskState task;
	}
}