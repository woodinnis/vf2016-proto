using UnityEngine;
using System.Collections;

public class GameplayClientManager : MonoBehaviour
{
	private NetworkView mView = null;

	void Awake()
	{
		NetworkClientManager.StateChanged += HandleStateChanged;
		mView = this.gameObject.GetComponent<NetworkView>();
	}

	void HandleStateChanged(NetworkClientManager.ConnectionState state)
	{
		if(NetworkClientManager.ConnectionState.Connected == state)
		{
			InvokeRepeating("Tick", 0, 1);
		}
		else
		{
			CancelInvoke("Tick");
		}
	}

	void Tick()
	{
		mView.RPC("OnClientLocationUpdate", RPCMode.Others, 49.0f, -122.0f);
	}

	#region Client RPC

	#endregion

	#region Server RPC

	[RPC]
	void OnClientLocationUpdate(float latitute, float longitude, NetworkMessageInfo info) { }

	#endregion
}
