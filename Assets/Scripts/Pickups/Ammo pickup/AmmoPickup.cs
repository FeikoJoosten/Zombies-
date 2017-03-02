using UnityEngine;

public class AmmoPickup : OverridableMonoBehaviour
{
	[SerializeField]
	private int amountToReload = 0;
	[SerializeField]
	private int weaponToReload = 0;
	[SerializeField]
	private float rotationSpeed = 0;
	[SerializeField]
	private float trailMovementSpeed = 0;
	[SerializeField]
	private TrailRenderer trail = null;
	[SerializeField]
	private ParticleSystem particles = null;
	[SerializeField]
	private Vector3 trailEndPosition = Vector3.zero;

	private Vector3 trailStartPosition;
	private float currentMovementSpeed;
	private float currentMovementPercentage;
	private bool gaveAmmo;
	private bool shouldShowLineRenderer = true;

	public bool ShouldShowLineRenderer
	{
		get { return shouldShowLineRenderer; }
		set { shouldShowLineRenderer = value; }
	}

	private void Start()
	{
		trailStartPosition = trail.transform.localPosition;

		if (shouldShowLineRenderer != false) return;

		trail.enabled = false;
		particles.Stop(true);
	}

	public override void UpdateMe()
	{
		if (shouldShowLineRenderer == false) return;

		transform.Rotate(transform.up * rotationSpeed * Time.deltaTime);
		trail.transform.localPosition = Vector3.Lerp(trailStartPosition, trailStartPosition + trailEndPosition, currentMovementPercentage);

		if (currentMovementSpeed > trailMovementSpeed) return;

		currentMovementSpeed += Time.deltaTime;
		currentMovementPercentage = currentMovementSpeed / trailMovementSpeed;
	}

	private void OnTriggerEnter(Collider other)
	{
		Player player = other.GetComponent<Player>();

		if (player == null) return;

		if (player.GetWeaponInformation(weaponToReload).CurrentTotalAmmunitionLeft >=
			player.GetWeaponInformation(weaponToReload).MaxAmmunitionCount) return;

		if (gaveAmmo == true) return;

		if (player.CurrentWeapon != null)
		{
			if (GameManager.GetInstance().CurrentGameType == GameTypes.TTT)
			{
				player.PickupWeapon(weaponToReload);
			}
			player.GetWeaponInformation(weaponToReload).AddAmmoToAmmoPile(amountToReload);
		}
		gaveAmmo = true;

		if (PhotonNetwork.isMasterClient == true)
		{
			UpdateManager.RemoveSpecificItemAndDestroyIt(this);
		}
	}

	private void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isWriting == true && PhotonNetwork.isMasterClient == true)
		{
			stream.SendNext(shouldShowLineRenderer);
		}
		else
		{
			shouldShowLineRenderer = (bool)stream.ReceiveNext();

			if (shouldShowLineRenderer == true) return;

			trail.enabled = false;
			particles.Stop(true);
		}
	}
}
