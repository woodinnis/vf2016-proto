using UnityEngine;
using System.Collections;
using System.ComponentModel;

public class NetworkServerManager : MonoBehaviour
{
	private static int MAXCLIENTS = 16;
	private static int PORT = 5000;
	private static string GAMEMODENAME = "TESTMODE";
	private static string GAMETYPENAME = "VFCONGAME";

	private static NetworkServerManager sInstance = null;
	public static NetworkServerManager Instance
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
	}

	void Connect()
	{
		SetState(ConnectionState.Connecting);
		try
		{
			var result = Network.InitializeServer(MAXCLIENTS, PORT, false);
			if(NetworkConnectionError.NoError == result)
			{
				MasterServer.RegisterHost(GAMETYPENAME, GAMEMODENAME);
				SetState(ConnectionState.Connected);
			}
			else
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
		if(ConnectionState.Connected == State || ConnectionState.Connecting == State)
		{
			// Force-close all active connections!
			foreach(var connection in Network.connections)
			{
				try
				{
					Network.CloseConnection(connection, true);
				}
				catch(UnityException ex)
				{
					Debug.LogErrorFormat("Failed to close connection {0}: {1}", connection.ToString(), ex.ToString());
				}
			}

			try
			{
				// TODO Apparently, this doesn't work right?
				MasterServer.UnregisterHost();
				Network.Disconnect();
			}
			catch(UnityException ex)
			{
				Debug.LogError("Error on disconnect: " + ex.ToString());
			}
			SetState(ConnectionState.Disconnected);
		}
	}

	void OnApplicationQuit()
	{
		if(null != sInstance)
		{
			Disconnect();
			sInstance = null;
		}
	}

	void OnDisconnectedFromServer(NetworkDisconnection info)
	{
		if(State != ConnectionState.Disconnected && NetworkDisconnection.LostConnection == info)
		{
			Debug.Log("Server lost connection");
			SetState(ConnectionState.Disconnected);
		}
	}

	void OnMasterServerEvent(MasterServerEvent mse)
	{
		Debug.Log("Master Server Event: " + mse.ToString());
	}

	void OnPlayerConnected(NetworkPlayer player)
	{
		if(!IsConnected)
		{
			Debug.LogError("Player connected in bad state!");
			return;
		}

		Debug.Log("Player connected: " + player.ToString());
	}

	void OnPlayerDisconnected(NetworkPlayer player)
	{
		if(!IsConnected)
		{
			Debug.LogError("Player disconnected in bad state!");
			return;
		}

		Debug.Log("Player disconnected: " + player.ToString());
	}

	void SetState(ConnectionState state)
	{
		if(state == State)
		{
			return;
		}

		Debug.Log("Server state: " + state.ToString());
		State = state;
		if(null != StateChanged)
		{
			StateChanged(state);
		}
	}
	
	void Update()
	{
		if(ConnectionState.Disconnected == State)
		{
			Connect();
		}
	}
}
