using UnityEngine;

public class Bullet : OverridableMonoBehaviour
{
	[SerializeField]
	private float damage = 0;
	[SerializeField]
	private float movementSpeed = 0;
	[SerializeField]
	private Rigidbody rig = null;
	[SerializeField]
	private float destroyTime = 10;
	[SerializeField]
	private LayerMask playerLayerMask = 0;
	[SerializeField]
	private WeaponType bulletType = WeaponType.Pistol;

	private int ownerID;
	private bool touchedObject;

	public bool TouchedObject
	{
		get { return touchedObject; }
		set { touchedObject = value; }
	}
	public int OwnerID
	{
		get { return ownerID; }
		set { ownerID = value; }
	}
	public float Damage
	{
		get { return damage; }
	}
	public WeaponType BulletType
	{
		get { return bulletType; }
	}

	public override void UpdateMe()
	{
		if (PhotonNetwork.isMasterClient == false)
		{
			return;
		}

		destroyTime -= Time.deltaTime;

		if (destroyTime < 0)
		{
			//UpdateManager.RemoveSpecificItemAndDestroyIt(this);
			Destroy();
		}
	}

	private void Destroy()
	{
		UpdateManager.RemoveSpecificItem(this);
		Destroy(gameObject);
	}

	private void FixedUpdate()
	{
		rig.velocity = transform.up * movementSpeed * Time.deltaTime;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (GameManager.GetInstance().CurrentGameType != GameTypes.ZombieMode)
		{
			if (other != null)
			{
				if (other.gameObject.layer == playerLayerMask)
				{
					Debug.Log("returning");
					return;
				}
			}
		}

		if (other != null)
		{
			Destroy(gameObject);
		}
	}
}
