using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class Grenade : OverridableMonoBehaviour
{
	[SerializeField]
	private GameObject explosionPrefab = null;
	[SerializeField]
	private SphereCollider coll = null;
	[SerializeField]
	private float waitBeforeExplosionTime;
	[SerializeField]
	private float damage = 0;
	[SerializeField]
	private float destroyTime = 0;
	[SerializeField]
	private Rigidbody rig = null;
	[SerializeField]
	private AudioClip audioClip = null;
	[SerializeField]
	private WeaponType grenadeType = WeaponType.Grenade;

	private bool isExploded;
	private bool doOnce;
	private bool touchedObject;

	private int ownerID;

	public float Damage
	{
		get { return damage; }
	}
	public bool IsExploded
	{
		get { return isExploded; }
	}
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
	public Rigidbody Rig
	{
		get { return rig; }
	}
	public WeaponType GrenadeType
	{
		get { return grenadeType; }
	}

	private void Start()
	{
		if (coll.enabled == true)
		{
			coll.enabled = false;
		}
	}

	public override void UpdateMe()
	{
		if (waitBeforeExplosionTime > 0)
		{
			waitBeforeExplosionTime -= Time.deltaTime;
		}
		else
		{
			if (doOnce == false)
			{
				AudioSource Audio = Instantiate(new GameObject("Grenade audio").AddComponent<AudioSource>());
				GameManager.GetInstance().GetAudioManager().AddSFXAudioSource(Audio);
				GameManager.GetInstance().GetAudioManager().PlaySFXSound(Audio, audioClip);
				Destroy(Audio.gameObject, audioClip.length);
				doOnce = true;
			}

			GameObject obj = (GameObject)Instantiate(explosionPrefab, transform.position, explosionPrefab.transform.rotation);
			coll.enabled = true;
			isExploded = true;
			Destroy(obj, destroyTime);
			Destroy(gameObject, destroyTime);
		}
	}
}
