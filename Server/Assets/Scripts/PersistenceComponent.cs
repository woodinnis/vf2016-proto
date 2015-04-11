using UnityEngine;
using System.Collections;

/// <summary>
/// Persistence component.
/// Keeps the parent game object alive for the duration of the application.
/// This should be used sparingly, and only on static instance anchors.
/// </summary>
public class PersistenceComponent : MonoBehaviour
{
	void Awake()
	{
		DontDestroyOnLoad(this.gameObject);
	}

	void OnApplicationQuit()
	{
		Destroy(this.gameObject);
	}
}
