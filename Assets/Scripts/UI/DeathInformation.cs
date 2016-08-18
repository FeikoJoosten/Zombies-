using UnityEngine;
using System.Collections;

public class DeathInformation : MonoBehaviour
{
	[SerializeField]
	private GameObject traitorTexture;
	[SerializeField]
	private GameObject innocentTexture;
	[SerializeField]
	private GameObject detectiveTeture;
	[SerializeField]
	private GameObject teamStatusHolder;
	[SerializeField]
	private GameObject pistolTexture;
	[SerializeField]
	private GameObject uziTexture;
	[SerializeField]
	private GameObject machineGunTexture;
	[SerializeField]
	private GameObject shotgunTexture;
	[SerializeField]
	private GameObject sniperTexture;
	[SerializeField]
	private GameObject grenadeTexture;
	[SerializeField]
	private GameObject weaponStatusHolder;
	[SerializeField]
	private GameObject bodyTexture;
	[SerializeField]
	private GameObject headTexture;
	[SerializeField]
	private GameObject leftArmTexture;
	[SerializeField]
	private GameObject rightArmTexture;
	[SerializeField]
	private GameObject leftLegTexture;
	[SerializeField]
	private GameObject rightLegTexture;
	[SerializeField]
	private GameObject bodyTypeStatusHolder;

	public void CreateDeathInformation(TTTTeams team, WeaponType weapon, PlayerBodyType hitBodyType)
	{
		ResetObjects();

		CreateTeamStatusObject(team).transform.SetParent(teamStatusHolder.transform);
		CreateWeaponStatusObject(weapon).transform.SetParent(weaponStatusHolder.transform);
		CreateBodyTypeStatusObject(hitBodyType).transform.SetParent(bodyTypeStatusHolder.transform);
	}

	GameObject CreateTeamStatusObject(TTTTeams team)
	{
		if (team == TTTTeams.Detective)
		{
			detectiveTeture.SetActive(true);
			return detectiveTeture;
		}
		else if (team == TTTTeams.Innocent)
		{
			innocentTexture.SetActive(true);
			return innocentTexture;
		}
		else if (team == TTTTeams.Traitor)
		{
			traitorTexture.SetActive(true);
			return traitorTexture;
		}

		return null;
	}

	GameObject CreateWeaponStatusObject(WeaponType weapon)
	{
		if (weapon == WeaponType.Pistol)
		{
			pistolTexture.SetActive(true);
			return pistolTexture;
		}
		else if (weapon == WeaponType.Uzi)
		{
			uziTexture.SetActive(true);
			return uziTexture;
		}
		else if (weapon == WeaponType.MachineGun)
		{
			machineGunTexture.SetActive(true);
			return machineGunTexture;
		}
		else if (weapon == WeaponType.Shotgun)
		{
			shotgunTexture.SetActive(true);
			return shotgunTexture;
		}
		else if (weapon == WeaponType.Sniper)
		{
			sniperTexture.SetActive(true);
			return sniperTexture;
		}
		else if (weapon == WeaponType.Grenade)
		{
			grenadeTexture.SetActive(true);
			return grenadeTexture;
		}

		return null;
	}

	GameObject CreateBodyTypeStatusObject(PlayerBodyType body)
	{
		if (body == PlayerBodyType.Body)
		{
			bodyTexture.SetActive(true);
			return bodyTexture;
		}
		else if (body == PlayerBodyType.Head)
		{
			headTexture.SetActive(true);
			return headTexture;
		}
		else if (body == PlayerBodyType.LeftArm)
		{
			leftArmTexture.SetActive(true);
			return leftArmTexture;
		}
		else if (body == PlayerBodyType.RightArm)
		{
			rightArmTexture.SetActive(true);
			return rightArmTexture;
		}
		else if (body == PlayerBodyType.LeftLeg)
		{
			leftLegTexture.SetActive(true);
			return leftLegTexture;
		}
		else if (body == PlayerBodyType.RightLeg)
		{
			rightLegTexture.SetActive(true);
			return rightLegTexture;
		}

		return null;
	}

	void ResetObjects()
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
