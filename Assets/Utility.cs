
using Game;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class Utility
{
	public static Color lineColor = Color.black;
	///<summary>Start [inclusive] and end [exclusive].</summary>
	public static bool InsideRange(int x, int startX, int endX, int y, int startY, int endY)
	{
		return x >= startX && x < endX && y >= startY && y < endY;
	}
	public static void SetCanvasGroup(CanvasGroup canvasGroup, bool enable)
	{
		canvasGroup.alpha = enable ? 1 : 0;
		canvasGroup.interactable = enable;
		canvasGroup.blocksRaycasts = enable;
	}
	public static void SetAlpha(Graphic graphic, float alpha)
	{
		graphic.color = new Color(graphic.color.r, graphic.color.g, graphic.color.b, alpha);
	}
	public static void SetAlpha(SpriteRenderer spriteRenderer, float alpha)
	{
		Color color = spriteRenderer.color;
		color.a = alpha;
		spriteRenderer.color = color;

	}
	public static string TintString(string text, Color color)
	{
		return "<color=#" + ColorUtility.ToHtmlStringRGBA(color) + ">" + text + "</color>";
	}
	public static string TintString(string text, string hex)
	{
		return "<color=" + hex + ">" + text + "</color>";
	}

	public static IEnumerator HighlightMesh(Renderer[] renderers, float duration)
	{
		Color[] defaultColors = new Color[renderers.Length];

		for (int i = 0; i < renderers.Length; i++)
		{
			if (!renderers[i])
				yield break;

			defaultColors[i] = renderers[i].sharedMaterial.GetColor("_EmissionColor");
			renderers[i].material.SetColor("_EmissionColor", Color.white);

		}

		yield return new WaitForSeconds(duration);

		for (int i = 0; i < renderers.Length; i++)
		{
			if (!renderers[i])
				yield break;

			renderers[i].material.SetColor("_EmissionColor", defaultColors[i]);
		}
	}
	public static IEnumerator FadeIn(CanvasGroup canvasGroup, float fadeDuration)
	{
		float timer = 0;

		while (timer < 1)
		{
			timer += Time.deltaTime / fadeDuration;

			canvasGroup.alpha = timer;

			yield return null;
		}
		SetCanvasGroup(canvasGroup, true);
	}
	public static IEnumerator FadeIn(Image image, float fadeDuration)
	{
		float timer = 0;

		while (timer < 1)
		{
			timer += Time.deltaTime / fadeDuration;

			SetAlpha(image, timer);

			yield return null;
		}
	}
	public static IEnumerator FadeOut(CanvasGroup canvasGroup, float fadeDuration)
	{
		float timer = 1;

		while (timer > 0)
		{
			timer -= Time.deltaTime / fadeDuration;

			canvasGroup.alpha = timer;

			yield return null;
		}
		SetCanvasGroup(canvasGroup, false);
	}
	public static IEnumerator FadeOut(Image image, float fadeDuration)
	{
		float timer = 1;

		while (timer > 0)
		{
			timer -= Time.deltaTime / fadeDuration;

			SetAlpha(image, timer);

			yield return null;
		}
	}
	public static Color GetRandomColor()
	{
		Color color = new Color(0.5f, 0.5f, 0.5f);

		int i = Random.Range(0, 3);


		if (i == 0)
			color.r = 0;
		else if (i == 1)
			color.g = 0;
		else if (i == 2)
			color.b = 0;

		i = Random.Range(0, 3);

		if (i == 0)
			color.r = 1;
		else if (i == 1)
			color.g = 1;
		else if (i == 2)
			color.b = 1;


		else if (color.r == 0.5f)
			color.r = Random.Range(0f, 1f);
		else if (color.g == 0.5f)
			color.g = Random.Range(0f, 1f);
		else if (color.b == 0.5f)
			color.b = Random.Range(0f, 1f);

		return color;

	}
	public static float ParseFloat(string valStr)
	{
		return float.Parse(valStr, CultureInfo.InvariantCulture);
	}
	public static string ToString(float value)
	{
		return value.ToString("0.#########", CultureInfo.InvariantCulture);
	}
	private static LineRenderer CreateLineRenderer(GameObject container)
	{
		LineRenderer line = container.GetComponentInChildren<LineRenderer>();
		if (!line)
		{
			line = GameObject.Instantiate(new GameObject("LineRenderer"), container.transform.position, Quaternion.identity).AddComponent<LineRenderer>();
			line.transform.SetParent(container.transform);
		}
		return line;
	}
	public static void DrawCircle(GameObject container, float radius, float lineWidth = 0.05f)
	{
		float x = 0;
		float y = 0.4f;
		float z = 0;

		int segments = 30;

		LineRenderer line = CreateLineRenderer(container);

		line.material = new Material(Shader.Find("UI/Default"));
		line.useWorldSpace = false;
		line.startWidth = lineWidth;
		line.endWidth = lineWidth;
		line.positionCount = segments + 2;
		line.startColor = lineColor;
		line.endColor = lineColor;

		float angle = 360f / segments;

		for (int i = 0; i < line.positionCount - 1; i++)
		{
			x = Mathf.Sin(Mathf.Deg2Rad * angle * i) * radius;
			z = Mathf.Cos(Mathf.Deg2Rad * angle * i) * radius;

			line.SetPosition(i, new Vector3(x, y, z));
		}

		line.SetPosition(line.positionCount - 1, Vector3.zero);
	}
	public static void DebugCircle(Vector3 position, float radius)
	{
		float x = 0;
		float y = position.y + 0.4f;
		float z = 0;


		int segments = (int)(radius * 4);

		float angle = 360f / segments;

		Vector3 lastPosition = Vector3.zero;

		for (int i = 0; i < segments + 1; i++)
		{
			x = position.x + Mathf.Sin(Mathf.Deg2Rad * angle * i) * radius;
			z = position.z + Mathf.Cos(Mathf.Deg2Rad * angle * i) * radius;

			if (i == 0)
			{
				lastPosition = new Vector3(x, y, z);
			}
			else
			{
				Debug.DrawLine(lastPosition, new Vector3(x, y, z), Color.cyan);
				lastPosition = new Vector3(x, y, z);
			}
		}
	}
	public static IEnumerator DebugCircle(Vector3 position, float radius, float duration)
	{
		float x = 0;
		float y = position.y + 0.4f;
		float z = 0;


		int segments = (int)(radius * 6);

		float angle = 360f / segments;

		Vector3 lastPosition = Vector3.zero;

		float timer = duration;
		while (timer > 0)
		{
			timer -= Time.deltaTime;

			for (int i = 0; i < segments + 1; i++)
			{
				x = position.x + Mathf.Sin(Mathf.Deg2Rad * angle * i) * radius;
				z = position.z + Mathf.Cos(Mathf.Deg2Rad * angle * i) * radius;

				if (i == 0)
				{
					lastPosition = new Vector3(x, y, z);
				}
				else
				{
					Debug.DrawLine(lastPosition, new Vector3(x, y, z), Color.cyan);
					lastPosition = new Vector3(x, y, z);
				}
			}
			yield return null;
		}
	}
	public static void DrawLine(GameObject container, Vector3 endPosition, float lineWidth = 0.05f)
	{
		LineRenderer line = container.AddComponent<LineRenderer>();
		line.material = new Material(Shader.Find("UI/Default"));
		line.useWorldSpace = false;
		line.startWidth = lineWidth;
		line.endWidth = lineWidth;
		line.positionCount = 2;
		line.startColor = Color.white;
		line.endColor = Color.white;

		line.SetPosition(0, container.transform.position);
		line.SetPosition(1, endPosition);
	}
	public static bool CheckEnumType(System.Enum a, System.Enum b)
	{
		return a.Equals(b);
	}
	public static void EncodeVector2(Vector2 vector2, ref StringBuilder str)
	{
		str.AppendFormat(
			"{0} {1} ",
			ToString(vector2.x),
			ToString(vector2.y)
		);
	}
	public static void DecodeVector2(string[] tokens, out Vector2 vector)
	{
		if (tokens.Length != 2)
		{
			Debug.LogError("Wring number of element to be parsed.");
		}
		vector = new Vector2(
			ParseFloat(tokens[0]),
			ParseFloat(tokens[1])
		);
	}
	public static void EncodeVector3(Vector3 vector3, ref StringBuilder str)
	{
		str.AppendFormat(
			"{0} {1} {2} ",
			ToString(vector3.x),
			ToString(vector3.y),
			ToString(vector3.z)
		);
	}
	public static void DecodeVector3(string[] tokens, out Vector3 vector)
	{
		if (tokens.Length != 3)
		{
			Debug.LogError("Wring number of element to be parsed.");
		}
		vector = new Vector3(
			ParseFloat(tokens[0]),
			ParseFloat(tokens[1]),
			ParseFloat(tokens[2])
		);
	}
	public static void EncodeQuaternion(Quaternion quaternion, ref StringBuilder str)
	{
		str.AppendFormat(
			"{0} {1} {2} {3} ",
			ToString(quaternion.x),
			ToString(quaternion.y),
			ToString(quaternion.z),
			ToString(quaternion.w)
		);
	}
	public static void DecodeQuaternion(string[] tokens, out Quaternion quaternion)
	{
		if (tokens.Length != 4)
		{
			Debug.LogError("Wring number of element to be parsed.");
		}
		quaternion = new Quaternion(
			ParseFloat(tokens[0]),
			ParseFloat(tokens[1]),
			ParseFloat(tokens[2]),
			ParseFloat(tokens[3])
		);
	}
#if UNITY_EDITOR
	public static T[] FindAssetsByType<T>(params string[] folders) where T : Object
	{
		string type = typeof(T).ToString().Replace("UnityEngine.", "");

		string[] guids;
		if (folders == null || folders.Length == 0)
		{
			guids = AssetDatabase.FindAssets("t:" + type);
		}
		else
		{
			guids = AssetDatabase.FindAssets("t:" + type, folders);
		}

		T[] assets = new T[guids.Length];

		for (int i = 0; i < guids.Length; i++)
		{
			string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
			assets[i] = AssetDatabase.LoadAssetAtPath<T>(assetPath);
		}
		return assets;
	}
#endif
}
