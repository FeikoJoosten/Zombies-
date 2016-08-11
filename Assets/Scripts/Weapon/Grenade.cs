﻿using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SphereCollider))]
public class Grenade : OverridableMonoBehaviour
{
	[SerializeField]
	private GameObject explosionPrefab;
	[SerializeField]
	private SphereCollider coll;
	[SerializeField]
	private float waitBeforeExplosionTime;
	[SerializeField]
	private float damage;
	[SerializeField]
	private float destroyTime;
	[SerializeField]
	private Rigidbody rig;
	[SerializeField]
	private AudioSource audioSource;
	[SerializeField]
	private AudioClip audioClip;
	[SerializeField]
	private WeaponType grenadeType;

	private bool isExploded = false;
	private bool doOnce = false;
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

	void Start()
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
				AudioSource audio = Instantiate(new GameObject("Grenade audio").AddComponent<AudioSource>());
				GameManager.GetInstance().GetAudioManager().AddSFXAudioSource(audio);
				GameManager.GetInstance().GetAudioManager().PlaySFXSound(audio, audioClip);
				Destroy(audio.gameObject, audioClip.length);
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
