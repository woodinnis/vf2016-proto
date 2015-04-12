using UnityEngine;
using System.Collections;

/// <summary>
/// Utility methods for dealing with Unity objects
/// </summary>
public static class GameObjectUtil
{
	#region GameObject

	public static T FindComponentFromGameObject<T>(string objectName)
	{
		var go = GameObject.Find(objectName);
		if(null == go)
		{
			Debug.LogErrorFormat("Unable to find {0} object", objectName);
			return default(T);
		}
		
		return go.GetComponent<T>();
	}

	#endregion
}
