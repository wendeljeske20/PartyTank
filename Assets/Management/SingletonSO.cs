using UnityEngine;
using System.IO;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Wrapper to instance of the single scriptable objects.
/// </summary>
/// <typeparam name="T">Heir type.</typeparam>
public class SingletonSO<T> : ScriptableObject where T : ScriptableObject
{
	/// <summary>
	/// Reference to instance of the <see cref="T"/>.
	/// <para>Get: If <see cref="instance"/> is null, try load this instance.</para>
	/// </summary>
	public static T Instance
	{
		get
		{
			if (instance == null)
			{
				instance = Load();
			}
			return instance;
		}
	}

	/// <summary>
	/// Reference to instance of the <see cref="T"/>.
	/// </summary>
	private static T instance;

	/// <summary>
	/// Retuirn if instance is null or not.
	/// </summary>
	private static bool InstanceIsNull
	{
		get
		{
			return Instance == null;
		}
	}

	/// <summary>
	/// Asset load method.
	/// </summary>
	/// <returns>Instance if exists in the project.</returns>
	private static T Load()
	{
		T asset = Resources.Load<T>(typeof(T).Name);
		if (asset == null)
		{
			Debug.LogError("The resource " + typeof(T).Name + " could not be loaded.");
		}
		return asset;
	}

	#region Editor
#if UNITY_EDITOR

	/// <summary>
	/// Reference to editor window created, based on <see cref="instance"/>.
	/// </summary>
	private static EditorWindow inspectorWindow;

	/// <summary>
	/// Default minimum size.
	/// </summary>
	private readonly static Vector2 defaultMinSize = new Vector2(500, 700);

	/// <summary>
	/// Default maximum size.
	/// </summary>
	private readonly static Vector2 defaultMaxSize = new Vector2(500, 1000);

	/// <summary>
	/// Create asset with path.
	/// </summary>
	/// <param name="path">path of the asset.</param>
	/// <returns>Return instance created.</returns>
	private static Object CreateAsset(string path)
	{
		Object instance = null;
		string filePath = path + "/" + typeof(T).Name + ".asset";
		if (!File.Exists(filePath))
		{
			instance = CreateInstance(typeof(T));
			AssetDatabase.CreateAsset(instance, filePath);
		}
		else
		{
			instance = AssetDatabase.LoadAssetAtPath(filePath, typeof(T));
			Debug.LogWarning("Already found Data Asset on Internal folder, using that instead of a new one!");
		}
		return instance;
	}

	/// <summary>
	/// Creates a instance of <see cref="T"/> type.
	/// </summary>
	private static void Create()
	{
		if (Application.isPlaying)
		{
			Debug.LogWarning("Aborting tool creation while Playing.");
			return;
		}

		if (!ValidateCreate())
		{
			return;
		}

		CreateAsset(AssetDatabase.GetAssetPath(Selection.activeObject));

		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();

		FocusInstanceOnProjectWindow();
		Debug.Log(ObjectNames.NicifyVariableName(typeof(T).Name) + " asset instance successfully created.", Instance);
	}

	/// <summary>
	/// Validation in the creation of the instance.
	/// </summary>
	/// <returns>Return if the asset can be created.</returns>
	private static bool ValidateCreate()
	{
		if (Selection.activeObject.GetType() != typeof(DefaultAsset))
		{
			Debug.LogError("The " + ObjectNames.NicifyVariableName(typeof(T).Name) + " asset is not of the DefaultAsset type.");
			return false;
		}
		if (!InstanceIsNull)
		{
			FocusInstanceOnProjectWindow();
			Debug.LogError("An instance of " + ObjectNames.NicifyVariableName(typeof(T).Name) + " asset already exists.", Instance);
			return false;
		}
		string[] path = AssetDatabase.GetAssetPath(Selection.activeObject).Split('/');
		if (!path[path.Length - 1].Equals("Resources"))
		{
			Debug.LogError(
				"Could not create an instance of asset "
				+ ObjectNames.NicifyVariableName(typeof(T).Name)
				+ " because it must be created inside a folder named \"Resources\".");
			return false;
		}
		return true;
	}

	/// <summary>
	/// Forces the project window to focus on an instance of this asset.
	/// </summary>
	private static void FocusInstanceOnProjectWindow()
	{
		System.Type projectBrowserType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.ProjectBrowser");
		EditorWindow projectBrowser = EditorWindow.GetWindow(projectBrowserType, true);
		inspectorWindow = CreateInspectorWindow();
		SelectInstance(inspectorWindow, true);

		EditorGUIUtility.PingObject(Instance);

		projectBrowser.Show();
		projectBrowser.Focus();

		inspectorWindow.Show();
		inspectorWindow.Focus();
	}

	/// <summary>
	/// Open a new window based on the instance of that asset.
	/// </summary>
	protected static void OpenWindow()
	{
		if (InstanceIsNull)
		{
			Debug.LogError("An instance of " + ObjectNames.NicifyVariableName(typeof(T).Name) + " asset does not exist.");
			return;
		}
		inspectorWindow = CreateInspectorWindow();
		SelectInstance(inspectorWindow, true);
		inspectorWindow.name = ObjectNames.NicifyVariableName(typeof(T).Name);
		inspectorWindow.titleContent = new GUIContent(ObjectNames.NicifyVariableName(typeof(T).Name));
		inspectorWindow.minSize = defaultMinSize;
		inspectorWindow.maxSize = defaultMaxSize;
		inspectorWindow.Show();
		inspectorWindow.Focus();
	}

	/// <summary>
	/// Create a new inspector window.
	/// </summary>
	/// <returns>Reference to new inspector window.</returns>
	private static EditorWindow CreateInspectorWindow()
	{
		return ScriptableObject.CreateInstance(
			typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.InspectorWindow"))
			as EditorWindow;
	}

	/// <summary>
	/// Get default inspector window.
	/// </summary>
	/// <returns> Reference to default inspector window.</returns>
	private static EditorWindow GetInspectorWindow()
	{
		return EditorWindow.GetWindow(
				typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.InspectorWindow"));
	}

	/// <summary>
	/// Forces a <see cref="EditorWindow"/> the focus this asset's instance.
	/// </summary>
	/// <param name="editorWindow">Reference to <see cref="EditorWindow"/> modified.</param>
	/// <param name="isLocked">Locks window.</param>
	private static void SelectInstance(EditorWindow editorWindow, bool isLocked = false)
	{
		PropertyInfo property = editorWindow.GetType().GetProperty("isLocked");
		property.SetValue(editorWindow, false);
		Object active = Selection.activeObject;
		Selection.activeObject = Instance;
		property.SetValue(editorWindow, isLocked);
		Selection.activeObject = active;
	}
#endif
	#endregion
}