using UnityEngine;
using Steamworks;
using System.Collections;

public class Player : OverridableMonoBehaviour
{
	[SerializeField]
	private int killsNeededToUnlockNextWeapon = 45;
	[SerializeField]
	private float startingHealth;
	[SerializeField]
	private float movementSpeed;
	[SerializeField]
	private float sprintSpeed;
	[SerializeField]
	private float rotationSpeed;
	[SerializeField]
	private float maxYRotation;
	[SerializeField]
	private Weapon currentWeapon;
	[SerializeField]
	private Weapon[] allWeapons;
	[SerializeField]
	private WeaponMenu weaponMenu;
	[SerializeField]
	private AmmoInfoUI ammoInfoUI;
	[SerializeField]
	private float maxWeaponMenuActiveTime;
	[SerializeField]
	private Animator ani;
	[SerializeField]
	private Healthbar UIHealthBar;
	[SerializeField]
	private Healthbar inGameHealthBar;
	[SerializeField]
	private HighscoreList highScoreList;
	[SerializeField]
	private RectTransform ingameUI;
	[SerializeField]
	private RectTransform endscreenUI;
	[SerializeField]
	private Camera[] playerCameras;
	[SerializeField]
	private Rigidbody[] ragdollRigibodys;
	[SerializeField]
	private Collider[] colliders;
	[SerializeField]
	private PlayerController playerController;
	[SerializeField]
	private Spectator spectatorPrefab;
	[SerializeField]
	private PauseMenuManager pauseMenuManager;
	[SerializeField]
	private GameObject[] nonRendarableBodyParts;

	private float currentHealth;
	private float currentWeaponMenuTimer;
	private float savedTimescale;
	private int reloadHashID;
	private int cameraRotationXHashID;
	private int sprintingHashID;
	private int walkingHashID;
	private int killCount;
	private bool isReloading;
	private bool isDead = false;
	private bool hasLoadedSettings = false;
	private Rigidbody rig;
	private Vector3 receivedPosition;

	public float StartingHealth
	{
		get { return startingHealth; }
	}
	public float CurrentHealth
	{
		get { return currentHealth; }
		set { currentHealth = value; }
	}
	public int KillCount
	{
		get { return killCount; }
		set { killCount = value; }
	}
	public Weapon[] AllWeapons
	{
		get { return allWeapons; }
	}
	public Weapon CurrentWeapon
	{
		get { return currentWeapon; }
		set { currentWeapon = value; }
	}
	public bool IsReloading
	{
		get { return isReloading; }
		set { isReloading = value; }
	}
	public bool IsWeaponMenuActive
	{
		get { return weaponMenu.IsEnabled; }
	}
	public bool HasPauseMenuOpen
	{
		get { return pauseMenuManager.gameObject.activeInHierarchy; }
	}

	void Start()
	{
		GetAnimationHashIDs();
		currentWeapon = allWeapons[0];
		currentHealth = startingHealth;
		rig = GetComponent<Rigidbody>();

		for (int i = 0; i < ragdollRigibodys.Length; i++)
		{
			ragdollRigibodys[i].isKinematic = true;
		}

		if (PhotonNetwork.offlineMode == true)
		{
			AssignStartValuesForOwner();
		}
		else
		{
			for (int i = 0; i < playerCameras.Length; i++)
			{
				playerCameras[i].gameObject.SetActive(false);
			}
			playerController.enabled = false;
		}
	}

	void AssignMPSettings()
	{
		if (hasLoadedSettings == false)
		{
			if (photonView.owner == PhotonNetwork.player)
			{
				for (int i = 0; i < playerCameras.Length; i++)
				{
					playerCameras[i].gameObject.SetActive(true);
				}
				playerController.enabled = true;

				AssignStartValuesForOwner();
			}
			else
			{
				AssignStartValuesForOthers();
			}

			hasLoadedSettings = true;
			return;
		}
	}

	public void AssignStartValuesForOwner()
	{
		int nonRendarableLayer = LayerMask.NameToLayer("PlayerNotRenderable");
		for (int i = 0; i < nonRendarableBodyParts.Length; i++)
		{
			nonRendarableBodyParts[i].layer = nonRendarableLayer;
		}

		Cursor.lockState = CursorLockMode.None;
		Cursor.lockState = CursorLockMode.Locked;
		gameObject.name = PhotonNetwork.player.name + (PhotonNetwork.offlineMode == false ? (PhotonNetwork.isMasterClient == true ? " (Host)" : "") : "");

		//for (int i = 0; i < allWeapons.Length; i++)
		//{
		//	allWeapons[i].IsAllowedToUse = true;
		//	allWeapons[i].AssignStartingAmmo();
		//}
		allWeapons[0].IsAllowedToUse = true;
		allWeapons[0].AssignStartingAmmo();

		if (PhotonNetwork.offlineMode == false)
		{
			highScoreList.CreateMultiplayerHighScoreList();
		}
		else
		{
			highScoreList.CreateSingleplayerHighScoreList(gameObject.GetInstanceID());
		}

		UpdateWeaponUIInfo();
	}

	public void AssignStartValuesForOthers()
	{
		gameObject.name = PhotonPlayer.Find(photonView.OwnerActorNr).name + (PhotonPlayer.Find(photonView.OwnerActorNr).isMasterClient == true ? " (Host)" : "");

		for (int i = 0; i < playerCameras.Length; i++)
		{
			playerCameras[i].gameObject.SetActive(false);
		}

		playerController.enabled = false;
	}

	void GetAnimationHashIDs()
	{
		sprintingHashID = Animator.StringToHash("Sprinting");
		walkingHashID = Animator.StringToHash("Walking");
		reloadHashID = Animator.StringToHash("Reload");
		cameraRotationXHashID = Animator.StringToHash("CameraRotationX");
	}

	public override void UpdateMe()
	{
		if (PhotonNetwork.playerList.Length != GameManager.GetInstance().GetNetworkManager().AllRemainingPlayers.Count)
		{
			return;
		}
		else
		{
			if (PhotonNetwork.offlineMode == false)
			{
				AssignMPSettings();
			}
		}

		if (GameManager.GetInstance().GetNetworkManager().AllRemainingPlayers.Count == 0 && isDead == true)
		{
			if (endscreenUI.gameObject.activeInHierarchy == false)
			{
				OpenEndscreen();
				Debug.Log("Opening endscreen");
			}
		}

		if (PhotonNetwork.offlineMode == false)
		{
			if (photonView.owner == PhotonNetwork.player)
			{
				if (weaponMenu.IsEnabled == true)
				{
					UpdateWeaponMenuTimeoutTimer();
				}
			}
			else
			{
				transform.position = Vector3.Lerp(transform.position, receivedPosition, Time.deltaTime * sprintSpeed);
			}
		}
		else
		{
			if (weaponMenu.IsEnabled == true)
			{
				UpdateWeaponMenuTimeoutTimer();
			}
		}
	}

	public void FireWeapon()
	{
		if (isReloading == false && Time.timeScale != 0)
		{
			currentWeapon.Fire();
			UpdateWeaponUIInfo();
		}
	}

	[PunRPC]
	public void UpdateMovement(Vector2 value, bool isSprinting)
	{
		if (sprintingHashID == 0 || walkingHashID == 0)
		{
			return;
		}

		if (value.x == 0 && value.y == 0)
		{
			ani.SetBool(sprintingHashID, false);
			ani.SetBool(walkingHashID, false);
			return;
		}

		Debug.DrawLine(rig.transform.position, rig.transform.position + rig.transform.forward * value.y * 10);

		Vector3 movement = (rig.transform.forward * value.y) + (rig.transform.right * value.x);

		if (Physics.Raycast(transform.position + new Vector3(0, 0.75F, 0), movement.normalized, 0.25F) == false)
		{
			if (isSprinting == true)
			{
				rig.MovePosition(transform.position + (movement.normalized * sprintSpeed * Time.deltaTime));
			}
			else
			{
				rig.MovePosition(transform.position + (movement.normalized * movementSpeed * Time.deltaTime));
			}
		}
	}

	[PunRPC]
	public void UpdateRotation(Vector2 value)
	{
		Vector3 playerRotation = new Vector3(0, value.y, 0);
		transform.Rotate(playerRotation * rotationSpeed);

		playerCameras[0].gameObject.transform.Rotate(value.x * rotationSpeed, 0, 0);
		Vector3 headRotation = playerCameras[0].gameObject.transform.rotation.eulerAngles;
		Vector3 bodyRotation = transform.rotation.eulerAngles;
		playerCameras[0].gameObject.transform.rotation = Quaternion.Euler(
			new Vector3(Mathf.Clamp(headRotation.x > 180 ? headRotation.x - 360 : headRotation.x, -maxYRotation, maxYRotation),
			bodyRotation.y, bodyRotation.z));

		if (cameraRotationXHashID != 0)
		{
			ani.SetFloat(cameraRotationXHashID, headRotation.x > 180 ? headRotation.x - 360 : headRotation.x);
		}
	}

	void UpdateWeaponUIInfo()
	{
		if (currentWeapon.HasInfiniteAmmo == false)
		{
			ammoInfoUI.UpdateAmmoText(currentWeapon.CurrentTotalAmmunitionLeft);
			ammoInfoUI.UpdateMagText(currentWeapon.CurrentAmmunitionInMagLeft);
		}
		else
		{
			ammoInfoUI.UpdateAmmoText(999);
			ammoInfoUI.UpdateMagText(999);
		}
	}

	[PunRPC]
	void SwitchWeapon(int nextWeapon)
	{
		if (isReloading == true)
		{
			ani.SetBool(reloadHashID, false);
			isReloading = false;
		}

		currentWeapon.IsWaitTimerFinished = true;
		currentWeapon.gameObject.SetActive(false);
		currentWeapon = allWeapons[nextWeapon];
		currentWeapon.gameObject.SetActive(true);
		UpdateWeaponUIInfo();
	}

	public void ToggleHighscoreMenu()
	{
		if (weaponMenu.IsEnabled == true)
		{
			weaponMenu.ToggleActiveState();
		}

		ingameUI.gameObject.SetActive(!ingameUI.gameObject.activeInHierarchy);
		highScoreList.UIParent.SetActive(!highScoreList.UIParent.activeInHierarchy);
	}

	public void ToggleWeaponMenu()
	{
		if (highScoreList.UIParent.activeInHierarchy == true)
		{
			ingameUI.gameObject.SetActive(true);
			highScoreList.UIParent.SetActive(false);
		}

		weaponMenu.ToggleActiveState();
		currentWeaponMenuTimer = 0;
	}

	public void SelectWeapon()
	{
		SwitchWeapon(weaponMenu.ReturnCurrentSelectedWeapon());

		if (PhotonNetwork.offlineMode == false)
		{
			photonView.RPC("SwitchWeapon", PhotonTargets.Others, weaponMenu.ReturnCurrentSelectedWeapon()); 
		}

		weaponMenu.ToggleActiveState();
		currentWeaponMenuTimer = 0;
	}

	public void SelectSpecificWeapon(int weaponToSelect)
	{
		if (allWeapons[weaponToSelect].IsAllowedToUse == false)
		{
			return;
		}

		SwitchWeapon(weaponToSelect);
		if (PhotonNetwork.offlineMode == false)
		{
			photonView.RPC("SwitchWeapon", PhotonTargets.Others, weaponToSelect);
		}
		weaponMenu.SelectSpecificWeapon(weaponToSelect);
		currentWeaponMenuTimer = 0;
	}

	public void SelectNextWeapon()
	{
		weaponMenu.MoveWeaponSelectorToTheRight();
		currentWeaponMenuTimer = 0;
	}

	public void SelectPreviousWeapon()
	{
		weaponMenu.MoveWeaponSelectorToTheLeft();
		currentWeaponMenuTimer = 0;
	}

	public Weapon GetWeaponInformation(int weaponToSelect)
	{
		UpdateWeaponUIInfo();
		return allWeapons[weaponToSelect];
	}

	public void TogglePauseGame()
	{
		if (pauseMenuManager.gameObject.activeInHierarchy == false)
		{
			currentWeapon.gameObject.SetActive(false);
			ingameUI.gameObject.SetActive(false);
			highScoreList.UIParent.gameObject.SetActive(false);
			pauseMenuManager.gameObject.SetActive(true);

			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;

			if (PhotonNetwork.offlineMode == true)
			{
				savedTimescale = Time.timeScale;
				Time.timeScale = 0;
			}
		}
		else
		{
			currentWeapon.gameObject.SetActive(true);
			ingameUI.gameObject.SetActive(true);
			pauseMenuManager.gameObject.SetActive(false);

			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;

			if (PhotonNetwork.offlineMode == true)
			{
				Time.timeScale = savedTimescale;
			}
		}
	}

	void UpdateWeaponMenuTimeoutTimer()
	{
		currentWeaponMenuTimer += Time.deltaTime;

		if (currentWeaponMenuTimer > maxWeaponMenuActiveTime)
		{
			ToggleWeaponMenu();
			weaponMenu.ResetWeaponSelector();
		}
	}

	void OnTriggerStay(Collider other)
	{
		Zombie zombie = other.GetComponent<Zombie>();

		if (zombie != null)
		{
			if (zombie.IsAttacking == true)
			{
				zombie.StopAttacking();
				if (PhotonNetwork.offlineMode == false)
				{
					photonView.RPC("RemoveHealth", PhotonTargets.All, zombie.Damage);
				}
				else
				{
					RemoveHealth(zombie.Damage);
				}
			}
		}
	}

	public void AddKillCount(int playerIDToUpdate)
	{
		killCount++;

		if (PhotonNetwork.offlineMode == true)
		{
			highScoreList.AddNumberToKillCountForPlayer(playerIDToUpdate, 1);
		}
		else
		{
			foreach (var player in GameManager.GetInstance().GetNetworkManager().AllRemainingPlayers)
			{
				player.Value.highScoreList.photonView.RPC("AddNumberToKillCountForPlayer", PhotonTargets.All, playerIDToUpdate, 1);
			}
		}

		if (killCount % killsNeededToUnlockNextWeapon == 0)
		{
			UnlockNextWeapon();
		}
	}

	void UnlockNextWeapon()
	{
		int weaponToUnlock = 0;

		for (int i = 0; i < allWeapons.Length; i++)
		{
			if (allWeapons[i].IsAllowedToUse == true)
			{
				weaponToUnlock++;
			}
		}

		if (weaponToUnlock < allWeapons.Length)
		{
			allWeapons[weaponToUnlock].IsAllowedToUse = true;
			allWeapons[weaponToUnlock].AssignStartingAmmo();
		}
	}

	[PunRPC]
	public void AddHealth(float healthToAdd)
	{
		if (photonView.isMine == true)
		{
			photonView.RPC("AddHealth", PhotonTargets.Others, healthToAdd);
		}

		currentHealth += healthToAdd;
		UIHealthBar.UpdateHealthBar(currentHealth);
		inGameHealthBar.UpdateHealthBar(currentHealth);

		if (currentHealth + healthToAdd > startingHealth)
		{
			inGameHealthBar.gameObject.SetActive(false);
		}
	}

	[PunRPC]
	void RemoveHealth(float healthToRemove)
	{
		if (currentHealth - healthToRemove > 0)
		{
			currentHealth -= healthToRemove;

			if (inGameHealthBar.gameObject.activeInHierarchy == false)
			{
				inGameHealthBar.gameObject.SetActive(true);
			}

			inGameHealthBar.UpdateHealthBar(currentHealth);

			UIHealthBar.UpdateHealthBar(currentHealth);

		}
		else
		{
			inGameHealthBar.gameObject.SetActive(false);
			UIHealthBar.UpdateHealthBar(0);
			Die();
		}
	}

	void Die()
	{
		playerController.enabled = false;
		ani.enabled = false;

		for (int i = 0; i < ragdollRigibodys.Length; i++)
		{
			ragdollRigibodys[i].isKinematic = false;
		}

		for (int i = 0; i < colliders.Length; i++)
		{
			colliders[i].enabled = !colliders[i].enabled;
		}

		if (PhotonNetwork.offlineMode == false)
		{
			GameManager.GetInstance().GetNetworkManager().AllRemainingPlayers.Remove(photonView.viewID);
			if (GameManager.GetInstance().GetNetworkManager().AllRemainingPlayers.Count > 1)
			{
				if (photonView.isMine == true)
				{
					CreateSpectatorObject(); 
				}
			}
			else
			{
				OpenEndscreen();
			}
		}
		else
		{
			OpenEndscreen();
		}

		isDead = true;
		enabled = false;
	}

	void CreateSpectatorObject()
	{
		ingameUI.gameObject.SetActive(false);
		playerCameras[1].gameObject.SetActive(false);

		Spectator spectator = Instantiate(spectatorPrefab);
		spectator.HighScoreList = highScoreList;
		spectator.PlayerCamera = playerCameras[0];
	}

	void OpenEndscreen()
	{
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;

		ingameUI.gameObject.SetActive(false);
		highScoreList.UIParent.SetActive(true);
		endscreenUI.gameObject.SetActive(true);
	}

	public void LeaveGame()
	{
		if (PhotonNetwork.offlineMode == false)
		{
			PhotonNetwork.LeaveRoom();
		}

		UnityEngine.SceneManagement.SceneManager.LoadScene(0);
	}

	public void ReloadWeapon()
	{
		if (isReloading == true)
		{
			return;
		}
		else if (currentWeapon.CurrentAmmunitionInMagLeft == currentWeapon.MaxAmmunitionInMagCount)
		{
			isReloading = false;
			return;
		}
		else if (currentWeapon.CurrentTotalAmmunitionLeft == 0)
		{
			isReloading = false;
			return;
		}
		else
		{
			isReloading = true;
			StartCoroutine(ReloadWeaponCoroutine());
		}
	}

	IEnumerator ReloadWeaponCoroutine()
	{
		ani.SetBool(reloadHashID, true);
		isReloading = true;

		yield return new WaitForSeconds(currentWeapon.ReloadTime);

		if (isReloading == true)
		{
			ani.SetBool(reloadHashID, false);
			currentWeapon.ReloadWeapon();
			UpdateWeaponUIInfo();
			isReloading = false;
		}
	}

	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isWriting == true)
		{
			stream.SendNext(transform.position);
		}
		else
		{
			receivedPosition = (Vector3)stream.ReceiveNext();
		}
	}
}