using UnityEngine;
using System.Collections;
using System.ComponentModel;
using System.Linq;

public class NetworkClientManager : MonoBehaviour
{
	//private static string GAMEMODENAME = "TESTMODE";
	private static string GAMETYPENAME = "VFCONGAME";

	private static NetworkClientManager sInstance = null;
	public static NetworkClientManager Instance
	{
		get { return sInstance; }
	}
	
	public delegate void StateChange(ConnectionState state);
	public static StateChange StateChanged;
	
	public enum ConnectionState
	{
		Failed			= -1,
		Disconnected	= 0,
		Connecting,
		Connected
	}
	
	[DefaultValue(ConnectionState.Disconnected)]
	public ConnectionState State { get; private set; }
	
	public bool IsConnected
	{
		get { return (State == ConnectionState.Connected); }
	}
	
	void Awake()
	{
		if(null == sInstance)
		{
			sInstance = this;
		}

		InvokeRepeating("Tick", 0.0f, 1.0f);
	}
	
	void Connect()
	{
		SetState(ConnectionState.Connecting);
		try
		{
			MasterServer.RequestHostList(GAMETYPENAME);
			var hosts = MasterServer.PollHostList();
			if(null == hosts || hosts.Length == 0)
			{
				Debug.Log("Connection error: No hosts found");
				SetState(ConnectionState.Disconnected);
				return;
			}

			// Only attempt the first host
			var targetHost = hosts.SingleOrDefault();
			if(null == targetHost)
			{
				Debug.LogError("Connection error: Host entry is invalid!");
				SetState(ConnectionState.Disconnected);
				return;
			}

			Debug.LogFormat("Connecting to: {0} {1}", targetHost.gameType, targetHost.gameName);
			var result = Network.Connect(targetHost);
			if(NetworkConnectionError.NoError != result)
			{
				Debug.Log("Connection error: " + result.ToString());
				SetState(ConnectionState.Disconnected);
			}
		}
		catch(UnityException ex)
		{
			Debug.LogError("Connection exception: " + ex.ToString());
			SetState(ConnectionState.Failed);
		}
	}
	
	void Disconnect()
	{
		if(State == ConnectionState.Connected || State == ConnectionState.Connecting)
		{
			SetState(ConnectionState.Disconnected);
			try
			{
				Network.Disconnect();
			}
			catch(UnityException ex)
			{
				Debug.LogError("Error on disconnect: " + ex.ToString());
			}
		}
	}
	
	void OnApplicationQuit()
	{
		CancelInvoke("Tick");

		if(null != sInstance)
		{
			Disconnect();
			sInstance = null;
		}
	}

	void OnConnectedToServer()
	{
		if(State == ConnectionState.Connecting)
		{
			SetState(ConnectionState.Connected);
		}
		else
		{
			Debug.LogError("Connected in bad state!");
			Disconnect();
		}
	}
	
	void OnDisconnectedFromServer(NetworkDisconnection info)
	{
		if(State != ConnectionState.Disconnected)
		{
			Debug.Log("Disconnected from server: " + info.ToString());
			SetState(ConnectionState.Disconnected);
		}
	}

	void OnFailedToConnect(NetworkConnectionError error)
	{
		if(State != ConnectionState.Connecting)
		{
			Debug.LogError("Failed to connect in bad state!");
		}

		Debug.Log("Connection error: " + error.ToString());
		SetState(ConnectionState.Disconnected);
	}

	void OnMasterServerEvent(MasterServerEvent mse)
	{
		Debug.Log("Master Server Event: " + mse.ToString());
	}

	void SetState(ConnectionState state)
	{
		if(state == State)
		{
			return;
		}
		
		Debug.Log("Client state: " + state.ToString());
		State = state;
		if(null != StateChanged)
		{
			StateChanged(state);
		}
	}

	void Tick()
	{
		if(ConnectionState.Disconnected == State)
		{
			Connect();
		}
	}
}
