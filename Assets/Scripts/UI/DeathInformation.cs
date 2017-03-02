using UnityEngine;

public class DeathInformation : MonoBehaviour
{
	[SerializeField]
	private GameObject traitorTexture = null;
	[SerializeField]
	private GameObject innocentTexture = null;
	[SerializeField]
	private GameObject detectiveTeture = null;
	[SerializeField]
	private GameObject teamStatusHolder = null;
	[SerializeField]
	private GameObject pistolTexture = null;
	[SerializeField]
	private GameObject uziTexture = null;
	[SerializeField]
	private GameObject machineGunTexture = null;
	[SerializeField]
	private GameObject shotgunTexture = null;
	[SerializeField]
	private GameObject sniperTexture = null;
	[SerializeField]
	private GameObject grenadeTexture = null;
	[SerializeField]
	private GameObject weaponStatusHolder = null;
	[SerializeField]
	private GameObject bodyTexture = null;
	[SerializeField]
	private GameObject headTexture = null;
	[SerializeField]
	private GameObject leftArmTexture = null;
	[SerializeField]
	private GameObject rightArmTexture = null;
	[SerializeField]
	private GameObject leftLegTexture = null;
	[SerializeField]
	private GameObject rightLegTexture = null;
	[SerializeField]
	private GameObject bodyTypeStatusHolder = null;

	public void CreateDeathInformation(TTTTeams team, WeaponType weapon, PlayerBodyType hitBodyType)
	{
		ResetObjects();

		CreateTeamStatusObject(team).transform.SetParent(teamStatusHolder.transform);
		CreateWeaponStatusObject(weapon).transform.SetParent(weaponStatusHolder.transform);
		CreateBodyTypeStatusObject(hitBodyType).transform.SetParent(bodyTypeStatusHolder.transform);
	}

	private GameObject CreateTeamStatusObject(TTTTeams team)
	{
		switch (team)
		{
			case TTTTeams.Detective:
				detectiveTeture.SetActive(true);
				return detectiveTeture;
			case TTTTeams.Innocent:
				innocentTexture.SetActive(true);
				return innocentTexture;
			case TTTTeams.Traitor:
				traitorTexture.SetActive(true);
				return traitorTexture;
		}

		return null;
	}

	private GameObject CreateWeaponStatusObject(WeaponType weapon)
	{
		switch (weapon)
		{
			case WeaponType.Pistol:
				pistolTexture.SetActive(true);
				return pistolTexture;
			case WeaponType.Uzi:
				uziTexture.SetActive(true);
				return uziTexture;
			case WeaponType.MachineGun:
				machineGunTexture.SetActive(true);
				return machineGunTexture;
			case WeaponType.Shotgun:
				shotgunTexture.SetActive(true);
				return shotgunTexture;
			case WeaponType.Sniper:
				sniperTexture.SetActive(true);
				return sniperTexture;
			case WeaponType.Grenade:
				grenadeTexture.SetActive(true);
				return grenadeTexture;
		}

		return null;
	}

	private GameObject CreateBodyTypeStatusObject(PlayerBodyType body)
	{
		switch (body)
		{
			case PlayerBodyType.Body:
				bodyTexture.SetActive(true);
				return bodyTexture;
			case PlayerBodyType.Head:
				headTexture.SetActive(true);
				return headTexture;
			case PlayerBodyType.LeftArm:
				leftArmTexture.SetActive(true);
				return leftArmTexture;
			case PlayerBodyType.RightArm:
				rightArmTexture.SetActive(true);
				return rightArmTexture;
			case PlayerBodyType.LeftLeg:
				leftLegTexture.SetActive(true);
				return leftLegTexture;
			case PlayerBodyType.RightLeg:
				rightLegTexture.SetActive(true);
				return rightLegTexture;
		}

		return null;
	}

	private void ResetObjects()
	{
		traitorTexture.SetActive(false);
		innocentTexture.SetActive(false);
		detectiveTeture.SetActive(false);
		pistolTexture.SetActive(false);
		uziTexture.SetActive(false);
		machineGunTexture.SetActive(false);
		shotgunTexture.SetActive(false);
		sniperTexture.SetActive(false);
		grenadeTexture.SetActive(false);
		bodyTexture.SetActive(false);
		headTexture.SetActive(false);
		leftArmTexture.SetActive(false);
		rightArmTexture.SetActive(false);
		leftLegTexture.SetActive(false);
		rightLegTexture.SetActive(false);
	}
}
