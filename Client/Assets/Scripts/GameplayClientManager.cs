using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GameplayClientManager : MonoBehaviour
{
	private static readonly float MaxSearchRadius = 50;
	private static readonly float MaxSearchRadiusSqr = MaxSearchRadius * MaxSearchRadius;

	private NetworkView mView = null;
	private Vector2 mLastPosition = new Vector2();

	private GameObject mCanvas = null;
	private CanvasScaler mCanvasScaler = null;
	private GameObject mUserMarker = null;

	private GameObject mPlayerDotPrefab = null;

	private Dictionary<string, GameObject> mClientSprites = new Dictionary<string, GameObject>();

	void Awake()
	{
		NetworkClientManager.StateChanged += HandleStateChanged;
		mView = this.gameObject.GetComponent<NetworkView>();

		mPlayerDotPrefab = Resources.Load("PlayerDot") as GameObject;
		if(null == mPlayerDotPrefab)
		{
			Debug.LogError("Unable to load PlayerDot prefab!");
		}

		mCanvas = GameObject.Find("Canvas");
		if(null != mCanvas)
		{
			mCanvasScaler = mCanvas.GetComponent<CanvasScaler>();
			if(null != mCanvasScaler && null != mPlayerDotPrefab)
			{
				mUserMarker = Instantiate(mPlayerDotPrefab) as GameObject;
				if(null != mUserMarker)
				{
					mUserMarker.name = "LocalPlayer";
					var renderer = mUserMarker.GetComponent<SpriteRenderer>();
					if(null != renderer)
					{
						// Make it orange!
						renderer.color = new Color(1, 0.6f, 0);
					}
				}
				else
				{
					Debug.LogError("Unable to create local player dot");
				}
			}
		}
	}

	void HandleStateChanged(NetworkClientManager.ConnectionState state)
	{
		if(NetworkClientManager.ConnectionState.Connected == state)
		{
			Reset();
			InvokeRepeating("Tick", 0, 1);
		}
		else
		{
			CancelInvoke("Tick");
		}
	}

	void OnApplicationQuit()
	{
		CancelInvoke("Tick");
		if(null != mUserMarker)
		{
			Destroy(mUserMarker);
			mUserMarker = null;
		}
	}

	void Reset()
	{
		mLastPosition = new Vector2(0, 0);
	}

	void Tick()
	{
		Vector2 currentPosition = FakeInputManager.Instance.Location;
		if(!mLastPosition.Equals(currentPosition))
		{
			mLastPosition.x = currentPosition.x;
			mLastPosition.y = currentPosition.y;

			mView.RPC("OnClientLocationUpdate", RPCMode.Others, mLastPosition.x, mLastPosition.y);
		}
	}

	#region Client RPC

	[RPC]
	void OnClientLocationUpdate(float latitute, float longitude, NetworkMessageInfo info)
	{
		// Ignore our own
		if(info.sender == Network.player)
		{
			Debug.LogWarning("How did we get our own location update?");
			return;
		}

		Vector2 clientPos = new Vector2(latitute, longitude);
		if((mLastPosition - clientPos).sqrMagnitude > MaxSearchRadiusSqr)
		{
			if(mClientSprites.ContainsKey(info.sender.guid))
			{
				//RemoveClient(info.sender.guid);
			}

			// Too far away!
			return;
		}
	}

	#endregion

	#region Server RPC

	#endregion
}
