using UnityEngine;
using System.Collections;

public class GameplayServerManager : MonoBehaviour
{
	private NetworkView mView = new NetworkView();
	
	void Awake()
	{
		mView = this.gameObject.GetComponent<NetworkView>();
	}

	#region Client RPC
	
	#endregion
	
	#region Server RPC
	
	[RPC]
	void OnClientLocationUpdate(float latitute, float longitude, NetworkMessageInfo info)
	{
		Debug.LogFormat("Client Update {0} {1} {2}", info.sender.ToString(), latitute, longitude);
	}
	
	#endregion
}
