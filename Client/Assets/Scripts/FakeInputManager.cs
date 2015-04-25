using UnityEngine;
using System.Collections;
using System.ComponentModel;

public class FakeInputManager : MonoBehaviour
{
	[DefaultValue(null)]
	public static FakeInputManager Instance { get; private set; }

	public delegate void LocationChange(Vector2 location);
	public static LocationChange LocationChanged;

	// 49, -122 is a very innacurate GPS location for Vancouver BC
	private Vector2 mLocation = new Vector2(49, -122);
	public Vector2 Location { get { return mLocation; } }

	// Distance to move per second
	[Range(0.001f, 1.0f)]
	[DefaultValue(0.01f)]
	public float DeltaPerSecond;

	private static readonly string FakeClientGUID = "FAKE";
	private bool mFakeClientCreated = false;
	// Closed to the default GPS location, but not exact
	private Vector2 mFakeClientLocation = new Vector2(49.01f, -122.01f);

	void Awake()
	{
		if(null == Instance)
		{
			Instance = this;
		}
	}

	void OnApplicationQuit()
	{
		Instance = null;
	}

	void Update()
	{
		float delta = DeltaPerSecond * Time.deltaTime;
		Vector2 locationDelta = new Vector2();

		if(Input.GetKey(KeyCode.DownArrow))
		{
			locationDelta.y = delta;
		}

		if(Input.GetKey(KeyCode.UpArrow))
		{
			locationDelta.y = -delta;
		}

		if(Input.GetKey(KeyCode.RightArrow))
		{
			locationDelta.x = delta;
		}
		
		if(Input.GetKey(KeyCode.LeftArrow))
		{
			locationDelta.x = -delta;
		}

		if(!mFakeClientCreated && Input.GetKey(KeyCode.C))
		{
			GameplayClientManager.Instance.CreateFake(FakeClientGUID);
			mFakeClientCreated = true;
		}
		else if(mFakeClientCreated && Input.GetKey(KeyCode.X))
		{
			GameplayClientManager.Instance.DestroyFake(FakeClientGUID);
			mFakeClientCreated = false;
		}

		if(!locationDelta.Equals(Vector2.zero))
		{
			mLocation = mLocation + locationDelta;
			if(null != LocationChanged)
			{
				LocationChanged(mLocation);
			}
		}

		// Update the real-fake location first!
		if(mFakeClientCreated)
		{
			UpdateFakeClient();
		}
	}

	void UpdateFakeClient()
	{
		float delta = DeltaPerSecond * Time.deltaTime;
		Vector2 locationDelta = new Vector2();
		
		if(Input.GetKey(KeyCode.S))
		{
			locationDelta.y = delta;
		}
		
		if(Input.GetKey(KeyCode.W))
		{
			locationDelta.y = -delta;
		}

		if(Input.GetKey(KeyCode.D))
		{
			locationDelta.x = delta;
		}
		
		if(Input.GetKey(KeyCode.A))
		{
			locationDelta.x = -delta;
		}

		if(!locationDelta.Equals(Vector2.zero))
		{
			mFakeClientLocation  = mFakeClientLocation + locationDelta;
			GameplayClientManager.Instance.FakeUpdateClient(FakeClientGUID, mFakeClientLocation);
		}
	}
}
