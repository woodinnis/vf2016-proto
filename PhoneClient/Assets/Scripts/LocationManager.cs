using UnityEngine;
using System.Collections;
using System.ComponentModel;

public class LocationManager : MonoBehaviour
{
	private static LocationManager sInstance;
	public static LocationManager Instance
	{
		get { return sInstance; }
	}

	static readonly float kUpdateDelay = 1.0f;
	private float mDelayTime = 0.0f;

	public enum ListeningState
	{
		Disabled	= -1,
		Stopped		= 0,
		Starting,
		Running
	};

	public delegate void LocationStateChanged(ListeningState state);
	public static event LocationStateChanged OnLocationStateChanged;

	public delegate void LocationUpdate(LocationInfo info);
	public static event LocationUpdate OnLocationUpdate;

	[DefaultValue(ListeningState.Stopped)]
	ListeningState State { get; set; }

	void Awake()
	{
		if(null == sInstance)
		{
			sInstance = this;
		}
	}

	void OnApplicationDestroyed()
	{
		if(ListeningState.Starting == State || ListeningState.Running == State)
		{
			try
			{
				Input.location.Stop();
			}
			catch(UnityException ex)
			{
				Debug.LogError("Failed to stop location: " + ex.ToString());
			}
			SetState(ListeningState.Stopped);
		}

		if(this == sInstance)
		{
			sInstance = null;
		}
	}

	private void SetState(ListeningState state)
	{
		if(state == State)
		{
			return;
		}

		Debug.Log("GPS State: " + state);
		State = state;

		if(null != OnLocationStateChanged)
		{
			OnLocationStateChanged(State);
		}
	}

	void Update()
	{
		mDelayTime -= Time.deltaTime;
		if(mDelayTime <= 0.0f)
		{
			mDelayTime += kUpdateDelay;
			UpdateTick();
		}
	}

	void UpdateTick()
	{
		if(!Input.location.isEnabledByUser)
		{
			switch(State)
			{
				case ListeningState.Disabled:
					// Do nothing
				break;

				case ListeningState.Stopped:
				{
					SetState(ListeningState.Disabled);
				}
				break;

				case ListeningState.Starting:
				case ListeningState.Running:
				{
					Input.location.Stop();
					try
					{
						Input.location.Stop();
					}
					catch(UnityException ex)
					{
						Debug.LogError("Failed to stop location: " + ex.ToString());
					}
					SetState(ListeningState.Disabled);
				}
				break;
			}
		}
		else
		{
			switch(State)
			{
				case ListeningState.Disabled:
				{
					SetState(ListeningState.Stopped);
				}
				break;

				case ListeningState.Stopped:
				{
					try
					{
						Input.location.Start(5.0f, 0.5f);
						SetState(ListeningState.Starting);
					}
					catch(UnityException ex)
					{
						Debug.LogError("Failed to start location: " + ex.ToString());
					}
				}
				break;

				case ListeningState.Starting:
				{
					if(LocationServiceStatus.Running == Input.location.status)
					{
						SetState(ListeningState.Running);
					}
					else if (LocationServiceStatus.Initializing != Input.location.status)
					{
						Debug.LogError("Failed to initalize location");
						SetState(ListeningState.Stopped);
					}
				}
				break;

				case ListeningState.Running:
				{
					// TODO: Determine ping?
					// TODO: Track last update time!
					if(null != OnLocationUpdate)
					{
						OnLocationUpdate(Input.location.lastData);
					}
				}
				break;
			}
		}
	}
}
