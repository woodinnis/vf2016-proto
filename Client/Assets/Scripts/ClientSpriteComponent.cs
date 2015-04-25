using UnityEngine;
using System.Collections;
using System.ComponentModel;

public class ClientSpriteComponent : MonoBehaviour
{
	[DefaultValue(null)]
	public GameObject Sprite { get; private set; }

	private Vector2 mLocation = Vector2.zero;
	public Vector2 Location
	{
		get { return mLocation; }
		private set { mLocation = value; }
	}

	private bool mDirty = false;
	private Vector2 mNextLocation = Vector2.zero;

	private SpriteRenderer mRenderer = null;
	private Transform mTransform = null;

	void Awake()
	{
		// Cache these
		mRenderer = gameObject.GetComponent<SpriteRenderer>();
		mTransform = gameObject.GetComponent<Transform>();
	}

	public void SetupClient(string name, Color color)
	{
		gameObject.name = name;
		mRenderer.color = color;
	}

	public void SetNextLocation(Vector2 location)
	{
		mNextLocation = location;
		mDirty = (mLocation.Equals(mNextLocation) == false);
	}

	public void UpdateLocation(bool forced)
	{
		if((mDirty || forced) == false)
		{
			return;
		}

		Vector2 delta = FakeInputManager.Instance.Location - mNextLocation;
		mTransform.position = new Vector3(-1 * delta.x * GameplayClientManager.Instance.LocationScalar, delta.y * GameplayClientManager.Instance.LocationScalar, 0);

		mDirty = false;
		mLocation = mNextLocation;
	}
}
