using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.ComponentModel;

public class GameplayClientManager : MonoBehaviour
{
	private static GameplayClientManager sInstance = null;
	public static GameplayClientManager Instance
	{
		get { return sInstance; }
	}

	private static readonly float MaxSearchRadius = 50;
	private static readonly float MaxSearchRadiusSqr = MaxSearchRadius * MaxSearchRadius;

	private NetworkView mView = null;
	private Vector2 mLastPosition = new Vector2();

	private GameObject mCanvas = null;
	private CanvasScaler mCanvasScaler = null;
	private GameObject mUserMarker = null;

	public float LocationScalar { get; private set; }

	private GameObject mPlayerDotPrefab = null;

	private Dictionary<string, GameObject> mClientSprites = new Dictionary<string, GameObject>();

	void Awake()
	{
		NetworkClientManager.StateChanged += HandleStateChanged;
		mView = this.gameObject.GetComponent<NetworkView>();

		if(null == sInstance)
		{
			sInstance = this;
		}

		LocationScalar = 1.0f;

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
				mUserMarker = CreateClientSprite("LocalPlayer", new Color(1, 0.6f, 0));
				if(null == mUserMarker)
				{
					Debug.LogError("Unable to create user maker");
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

		if(null != sInstance)
		{
			sInstance = null;
		}

		if(null != mUserMarker)
		{
			Destroy(mUserMarker);
			mUserMarker = null;
		}
	}

	void Reset()
	{
		mLastPosition = Vector2.zero;
	}

	void Tick()
	{
		Vector2 currentPosition = FakeInputManager.Instance.Location;
		if(!mLastPosition.Equals(currentPosition))
		{
			mLastPosition.Set(currentPosition.x, currentPosition.y);

			// TODO Do NOT do this! Set a global dirty flag instead!
			// See the large comment in SendClientNextUpdate() on how this SHOULD work
			foreach(var kvp in mClientSprites)
			{
				var client = kvp.Value.GetComponent<ClientSpriteComponent>();	// TODO CACHE THIS!!!
				if(null != client)
				{
					// This directly updates right now, which is BAD
					client.UpdateLocation(true);
				}
			}

			mView.RPC("OnClientLocationUpdate", RPCMode.Others, mLastPosition.x, mLastPosition.y);
		}
	}

	// Only do this for PC testing!
	#region Fake Client

	public void CreateFake(string guid)
	{
		if(mClientSprites.ContainsKey(guid))
		{
			Debug.LogWarning("Attempting to create duplicate fake: " + guid);
			return;
		}

		AddClient(guid);
	}

	public void DestroyFake(string guid)
	{
		if(!mClientSprites.ContainsKey(guid))
		{
			Debug.LogWarning("Attempting to destroy non-existant fake: " + guid);
			return;
		}

		RemoveClient(guid);
	}

	public void FakeUpdateClient(string guid, Vector2 location)
	{
		if(!mClientSprites.ContainsKey(guid))
		{
			return;
		}

		SendClientNextUpdate(guid, location.x, location.y);
	}

	#endregion

	#region Client

	void AddClient(string guid)
	{
		if(mClientSprites.ContainsKey(guid))
		{
			Debug.LogError("Attempting to add existing client: " + guid);
			return;
		}

		mClientSprites[guid] = CreateClientSprite(guid, Color.red);
	}

	GameObject CreateClientSprite(string name, Color color)
	{
		GameObject result = Instantiate(mPlayerDotPrefab) as GameObject;
		if(null != result)
		{
			var client = result.GetComponent<ClientSpriteComponent>();
			if(null != client)
			{
				client.SetupClient(name, color);
			}
			else
			{
				Debug.LogError("No ClientSpriteComponent on the prefab?");
				Destroy(result);
				return null;
			}
		}
		
		return result;
	}

	void RemoveClient(string guid)
	{
		if(!mClientSprites.ContainsKey(guid))
		{
			Debug.LogError("Attempting to remove non-existant client: " + guid);
			return;
		}

		var go = mClientSprites[guid];
		mClientSprites.Remove(guid);
		Destroy(go);
	}

	void SendClientNextUpdate(string guid, float latitude, float longitude)
	{
		if(!mClientSprites.ContainsKey(guid))
		{
			// Log and error message? Will this get spammy?
			return;
		}

		// TODO Okay, this is NOT going to stay like this!
		// What really needs to happen is this needs to get stored with the game object info
		// and then updated on the next frame for X number of sprites per frame.
		// This also means adding a dirty flag, that can get set for ALL when the player location,
		// changes, then going through a limited number of sprite updates per frame.
		// But that's a lot more work than just forcing the update now, and ...
		// I'm being lazy on this check in :)

		var go = mClientSprites[guid];
		var client = go.GetComponent<ClientSpriteComponent>();	// TODO CACHE THIS!!!
		if(null != client)
		{
			// This directly updates right now, which is BAD
			client.SetNextLocation(new Vector2(latitude, longitude));
			client.UpdateLocation(false);
		}
	}

	#endregion

	#region Client RPC

	// Only public to allow for fake client connection!
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
				RemoveClient(info.sender.guid);
			}

			// Too far away!
			return;
		}

		if(!mClientSprites.ContainsKey(info.sender.guid))
		{
			AddClient(info.sender.guid);
		}

		SendClientNextUpdate(info.sender.guid, latitute, longitude);
	}

	#endregion

	#region Server RPC

	#endregion
}
