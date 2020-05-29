using UnityEngine;

/// <summary>
/// Be aware this will not prevent a non singleton constructor
///   such as `T myT = new T();`
/// To prevent that, add `protected T () {}` to your singleton class.
/// 
/// As a note, this is made as MonoBehaviour because we need Coroutines.
/// <see cref="http://wiki.unity3d.com/index.php/Singleton"/>
/// </summary>
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
	/// <summary>
	/// Instance of the singleton object.
	/// </summary>
	private static T instanceObject;

	/// <summary>
	/// Used to prevent data races.
	/// </summary>
	private static object instanceLock = new object();

	/// <summary>
	/// Access singleton.
	/// </summary>
	/// <returns>Singleton object.</returns>
	public static T Instance
	{
		get
		{
			if (applicationIsQuitting)
			{
				Debug.LogWarning("[Singleton] Instance '" + typeof(T) +
					"' already destroyed on application quit." +
					" Won't create again - returning null.");
				return null;
			}

			lock (instanceLock)
			{
				if (instanceObject == null)
				{
					instanceObject = (T)FindObjectOfType(typeof(T));

					if (FindObjectsOfType(typeof(T)).Length > 1)
					{
						Debug.LogError("[Singleton] Something went really wrong " +
							" - there should never be more than 1 singleton!" +
							" Reopening the scene might fix it.");
						return instanceObject;
					}

					if (instanceObject == null)
					{
						GameObject singleton = new GameObject();
						instanceObject = singleton.AddComponent<T>();
						singleton.name = "(singleton) " + typeof(T).ToString();

						DontDestroyOnLoad(singleton);

					}
				}

				return instanceObject;
			}
		}
	}

	/// <summary>
	/// Member variable to detect if the singleton was already destroyed.
	/// </summary>
	private static bool applicationIsQuitting = false;

	/// <summary>
	/// When Unity quits, it destroys objects in a random order.
	/// In principle, a Singleton is only destroyed when application quits.
	/// If any script calls Instance after it have been destroyed, 
	///   it will create a buggy ghost object that will stay on the Editor scene
	///   even after stopping playing the Application. Really bad!
	/// So, this was made to be sure we're not creating that buggy ghost object.
	/// </summary>
	public void OnDestroy()
	{
		applicationIsQuitting = true;
	}
}