using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game;

public static class NetUtility
{
	public static string EncodeVector(Vector3 vec)
	{
		return string.Format("{0}:{1}:{2}",
			vec.x.ToString("R"),
			vec.y.ToString("R"),
			vec.z.ToString("R")
		);
	}

	public static Vector3 DecodeVector(string msg)
	{
		string[] args = msg.Split(':');
		return new Vector3(
			float.Parse(args[0]),
			float.Parse(args[1]),
			float.Parse(args[2])
		);
	}

	public static string EncodeQuaternion(Quaternion quat)
	{
		return string.Format("{0}:{1}:{2}:{3}",
			quat.x.ToString("R"),
			quat.y.ToString("R"),
			quat.z.ToString("R"),
			quat.w.ToString("R")
		);
	}

	public static Quaternion DecodeQuaternion(string msg)
	{
		string[] args = msg.Split(':');
		return new Quaternion(
			float.Parse(args[0]),
			float.Parse(args[1]),
			float.Parse(args[2]),
			float.Parse(args[3])
		);
	}
}
