using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CanvasUpdateComponent : MonoBehaviour
{
	private Text mLatitudeText = null;
	private Text mLongitudeText = null;

	void Awake()
	{
		mLatitudeText = GameObjectUtil.FindComponentFromGameObject<Text>("UserLatitude");
		if(null != mLatitudeText)
		{
			mLatitudeText.text = FakeInputManager.Instance.Location.x.ToString();
		}

		mLongitudeText = GameObjectUtil.FindComponentFromGameObject<Text>("UserLongitude");
		if(null != mLongitudeText)
		{
			mLongitudeText.text = FakeInputManager.Instance.Location.y.ToString();
		}

		FakeInputManager.LocationChanged += HandleLocationChanged;
	}

	void OnApplicationQuit()
	{
		FakeInputManager.LocationChanged -= HandleLocationChanged;
	}

	void HandleLocationChanged(Vector2 location)
	{
		if(null != mLatitudeText)
		{
			mLatitudeText.text = location.x.ToString();
		}

		if(null != mLongitudeText)
		{
			mLongitudeText.text = location.y.ToString();
		}
	}
}
