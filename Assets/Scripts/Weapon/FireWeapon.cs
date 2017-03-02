using UnityEngine;
using System.Collections.Generic;

public class FireWeapon : Weapon
{
	[SerializeField]
	private Bullet bulletPrefab = null;
	[SerializeField]
	private float bulletFireCount = 0;
	[SerializeField]
	private bool isShotgun = false;
	[SerializeField]
	private float spreadFactor = 0.01F;
	[SerializeField]
	private AudioSource audioSource = null;
	[SerializeField]
	private AudioClip audioClip = null;

	private PhotonView photonView;
	private List<AudioSource> audioSources = new List<AudioSource>();

	private void Start()
	{
		if (PhotonNetwork.offlineMode == false)
		{
			photonView = GetComponent<PhotonView>();
		}

		audioSources.Add(audioSource);

		if (PhotonNetwork.offlineMode == true)
		{
			GameManager.GetInstance().GetAudioManager().AddSFXAudioSource(audioSource);
		}
		else
		{
			if (GameManager.GetInstance().GetAudioManager() != null)
			{
				GameManager.GetInstance().GetAudioManager().AddSFXAudioSource(audioSource);
			}
			else
			{
				Debug.Log("Had to seach for AM");
				FindObjectOfType<AudioManager>().AddSFXAudioSource(audioSource);
			}
		}
	}

	public override void Fire()
	{
		if (!IsWaitTimerFinished || Player.IsReloading != false) return;
		if (IsAllowedToFire != true) return;

		if (CurrentAmmunitionInMagLeft <= 0)
		{
			if (CurrentTotalAmmunitionLeft > 0)
			{
				Player.ReloadWeapon();
			}
			else
			{
				return;
			}
		}
		else
		{
			CurrentAmmunitionInMagLeft--;
		}

		if (isShotgun == true)
		{
			for (int i = 0; i < bulletFireCount; i++)
			{
				Quaternion pelletRot = transform.rotation;
				pelletRot.x += Random.Range(-spreadFactor, spreadFactor);
				pelletRot.y += Random.Range(-spreadFactor, spreadFactor);
				pelletRot.z += Random.Range(-spreadFactor, spreadFactor);
				pelletRot.w += Random.Range(-spreadFactor, spreadFactor);

				FireShotgunBullet(pelletRot);
			}
		}
		else
		{
			FireBullet();
		}

		PlayAudio();

		StartCoroutine(WaitBeforeFire());

		if (IsAllowedToUseAutoFire == true)
		{
			StartCoroutine(FireRateCountDown());
		}
	}

	private void FireBullet()
	{
		if (PhotonNetwork.offlineMode == true)
		{
			Bullet bullet = (Bullet)Instantiate(bulletPrefab, SpawnLocation.position, SpawnLocation.rotation * bulletPrefab.transform.rotation);
			bullet.OwnerID = Player.gameObject.GetInstanceID();
		}
		else
		{
			Bullet bullet = (Bullet)Instantiate(bulletPrefab, SpawnLocation.position, SpawnLocation.rotation * bulletPrefab.transform.rotation);
			bullet.OwnerID = Player.photonView.viewID;
			photonView.RPC("InstantiateBullet", PhotonTargets.Others, Player.photonView.viewID);
		}
	}

	private void FireShotgunBullet(Quaternion pelletRot)
	{
		if (PhotonNetwork.offlineMode == true)
		{
			Bullet bullet = (Bullet)Instantiate(bulletPrefab, SpawnLocation.position, pelletRot * bulletPrefab.transform.rotation);
			bullet.OwnerID = Player.gameObject.GetInstanceID();
		}
		else
		{
			Bullet bullet = (Bullet)Instantiate(bulletPrefab, SpawnLocation.position, pelletRot * bulletPrefab.transform.rotation);
			bullet.OwnerID = Player.photonView.viewID;
			photonView.RPC("InstantiateBullet", PhotonTargets.Others, Player.photonView.viewID);
		}
	}

	private void PlayAudio()
	{
		int count = 0;

		for (int i = 0; i < audioSources.Count; i++)
		{
			if (audioSources[i].isPlaying == true)
			{
				count++;
			}
			else
			{
				GameManager.GetInstance().GetAudioManager().PlaySFXSound(audioSources[i], audioClip);
				break;
			}
		}

		if (count != audioSources.Count) return;

		audioSources.Add((AudioSource)CopyComponent(audioSource, audioSource.gameObject));
		audioSources[audioSources.Count - 1].playOnAwake = false;
		audioSources[audioSources.Count - 1].volume = audioSources[0].volume;
		GameManager.GetInstance().GetAudioManager().AddSFXAudioSource(audioSources[audioSources.Count - 1]);
		GameManager.GetInstance().GetAudioManager().PlaySFXSound(audioSources[audioSources.Count - 1], audioClip);
	}

	private Component CopyComponent(Component original, GameObject destination)
	{
		System.Type type = original.GetType();
		Component copy = destination.AddComponent(type);
		// Copied fields can be restricted with BindingFlags
		System.Reflection.FieldInfo[] fields = type.GetFields();
		foreach (System.Reflection.FieldInfo field in fields)
		{
			field.SetValue(copy, field.GetValue(original));
		}
		return copy;
	}

	[PunRPC]
	private void InstantiateBullet(int ownerID)
	{
		Bullet bullet = (Bullet)Instantiate(bulletPrefab, SpawnLocation.position, SpawnLocation.rotation * bulletPrefab.transform.rotation);
		bullet.OwnerID = ownerID;
		PlayAudio();
	}

	[PunRPC]
	private void InstantiateShotgunBullet(int ownerID, Quaternion bulletRotation)
	{
		Bullet bullet = (Bullet)Instantiate(bulletPrefab, SpawnLocation.position, bulletRotation);
		bullet.OwnerID = ownerID;
		PlayAudio();
	}
}
