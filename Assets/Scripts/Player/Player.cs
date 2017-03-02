using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : OverridableMonoBehaviour
{
	[SerializeField]
	private int killsNeededToUnlockNextWeapon = 45;
	[SerializeField]
	private float startingHealth = 100;
	[SerializeField]
	private float movementSpeed = 0;
	[SerializeField]
	private float sprintSpeed = 0;
	[SerializeField]
	private float sprintEndurance = 0;
	[SerializeField]
	private float sprintEnduranceLoweringSpeed = 0;
	[SerializeField]
	private float sprintEnduranceRaisingSpeed = 0;
	[SerializeField]
	private float rotationSpeed = 0;
	[SerializeField]
	private float maxYRotation = 0;
	[SerializeField]
	private float weaponDropSpeed = 20;
	[SerializeField]
	private Weapon currentWeapon;
	[SerializeField]
	private Weapon[] allWeapons = null;
	[SerializeField]
	private WeaponMenu weaponMenu = null;
	[SerializeField]
	private AmmoInfoUI ammoInfoUI = null;
	[SerializeField]
	private float maxWeaponMenuActiveTime = 0;
	[SerializeField]
	private Animator ani = null;
	[SerializeField]
	private Healthbar UIHealthBar = null;
	[SerializeField]
	private Healthbar inGameHealthBar = null;
	[SerializeField]
	private HighscoreList highScoreList = null;
	[SerializeField]
	private RectTransform ingameUI = null;
	[SerializeField]
	private RectTransform endscreenUI = null;
	[SerializeField]
	private DeathInformation deathInformationUI = null;
	[SerializeField]
	private Camera[] playerCameras = null;
	[SerializeField]
	private Rigidbody[] ragdollRigibodys = null;
	[SerializeField]
	private Collider[] colliders = null;
	[SerializeField]
	private Collider deathInformationCollider = null;
	[SerializeField]
	private PlayerController playerController = null;
	[SerializeField]
	private Spectator spectatorPrefab = null;
	[SerializeField]
	private PauseMenuManager pauseMenuManager = null;
	[SerializeField]
	private GameObject[] nonRendarableBodyParts = null;
	[SerializeField]
	private GameObject[] allBodyParts = null;
	[SerializeField]
	private GameObject minimapDot = null;
	[SerializeField]
	private ThrowableWeaponModel[] weaponModels = null;
	[SerializeField]
	private UnityEngine.UI.Text searchBodyText = null;

	private float currentHealth;
	private float currentWeaponMenuTimer;
	private float savedTimescale;
	private float currentSprintEndurance;
	private int reloadHashID;
	private int cameraRotationXHashID;
	private int sprintingHashID;
	private int walkingHashID;
	private int killCount;
	private int karmaCount = 1000;
	private bool isReloading;
	private bool isDead;
	private bool hasLoadedSettings;
	private bool isAllowedToLookAtDeathInformation;
	private Rigidbody rig;
	private Vector3 receivedPosition;
	private Vector3 headRotation;
	private Weapon currentCarriedWeapon;
	private PlayerBodyType lastHitBodypart;
	private WeaponType lastDamageReceivedByWeapon;
	private TTTTeams currentTTTTeam;
	private DeathInformationCollider lastDeathInformationColliderFound;

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
	public int KarmaCount
	{
		get { return karmaCount; }
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
	public bool IsAllowedToLookAtDeathInformation
	{
		get { return isAllowedToLookAtDeathInformation; }
	}
	public TTTTeams CurrentTTTTeam
	{
		get { return currentTTTTeam; }
	}
	public GameObject MinimapDot
	{
		get { return minimapDot; }
	}
	public Camera[] PlayerCameras
	{
		get { return playerCameras; }
	}

	private void Start()
	{
		GetAnimationHashIDs();
		currentWeapon = allWeapons[0];
		currentHealth = startingHealth;
		rig = GetComponent<Rigidbody>();

		for (int i = 0; i < ragdollRigibodys.Length; i++)
		{
			ragdollRigibodys[i].isKinematic = true;
		}

		if (GameManager.GetInstance().CurrentGameType != GameTypes.TTT)
		{
			searchBodyText.gameObject.SetActive(false);
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

	private void AssignMPSettings()
	{
		if (hasLoadedSettings != false) return;

		if (photonView.owner.Equals(PhotonNetwork.player))
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

		AssignCharacterSkin();

		hasLoadedSettings = true;
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
		gameObject.name = PhotonNetwork.player.NickName + (PhotonNetwork.offlineMode == false ? (PhotonNetwork.isMasterClient == true ? " (Host)" : "") : "");

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

		currentSprintEndurance = sprintEndurance;

		UpdateWeaponUIInfo();
	}

	public void AssignStartValuesForOthers()
	{
		gameObject.name = PhotonPlayer.Find(photonView.OwnerActorNr).NickName + (PhotonPlayer.Find(photonView.OwnerActorNr).IsMasterClient == true ? " (Host)" : "");

		for (int i = 0; i < playerCameras.Length; i++)
		{
			playerCameras[i].gameObject.SetActive(false);
		}

		playerController.enabled = false;
	}

	private void AssignCharacterSkin()
	{
		if (!PhotonNetwork.player.CustomProperties.ContainsKey("playerSkin")) return;

		foreach (Player photonPlayer in GameManager.GetInstance().GetNetworkManager().AllRemainingPlayers.Values)
		{
			if (photonPlayer.photonView.viewID != photonView.viewID)
			{
				continue;
			}

			for (int i = 0; i < allBodyParts.Length; i++)
			{
				allBodyParts[i].GetComponent<SkinnedMeshRenderer>().material.SetTexture("_MainTex", GameManager.GetInstance().AllPlayerSkins[(int)PhotonPlayer.Find(photonView.ownerId).CustomProperties["playerSkin"]]);
			}
		}
	}

	private void GetAnimationHashIDs()
	{
		sprintingHashID = Animator.StringToHash("Sprinting");
		walkingHashID = Animator.StringToHash("Walking");
		reloadHashID = Animator.StringToHash("Reload");
		cameraRotationXHashID = Animator.StringToHash("CameraRotationX");
	}

	public override void UpdateMe()
	{
		if (PhotonNetwork.playerList.Length != GameManager.GetInstance().GetNetworkManager().AllRemainingPlayers.Count)
			return;

		if (PhotonNetwork.offlineMode == false)
		{
			AssignMPSettings();
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
			if (photonView.owner.Equals(PhotonNetwork.player))
			{
				if (weaponMenu.IsEnabled == true)
				{
					UpdateWeaponMenuTimeoutTimer();
				}

				if (playerController.IsSprinting != false) return;
				if (!(currentSprintEndurance < sprintEndurance)) return;

				if (currentSprintEndurance + sprintEnduranceRaisingSpeed * Time.deltaTime < sprintEndurance)
				{
					currentSprintEndurance += sprintEnduranceRaisingSpeed * Time.deltaTime;
				}
				else
				{
					currentSprintEndurance = sprintEndurance;
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

			if (playerController.IsSprinting != false) return;

			if (currentSprintEndurance < sprintEndurance)
			{
				currentSprintEndurance += sprintEnduranceRaisingSpeed * Time.deltaTime;
			}
		}
	}

	public void FireWeapon()
	{
		if (isReloading != false || Time.timeScale == 0) return;

		currentWeapon.Fire();
		UpdateWeaponUIInfo();
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

		if (Physics.Raycast(transform.position + new Vector3(0, 0.75F, 0), movement.normalized, 0.25F) != false) return;

		if (isSprinting == true)
		{
			if (value.x == 0 && value.y == 0) return;

			if (currentSprintEndurance <= 0)
			{
				rig.MovePosition(transform.position + (movement.normalized * movementSpeed * Time.deltaTime));

			}
			else
			{
				if (currentSprintEndurance - sprintEnduranceLoweringSpeed * Time.deltaTime > 0)
				{
					currentSprintEndurance -= sprintEnduranceLoweringSpeed * Time.deltaTime;
				}
				else
				{
					currentSprintEndurance = 0;
				}
				rig.MovePosition(transform.position + (movement.normalized * sprintSpeed * Time.deltaTime));
			}
		}
		else
		{
			rig.MovePosition(transform.position + (movement.normalized * movementSpeed * Time.deltaTime));
		}
	}

	[PunRPC]
	public void UpdateRotation(Vector2 value)
	{
		Vector3 playerRotation = new Vector3(0, value.y, 0);
		transform.Rotate(playerRotation * rotationSpeed);

		playerCameras[0].gameObject.transform.Rotate(value.x * rotationSpeed, 0, 0);
		headRotation = playerCameras[0].gameObject.transform.rotation.eulerAngles;
		Vector3 bodyRotation = transform.rotation.eulerAngles;
		playerCameras[0].gameObject.transform.rotation = Quaternion.Euler(
			new Vector3(Mathf.Clamp(headRotation.x > 180 ? headRotation.x - 360 : headRotation.x, -maxYRotation, maxYRotation),
			bodyRotation.y, bodyRotation.z));

		if (cameraRotationXHashID != 0)
		{
			ani.SetFloat(cameraRotationXHashID, headRotation.x > 180 ? headRotation.x - 360 : headRotation.x);
		}
	}

	private void UpdateWeaponUIInfo()
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
	private void SwitchWeapon(int nextWeapon)
	{
		if (isReloading == true)
		{
			ani.SetBool(reloadHashID, false);
			isReloading = false;
		}

		if (currentWeapon != null)
		{
			currentWeapon.IsWaitTimerFinished = true;
			currentWeapon.gameObject.SetActive(false);
		}
		currentWeapon = allWeapons[nextWeapon];
		currentWeapon.gameObject.SetActive(true);
		UpdateWeaponUIInfo();
	}

	[PunRPC]
	private void UpdateTeamStatus(TTTTeams assignedTeam)
	{
		if (PhotonNetwork.isMasterClient == false)
		{
			GameManager.GetInstance().TTTWarmingUp = false;
		}

		currentTTTTeam = assignedTeam;
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

	private int GetSelectedWeapon()
	{
		int selectedWeapon = 0;
		for (int i = 0; i < allWeapons.Length; i++)
		{
			if (currentCarriedWeapon == allWeapons[i])
			{
				selectedWeapon = i;
			}
		}

		return selectedWeapon;
	}

	public void PickupWeapon(int pickedUpWeapon)
	{
		if (currentCarriedWeapon != null || currentCarriedWeapon == GetWeaponInformation(pickedUpWeapon)) return;

		allWeapons[pickedUpWeapon].IsAllowedToUse = true;
		SelectSpecificWeapon(pickedUpWeapon);
	}

	public void ThrowWeaponAway()
	{
		if (currentCarriedWeapon == null || GameManager.GetInstance().CurrentGameType != GameTypes.TTT)
		{
			return;
		}

		currentCarriedWeapon.IsAllowedToUse = false;
		currentCarriedWeapon.ClearAmmoPile();

		photonView.RPC("ThrowWeapon", PhotonTargets.All);

		currentCarriedWeapon = null;

		SelectSpecificWeapon(0);
	}

	[PunRPC]
	private void ThrowWeapon()
	{
		ThrowableWeaponModel oldWeapon = (ThrowableWeaponModel)Instantiate(weaponModels[GetSelectedWeapon()], currentWeapon.transform.position, weaponModels[GetSelectedWeapon()].transform.rotation);
		oldWeapon.GetComponent<Rigidbody>().AddForce((transform.forward + headRotation.normalized).normalized * weaponDropSpeed);
		oldWeapon.WeaponNumber = GetSelectedWeapon();
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

			if (PhotonNetwork.offlineMode != true) return;

			savedTimescale = Time.timeScale;
			Time.timeScale = 0;
		}
		else
		{
			currentWeapon.gameObject.SetActive(true);
			ingameUI.gameObject.SetActive(true);
			pauseMenuManager.ResetPauseMenuManager();
			pauseMenuManager.gameObject.SetActive(false);

			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;

			if (PhotonNetwork.offlineMode == true)
			{
				Time.timeScale = savedTimescale;
			}
		}
	}

	private void UpdateWeaponMenuTimeoutTimer()
	{
		currentWeaponMenuTimer += Time.deltaTime;

		if (!(currentWeaponMenuTimer > maxWeaponMenuActiveTime)) return;

		ToggleWeaponMenu();
		weaponMenu.ResetWeaponSelector();
	}

	private void OnTriggerStay(Collider other)
	{
		Zombie zombie = other.GetComponent<Zombie>();
		DeathInformationCollider deathInformation = other.GetComponent<DeathInformationCollider>();

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

		if (deathInformation != null)
		{
			if (deathInformation.gameObject == deathInformationCollider.gameObject) return;

			isAllowedToLookAtDeathInformation = true;

			if (lastDeathInformationColliderFound != deathInformation)
			{
				lastDeathInformationColliderFound = deathInformation;
			}

			searchBodyText.text = "Press the search body button to search the body";
		}
		else
		{
			isAllowedToLookAtDeathInformation = false;
			searchBodyText.text = "";
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

	private void UnlockNextWeapon()
	{
		int weaponToUnlock = 0;

		for (int i = 0; i < allWeapons.Length; i++)
		{
			if (allWeapons[i].IsAllowedToUse == true)
			{
				weaponToUnlock++;
			}
		}

		if (weaponToUnlock >= allWeapons.Length) return;

		allWeapons[weaponToUnlock].IsAllowedToUse = true;
		allWeapons[weaponToUnlock].AssignStartingAmmo();
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
	private void RemoveHealth(float healthToRemove)
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

	[PunRPC]
	public void RemoveHealthFromPlayerBody(float healthToRemove, PlayerBodyType bodyTypeThatReceivedDamage, WeaponType weaponTypeThatGaveDamage)
	{
		lastHitBodypart = bodyTypeThatReceivedDamage;
		lastDamageReceivedByWeapon = weaponTypeThatGaveDamage;

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

	[PunRPC]
	public void RemoveKarmaPoints(int karmaToRemove, int playerIDToUpdate)
	{
		karmaCount -= karmaToRemove;
		highScoreList.AddNumberToKarmaCountForPlayer(playerIDToUpdate, -karmaToRemove);
	}

	public KeyValuePair<PlayerBodyType, WeaponType> GetDeathInformation()
	{
		return new KeyValuePair<PlayerBodyType, WeaponType>(lastHitBodypart, lastDamageReceivedByWeapon);
	}

	private void Die()
	{
		playerController.enabled = false;
		ani.enabled = false;
		deathInformationCollider.gameObject.SetActive(true);

		if (GameManager.GetInstance().CurrentGameType == GameTypes.TTT)
		{
			GameManager.GetInstance().GetNetworkManager().RemovePlayerFromTeam(this);
			photonView.RPC("ReceiveDeathInformation", PhotonTargets.All, photonView.viewID);
		}

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
			if (GameManager.GetInstance().GetNetworkManager().AllRemainingPlayers.Count > 0)
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

	[PunRPC]
	private void ReceiveDeathInformation(int playerID)
	{
		highScoreList.ChangePlayersTeamTabOnDeath(playerID, currentTTTTeam);
	}

	[PunRPC]
	private void ConfirmDeathForPlayer(int playerID)
	{
		highScoreList.ChangePlayersTeamTabOnDeathConfirmation(playerID);
	}

	public void OpenDeathInformationMenu()
	{
		deathInformationUI.CreateDeathInformation(lastDeathInformationColliderFound.Player.currentTTTTeam, lastDeathInformationColliderFound.Player.lastDamageReceivedByWeapon, lastDeathInformationColliderFound.Player.lastHitBodypart);

		highScoreList.ChangePlayersTeamTabOnDeathConfirmation(photonView.viewID);

		ToggleDeathInformationUI();
	}

	private void ToggleDeathInformationUI()
	{
		if (ingameUI.gameObject.activeInHierarchy == true)
		{
			currentWeapon.gameObject.SetActive(false);
			highScoreList.UIParent.gameObject.SetActive(false);
			ingameUI.gameObject.SetActive(false);
			deathInformationUI.gameObject.SetActive(true);

			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
		else
		{
			currentWeapon.gameObject.SetActive(true);
			ingameUI.gameObject.SetActive(true);
			deathInformationUI.gameObject.SetActive(false);

			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
	}

	private void CreateSpectatorObject()
	{
		ingameUI.gameObject.SetActive(false);
		playerCameras[1].gameObject.SetActive(false);

		Spectator spectator = Instantiate(spectatorPrefab);
		spectator.HighScoreList = highScoreList;
		spectator.PlayerCamera = playerCameras[0];
	}

	private void OpenEndscreen()
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
		if (isReloading == true) return;

		if (currentWeapon.CurrentAmmunitionInMagLeft == currentWeapon.MaxAmmunitionInMagCount || currentWeapon.CurrentTotalAmmunitionLeft == 0)
		{
			isReloading = false;
			return;
		}

		isReloading = true;
		StartCoroutine(ReloadWeaponCoroutine());
	}

	private IEnumerator ReloadWeaponCoroutine()
	{
		ani.SetBool(reloadHashID, true);
		isReloading = true;

		yield return new WaitForSeconds(currentWeapon.ReloadTime);

		if (isReloading == false)
		{
			StopCoroutine(ReloadWeaponCoroutine());
		}
		else
		{
			ani.SetBool(reloadHashID, false);
			currentWeapon.ReloadWeapon();
			UpdateWeaponUIInfo();
			isReloading = false;
		}
	}

	public void SetupTTTTeamColors()
	{
		highScoreList.SetupTTTTeamColors();
	}

	private void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
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