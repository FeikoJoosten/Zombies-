using UnityEngine;
using InControl;
using System.Collections;

public class PlayerController : OverridableMonoBehaviour
{
	[SerializeField]
	private Player player;
	[SerializeField]
	private GameObject playerHead;
	[SerializeField]
	private GameObject playerArmLeft;
	[SerializeField]
	private GameObject playerArmRight;

	public bool IsSprinting
	{
		get { return playerActions.sprint.IsPressed; }
	}

	private PlayerActions playerActions = PlayerActions.CreateWithDefaultBindings();

	public override void UpdateMe()
	{
		if (playerActions.pauseButton.WasPressed)
		{
			player.TogglePauseGame();
		}

		if (player == null || player.CurrentWeapon == null || player.HasPauseMenuOpen == true)
		{
			return;
		}

		if (player.CurrentWeapon.IsAllowedToUseAutoFire == true)
		{
			if (playerActions.shoot.IsPressed)
			{
				player.FireWeapon();
			}
		}
		else
		{
			if (playerActions.shoot.WasPressed)
			{
				player.FireWeapon();
			}
		}

		if(playerActions.throwWeaponAway.WasReleased)
		{
			player.ThrowWeaponAway();
		}

		if (playerActions.reloadWeapon.WasPressed)
		{
			player.ReloadWeapon();
		}

		if (playerActions.toggleWeaponMenu.WasPressed)
		{
			player.ToggleWeaponMenu();
		}

		if (playerActions.toggleHighscoreMenu.WasPressed)
		{
			player.ToggleHighscoreMenu();
		}

		if (playerActions.select.WasPressed && player.IsWeaponMenuActive == true)
		{
			player.SelectWeapon();
			return;
		}

		if (playerActions.nextWeapon.WasPressed && player.IsWeaponMenuActive == true)
		{
			player.SelectNextWeapon();
		}

		if (playerActions.previousWeapon.WasPressed && player.IsWeaponMenuActive == true)
		{
			player.SelectPreviousWeapon();
		}

		if (playerActions.selectWeapon1.WasPressed)
		{
			player.SelectSpecificWeapon(0);
		}
		else if (playerActions.selectWeapon2.WasPressed)
		{
			player.SelectSpecificWeapon(1);
		}
		else if (playerActions.selectWeapon3.WasPressed)
		{
			player.SelectSpecificWeapon(2);
		}
		else if (playerActions.selectWeapon4.WasPressed)
		{
			player.SelectSpecificWeapon(3);
		}
		else if (playerActions.selectWeapon5.WasPressed)
		{
			player.SelectSpecificWeapon(4);
		}
		else if (playerActions.selectWeapon6.WasPressed)
		{
			player.SelectSpecificWeapon(5);
		}

		if (PhotonNetwork.offlineMode == false)
		{
			player.photonView.RPC("UpdateRotation", PhotonTargets.Others, playerActions.rotation.Value);
		}

		player.UpdateRotation(playerActions.rotation.Value);
	}

	void FixedUpdate()
	{
		if (player.HasPauseMenuOpen == false)
		{
			if (PhotonNetwork.offlineMode == false)
			{
				//player.photonView.RPC("UpdateMovement", PhotonTargets.OthersBuffered, playerActions.move.Value, playerActions.sprint.IsPressed);
			}

			player.UpdateMovement(playerActions.move.Value, playerActions.sprint.IsPressed);
		}
	}
}
