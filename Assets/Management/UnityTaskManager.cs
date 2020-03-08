using System.Collections;
using UnityEngine;

namespace Game
{
	class UnityTaskManager : MonoBehaviour
	{
		public class TaskState
		{
			public bool Running
			{
				get { return running; }
			}

			public bool Paused
			{
				get { return paused; }
			}

			public delegate void FinishedHandler(bool manual);
			public event FinishedHandler Finished;

			private IEnumerator coroutine;
			private bool running;
			private bool paused;
			private bool stopped;

			public TaskState(IEnumerator c)
			{
				coroutine = c;
			}

			public void Pause()
			{
				paused = true;
			}

			public void Unpause()
			{
				paused = false;
			}

			public void Start()
			{
				running = true;
				singleton.StartCoroutine(CallWrapper());
			}

			public void Stop()
			{
				stopped = true;
				running = false;
			}

			IEnumerator CallWrapper()
			{
				yield return null;
				IEnumerator e = coroutine;
				while (running)
				{
					if (paused)
						yield return null;
					else
					{
						if (e != null && e.MoveNext())
						{
							yield return e.Current;
						}
						else
						{
							running = false;
						}
					}
				}

				FinishedHandler handler = Finished;
				if (handler != null)
					handler(stopped);
			}
		}

		static UnityTaskManager singleton;

		public static TaskState CreateTask(IEnumerator coroutine)
		{
			if (singleton == null)
			{
				GameObject go = new GameObject("UnityTaskManager");
				singleton = go.AddComponent<UnityTaskManager>();
			}
			return new TaskState(coroutine);
		}
	}
}