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

	private Vector2 mNewLocation = new Vector2();

	// Distance to move per second
	[Range(0.001f, 1.0f)]
	[DefaultValue(0.01f)]
	public float DeltaPerSecond;

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

		mNewLocation.Set(mLocation.x, mLocation.y);

		if(Input.GetKey(KeyCode.DownArrow))
		{
			mNewLocation.y += delta;
		}

		if(Input.GetKey(KeyCode.UpArrow))
		{
			mNewLocation.y -= delta;
		}

		if(Input.GetKey(KeyCode.RightArrow))
		{
			mNewLocation.x += delta;
		}
		
		if(Input.GetKey(KeyCode.LeftArrow))
		{
			mNewLocation.x -= delta;
		}

		if(!mLocation.Equals(mNewLocation))
		{
			mLocation.Set(mNewLocation.x, mNewLocation.y);
			if(null != LocationChanged)
			{
				LocationChanged(mLocation);
			}
		}
	}
}
