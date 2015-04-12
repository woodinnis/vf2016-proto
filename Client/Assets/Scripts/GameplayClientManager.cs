using UnityEngine;
using System.Collections;

public class GameplayClientManager : MonoBehaviour
{
	private static readonly float MaxSearchRadius = 50;
	private static readonly float MaxSearchRadiusSqr = MaxSearchRadius * MaxSearchRadius;

	private NetworkView mView = null;
	private Vector2 mLastPosition = new Vector2();

	void Awake()
	{
		NetworkClientManager.StateChanged += HandleStateChanged;
		mView = this.gameObject.GetComponent<NetworkView>();
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
			// Too far away!
			return;
		}
	}

	#endregion

	#region Server RPC

	#endregion
}
