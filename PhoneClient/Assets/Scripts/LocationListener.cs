using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LocationListener : MonoBehaviour
{
	private Text mLatitudeText = null;
	private Text mLongitudeText = null;
	private Text mStatusText = null;

	void Awake()
	{
		FindUIElements();

		LocationManager.OnLocationStateChanged += HandleOnLocationStateChanged;
		LocationManager.OnLocationUpdate += HandleOnLocationUpdate;
	}

	void FindUIElements()
	{
		mStatusText = FindChildObjectComponent<Text>("StatusText");
		if(null == mStatusText)
		{
			Debug.LogError("Unable to find StatusText UI.Text");
		}
		else
		{
			SetStatusText("Unknown");
		}

		mLatitudeText = FindChildObjectComponent<Text>("LatitudeText");
		if(null == mLatitudeText)
		{
			Debug.LogError("Unable to find LatitudeText UI.Text");
		}
		else
		{
			mLatitudeText.text = string.Empty;
		}

		mLongitudeText = FindChildObjectComponent<Text>("LongitudeText");
		if(null == mLongitudeText)
		{
			Debug.LogError("Unable to find LongitudeText UI.Text");
		}
		else
		{
			mLongitudeText.text = string.Empty;
		}
	}

	// TODO: Make this a util thing!
	T FindChildObjectComponent<T>(string objectName)
	{
		var go = GameObject.Find(objectName);
		if(null == go)
		{
			Debug.LogErrorFormat("Unable to find {0} object", objectName);
			return default(T);
		}
		
		return go.GetComponent<T>();
	}

	void HandleOnLocationStateChanged (LocationManager.ListeningState state)
	{
		SetStatusText(state.ToString());
		if(null != mLatitudeText)
		{
			mLatitudeText.text = string.Empty;
		}
		if(null != mLongitudeText)
		{
			mLongitudeText.text = string.Empty;
		}
	}

	void HandleOnLocationUpdate (LocationInfo info)
	{
		if(null != mLatitudeText)
		{
			mLatitudeText.text = info.latitude.ToString();
		}
		if(null != mLongitudeText)
		{
			mLongitudeText.text = info.longitude.ToString();
		}
	}

	void OnApplicationDestroyed()
	{
		LocationManager.OnLocationStateChanged -= HandleOnLocationStateChanged;
	}

	void SetStatusText(string status)
	{
		if(null == mStatusText)
		{
			return;
		}

		if(!mStatusText.text.Equals(status))
		{
			mStatusText.text = status;
		}
	}
}
