using UnityEngine;
using System.Collections;

public class Bullet : OverridableMonoBehaviour
{
	[SerializeField]
	private float damage;
	[SerializeField]
	private float movementSpeed;
	[SerializeField]
	private Rigidbody rig;
	[SerializeField]
	private float destroyTime = 10;
	[SerializeField]
	private LayerMask playerLayerMask;
	[SerializeField]
	private WeaponType bulletType;

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

	void Destroy()
	{
		UpdateManager.RemoveSpecificItem(this);
		Destroy(gameObject);
	}

	void FixedUpdate()
	{
		rig.velocity = transform.up * movementSpeed * Time.deltaTime;
	}

	void OnTriggerEnter(Collider other)
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
