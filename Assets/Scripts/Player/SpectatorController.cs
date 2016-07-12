using UnityEngine;
using System.Collections;

public class SpectatorController : OverridableMonoBehaviour
{
	[SerializeField]
	private Spectator player;

	private PlayerActions playerActions = PlayerActions.CreateWithDefaultBindings();

	public override void UpdateMe()
	{
		if (player == null)
		{
			return;
		}

		if(GameManager.GetInstance().GetNetworkManager().AllRemainingPlayers.Count == 0)
		{
			player.OpenEndscreen();
		}

		if (player.PlayerCamera != null)
		{
			if(player.PlayerCamera.transform.parent != player.transform)
			{
				if (player.PlayerCamera != null)
				{
					player.PlayerCamera.transform.SetParent(player.transform);
					player.PlayerCamera.transform.position = Vector3.zero;
					player.PlayerCamera.transform.rotation = Quaternion.Euler(Vector3.zero);
				}
			}
		}

		if (playerActions.toggleHighscoreMenu.WasPressed)
		{
			player.ToggleHighscoreMenu();
		}

		if (playerActions.pauseButton.WasPressed)
		{
			player.PauseGame();
		}

		if (Time.timeScale != 0)
		{
			RotateCharacter(playerActions.rotation.Value);
		}
	}

	void FixedUpdate()
	{
		MoveCharacter(playerActions.move.Value);
	}

	void MoveCharacter(Vector2 value)
	{
		Vector3 movement = (player.PlayerCamera.transform.forward * value.y) + (player.Rig.transform.right * value.x);

		if (Physics.Raycast(transform.position + new Vector3(0, 0.75F, 0), movement.normalized, 0.25F) == false)
		{
			if (playerActions.sprint.IsPressed)
			{
				player.Rig.MovePosition(transform.position + (movement.normalized * player.SprintSpeed * Time.deltaTime));
			}
			else
			{
				player.Rig.MovePosition(transform.position + (movement.normalized * player.MovementSpeed * Time.deltaTime));
			}
		}
	}

	void RotateCharacter(Vector2 value)
	{
		Vector3 playerRotation = new Vector3(0, value.y, 0);
		transform.Rotate(playerRotation * player.RotationSpeed);

		player.PlayerCamera.transform.Rotate(value.x * player.RotationSpeed, 0, 0);
		Vector3 headRotation = player.PlayerCamera.transform.rotation.eulerAngles;
		Vector3 bodyRotation = transform.rotation.eulerAngles;
		player.PlayerCamera.transform.rotation = Quaternion.Euler(
			new Vector3(headRotation.x,
			bodyRotation.y, bodyRotation.z));
	}
}
