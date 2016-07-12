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

	private int ownerID;

	public int OwnerID
	{
		get { return ownerID; }
		set { ownerID = value; }
	}

	public float Damage
	{
		get { return damage; }
	}

	public override void UpdateMe()
	{
		if(PhotonNetwork.isMasterClient == false)
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
		if (other != null)
		{
			Destroy(gameObject);
		}
	}
}
