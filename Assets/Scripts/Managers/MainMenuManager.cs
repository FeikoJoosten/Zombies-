using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Steamworks;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class MainMenuManager : Photon.MonoBehaviour
{
	[SerializeField]
	private Slider maxPlayerSlider;
	[SerializeField]
	private Text maxPlayerSliderText;
	[SerializeField]
	private Text roomNameText;
	[SerializeField]
	private Text currentPlayersInRoomText;
	[SerializeField]
	private Text[] inRoomPlayerNames;
	[SerializeField]
	private GameObject findCreateMenu;
	[SerializeField]
	private Button findGameButton;
	[SerializeField]
	private Button createGameButton;
	[SerializeField]
	private Text connectionStateText;
	[SerializeField]
	private RectTransform roomSelectorParent;
	[SerializeField]
	private RoomSelector roomSelectorPrefab;
	[SerializeField]
	private RectTransform roomSelectorMenu;
	[SerializeField]
	private RectTransform inRoomMenu;
	[SerializeField]
	private RectTransform multiPlayerLoadingScreenMenu;
	[SerializeField]
	private RectTransform playerControllsHolder;
	[SerializeField]
	private ControllChanger playerControllerChangerPrefab;
	[SerializeField]
	private Scrollbar controlsMenuSlider;
	[SerializeField]
	private Image loadingBar;
	[SerializeField]
	private Text loadingBarText;
	[SerializeField]
	private LoadingBar[] multiplayerLoadingBars;
	[SerializeField]
	private Button multiplayerStartGameButton;
	[SerializeField]
	private Dropdown screenResolutionDropdown;
	[SerializeField]
	private RectTransform screenResolutionPrefab;
	[SerializeField]
	private RectTransform gameSettingsMenu;
	[SerializeField]
	private Toggle useFullscreenToggle;
	[SerializeField]
	private Dropdown gameQualityDropdown;
	[SerializeField]
	private Dropdown gameTypeDropdown;
	[SerializeField]
	private AudioSource audioSource;
	[SerializeField]
	private AudioClip[] audioClips;
	[SerializeField]
	private Slider SFXVolumeSlider;
	[SerializeField]
	private Slider musicVolumeSlider;
	[SerializeField]
	private Slider traitorPercentageSlider;
	[SerializeField]
	private Slider detectivePercentageSlider;
	[SerializeField]
	private Text SFXVolumeText;
	[SerializeField]
	private Text musicVolumeText;
	[SerializeField]
	private Text traitorPercentageText;
	[SerializeField]
	private Text detectivePercentageText;

	private int currentPlayersInRoom;
	private int skippedResolutions;
	private int currentSongPlaying = -1;
	private float sceneActivationWaitTimer = 3;
	private bool isLoadingScene = false;
	private AsyncOperation asyncOperation;
	private PlayerActions playerActions;

	private Dictionary<Button, Dictionary<InControl.PlayerAction, InControl.BindingSource>> bindingButtons = new Dictionary<Button, Dictionary<InControl.PlayerAction, InControl.BindingSource>>();
	private Dictionary<int, float> playerprogresses = new Dictionary<int, float>();

	void Start()
	{
		#region Audio setup

		GameManager.GetInstance().GetAudioManager().AddMusicAudioSource(audioSource);
		SFXVolumeSlider.value = GameManager.GetInstance().GetAudioManager().SavedSFXVolume * 100;
		musicVolumeSlider.value = GameManager.GetInstance().GetAudioManager().SavedMusicVolume * 100;
		SFXVolumeText.text = SFXVolumeSlider.value.ToString();
		musicVolumeText.text = musicVolumeSlider.value.ToString();

		#endregion

		#region Video menu setup
		for (int i = 0; i < Screen.resolutions.Length; i++)
		{
			if (Screen.resolutions[i].height < 480)
			{
				skippedResolutions++;
			}
		}

		if (Application.platform != RuntimePlatform.WindowsEditor)
		{
			if (PlayerPrefs.HasKey("QualityLevel") && PlayerPrefs.HasKey("IsFullScreen") && PlayerPrefs.HasKey("ScreenResolution"))
			{
				QualitySettings.SetQualityLevel(PlayerPrefs.GetInt("QualityLevel"));
				Screen.SetResolution(Screen.resolutions[PlayerPrefs.GetInt("ScreenResolution") + skippedResolutions].width, Screen.resolutions[PlayerPrefs.GetInt("ScreenResolution") + skippedResolutions].height, PlayerPrefs.GetInt("IsFullScreen") == 1 ? true : false);
			}
		}
		gameQualityDropdown.template.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(0, 50 * QualitySettings.names.Length);
		gameQualityDropdown.ClearOptions();
		screenResolutionDropdown.template.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(0, 50 * Screen.resolutions.Length);
		screenResolutionDropdown.ClearOptions();

		for (int i = 0; i < QualitySettings.names.Length; i++)
		{
			gameQualityDropdown.options.Add(new Dropdown.OptionData() { text = QualitySettings.names[i] });

			if (QualitySettings.GetQualityLevel() == i)
			{
				gameQualityDropdown.value = i;
				gameQualityDropdown.captionText.text = QualitySettings.names[i];
			}
		}

		for (int i = 0; i < Screen.resolutions.Length; i++)
		{
			if (Screen.resolutions[i].height < 480)
			{
				continue;
			}

			if (Screen.fullScreen == false)
			{
				if (Screen.resolutions[i].height == Screen.resolutions[Screen.resolutions.Length - 1].height)
				{
					continue;
				}
			}

			screenResolutionDropdown.options.Add(new Dropdown.OptionData() { text = Screen.resolutions[i].width + " X " + Screen.resolutions[i].height });

			if (Application.platform != RuntimePlatform.WindowsEditor)
			{
				if (Screen.resolutions[i].height == Screen.resolutions[PlayerPrefs.GetInt("ScreenResolution") + skippedResolutions].height && Screen.resolutions[i].width == Screen.resolutions[PlayerPrefs.GetInt("ScreenResolution") + skippedResolutions].width)
				{
					screenResolutionDropdown.value = i;
					screenResolutionDropdown.captionText.text = Screen.resolutions[PlayerPrefs.GetInt("ScreenResolution") + skippedResolutions].width + " X " + Screen.resolutions[PlayerPrefs.GetInt("ScreenResolution") + skippedResolutions].height;
				}
			}
		}

		if (Screen.fullScreen == true)
		{
			useFullscreenToggle.isOn = false;
		}
		else
		{
			useFullscreenToggle.isOn = true;
		}

		screenResolutionDropdown.value = screenResolutionDropdown.options.Count - 1;
		#endregion

		#region controlls menu setup
		playerActions = PlayerActions.CreateWithDefaultBindings();
		InControl.InputManager.OnDeviceAttached += InputManager_OnDeviceAttached;

		if (PlayerPrefs.HasKey("Bindings"))
		{
			playerActions.Load(PlayerPrefs.GetString("Bindings"));
		}

		RedrawControllsMenu();
		#endregion
	}

	private void InputManager_OnDeviceAttached(InControl.InputDevice obj)
	{
		RedrawControllsMenu();
	}

	void Update()
	{
		#region Audio

		if (GameManager.GetInstance().GetAudioManager().SavedMusicVolume > 0)
		{
			if (audioSource.isPlaying == false)
			{
				currentSongPlaying++;

				if (currentSongPlaying == audioClips.Length)
				{
					currentSongPlaying = 0;
				}

				if (audioClips[currentSongPlaying] != null)
				{
					GameManager.GetInstance().GetAudioManager().PlayMusicSound(audioSource, audioClips[currentSongPlaying]);
				}
			}
		}
		else
		{
			if (currentSongPlaying != -1)
			{
				currentSongPlaying = -1;
			}
		}

		#endregion

		#region Is loading
		if (isLoadingScene == true)
		{
			if (inRoomMenu.gameObject.activeInHierarchy == true)
			{
				multiPlayerLoadingScreenMenu.gameObject.SetActive(true);
				inRoomMenu.gameObject.SetActive(false);
			}

			if (PhotonNetwork.offlineMode == true)
			{
				loadingBar.fillAmount = asyncOperation.progress + 0.1F;
				loadingBarText.text = "Loading: " + (Mathf.RoundToInt(asyncOperation.progress * 100) + 10) + "%";
			}
			else
			{
				if (playerprogresses[PhotonNetwork.player.ID] != asyncOperation.progress)
				{
					photonView.RPC("SendLoadingData", PhotonTargets.All, PhotonNetwork.player.ID, asyncOperation.progress);
				}

				for (int i = 0; i < PhotonNetwork.playerList.Length; i++)
				{
					multiplayerLoadingBars[i].OwnerText = PhotonNetwork.playerList[i].name;
					multiplayerLoadingBars[i].LoadingBarProgress = playerprogresses[PhotonNetwork.playerList[i].ID] + 0.1F;
					multiplayerLoadingBars[i].ProgressText = "Loading: " + (Mathf.RoundToInt(playerprogresses[PhotonNetwork.playerList[i].ID] * 100) + 10) + "%";
				}

				int counter = 0;

				foreach (PhotonPlayer player in PhotonNetwork.playerList)
				{
					if (playerprogresses[player.ID] >= 0.9F)
					{
						counter++;
					}
				}

				if (counter == PhotonNetwork.playerList.Length)
				{
					sceneActivationWaitTimer -= Time.deltaTime;

					if (sceneActivationWaitTimer <= 0)
					{
						asyncOperation.allowSceneActivation = true;
					}
				}
			}
		}
		#endregion

		#region In Find/Create menu

		if (findCreateMenu.gameObject.activeInHierarchy == true)
		{
			connectionStateText.text = Enum.GetName(typeof(ConnectionState), PhotonNetwork.connectionState);

			if (PhotonNetwork.connectionState == ConnectionState.Connected)
			{
				findGameButton.interactable = true;
				createGameButton.interactable = true;
			}
			else
			{
				findGameButton.interactable = false;
				createGameButton.interactable = false;
			}
		}
		#endregion

		#region In room
		if (PhotonNetwork.inRoom == true)
		{
			if (currentPlayersInRoom != PhotonNetwork.room.playerCount)
			{
				playerprogresses.Clear();
				
				GameManager.GetInstance().CurrentGameType = (GameTypes)PhotonNetwork.room.customProperties["gameType"];

				string roomName = "Room name: " + PhotonNetwork.room.name + System.Environment.NewLine + "Game mode: ";

				switch (GameManager.GetInstance().CurrentGameType)
				{
					case GameTypes.ZombieMode:
					roomName += "Zombies!";
					break;
					case GameTypes.TeamDeathMatch:
					roomName += "Team Deathmatch";
					break;
					case GameTypes.TTT:
					roomName += "Trouble In Terrorist Town";
					break;
				}

				roomNameText.text = roomName;
				currentPlayersInRoomText.text = PhotonNetwork.room.playerCount.ToString() + "/" + PhotonNetwork.room.maxPlayers.ToString() + " players";

				int counter = 0;
				for (int i = 0; i < PhotonNetwork.room.playerCount; i++)
				{
					if (PhotonNetwork.playerList[i] == PhotonNetwork.masterClient)
					{
						inRoomPlayerNames[i].text = PhotonNetwork.playerList[i].name + " (Host)";
					}
					else
					{
						inRoomPlayerNames[i].text = PhotonNetwork.playerList[i].name;
					}
					counter++;
				}

				for (int i = counter; i < inRoomPlayerNames.Length; i++)
				{
					inRoomPlayerNames[i].text = "";
				}

				playerprogresses = new Dictionary<int, float>(PhotonNetwork.room.playerCount);

				foreach (PhotonPlayer player in PhotonNetwork.playerList)
				{
					playerprogresses.Add(player.ID, 0);
				}

				currentPlayersInRoom = PhotonNetwork.room.playerCount;
			}
		}
		#endregion

		#region Change bindings

		foreach (var button in bindingButtons)
		{
			for (int i = 0; i < button.Value.Count; i++)
			{
				var action = button.Value.ElementAt(i);

				if (action.Key.IsListeningForBinding == false)
				{
					button.Key.GetComponentInChildren<Text>().text = action.Value.Name;
					continue;
				}
				else
				{
					if ((action.Key.ListenOptions.IncludeControllers == true && action.Value.BindingSourceType == InControl.BindingSourceType.DeviceBindingSource) || (action.Key.ListenOptions.IncludeKeys == true && action.Value.BindingSourceType == InControl.BindingSourceType.KeyBindingSource) || (action.Key.ListenOptions.IncludeMouseButtons == true && action.Value.BindingSourceType == InControl.BindingSourceType.MouseBindingSource))
					{
						button.Key.GetComponentInChildren<Text>().text = "Listening";
					}
					else
					{
						button.Key.GetComponentInChildren<Text>().text = action.Value.Name;
					}

					action.Key.ListenOptions.OnBindingAdded = (act, binding) =>
					{
						RedrawControllsMenu();
					};
				}
			}
		}

		#endregion
	}

	void RedrawControllsMenu()
	{
		foreach (InControl.PlayerAction action in playerActions.Actions)
		{
			action.StopListeningForBinding();
		}

		bindingButtons.Clear();

		for (int j = playerControllsHolder.childCount - 1; j >= 0; j--)
		{
			Transform obj = playerControllsHolder.GetChild(j);
			Destroy(obj.GetComponent<LayoutElement>());
			Destroy(obj.GetComponent<HorizontalLayoutGroup>());
			obj.SetParent(null);
			Destroy(obj.gameObject);
		}

		for (int i = 0; i < playerActions.Actions.Count; i++)
		{
			var action = playerActions.Actions[i];

			if (action == playerActions.rotateUp || action == playerActions.rotateDown || action == playerActions.rotateLeft || action == playerActions.rotateRight)
			{
				continue;
			}

			ControllChanger changer = Instantiate(playerControllerChangerPrefab);

			changer.transform.SetParent(playerControllsHolder.transform);
			changer.transform.localScale = Vector3.one;
			changer.GetComponent<HorizontalLayoutGroup>().padding.top = changer.GetComponent<HorizontalLayoutGroup>().padding.top * playerControllsHolder.childCount;
			changer.ControllName = action.Name;

			for (int j = 0; j < action.Bindings.Count; j++)
			{
				var acti = new Dictionary<InControl.PlayerAction, InControl.BindingSource>();
				acti.Add(action, action.Bindings[j]);

				Button button = Instantiate(changer.ControllBinding);
				bindingButtons.Add(button, acti);
				button.GetComponent<RebindButton>().action = action;
				button.GetComponent<RebindButton>().binding = action.Bindings[j];
				button.transform.SetParent(changer.transform);
				button.transform.localScale = Vector3.one;
				button.GetComponentInChildren<Text>().text = action.Bindings[j].Name;
			}
		}

		playerControllsHolder.sizeDelta = new Vector2(0, (playerControllerChangerPrefab.GetComponent<LayoutElement>().minHeight * playerControllsHolder.childCount) + (playerControllsHolder.GetComponent<VerticalLayoutGroup>().padding.top * 2));

	}

	public void StartSinglePlayer(int levelToLoad)
	{
		if (SteamManager.Initialized == true)
		{
			PhotonNetwork.player.name = SteamFriends.GetPersonaName();
		}
		else
		{
			PhotonNetwork.player.name = System.Environment.UserName;
		}

		PhotonNetwork.offlineMode = true;
		isLoadingScene = true;
		asyncOperation = SceneManager.LoadSceneAsync(levelToLoad, LoadSceneMode.Single);
	}

	[PunRPC]
	public void StartMultiPlayerGame(int levelToLoad)
	{
		if (PhotonNetwork.isMasterClient == true)
		{
			photonView.RPC("StartMultiPlayerGame", PhotonTargets.Others, levelToLoad);
		}

		isLoadingScene = true;
		asyncOperation = GameManager.GetInstance().GetNetworkManager().StartGame(levelToLoad);
		asyncOperation.allowSceneActivation = false;

		for (int i = 0; i < PhotonNetwork.playerList.Length; i++)
		{
			multiplayerLoadingBars[i].gameObject.SetActive(true);
		}
	}

	[PunRPC]
	void SendLoadingData(int playerID, float progress)
	{
		if (playerprogresses.ContainsKey(playerID) == true)
		{
			//Debug.Log(playerID + " " + progress);
			playerprogresses[playerID] = progress;
		}
	}

	void SetupMultiplayerLoadingBars()
	{
		for (int i = 0; i < PhotonNetwork.room.playerCount; i++)
		{
			multiplayerLoadingBars[i].gameObject.SetActive(true);
			multiplayerLoadingBars[i].OwnerText = PhotonNetwork.playerList[i].name;
		}
	}

	public void SaveAudioSettings()
	{
		GameManager.GetInstance().GetAudioManager().UpdateAudioVolumes(SFXVolumeSlider.normalizedValue, musicVolumeSlider.normalizedValue);
	}

	public void GetAudioSettings()
	{
		SFXVolumeSlider.value = GameManager.GetInstance().GetAudioManager().SavedSFXVolume * 100;
		musicVolumeSlider.value = GameManager.GetInstance().GetAudioManager().SavedMusicVolume * 100;
	}

	public void UpdateSFXVolumeTextLabel()
	{
		SFXVolumeText.text = SFXVolumeSlider.value.ToString();
	}

	public void UpdateMusicVolumeTextLabel()
	{
		musicVolumeText.text = musicVolumeSlider.value.ToString();
	}

	public void JoinMasterServer()
	{
		if (PhotonNetwork.connected == false)
		{
			PhotonNetwork.offlineMode = false;
			PhotonNetwork.ConnectUsingSettings("V1.0");
		}

		if (SteamManager.Initialized == true)
		{
			PhotonNetwork.player.name = SteamFriends.GetPersonaName();
		}
		else
		{
			PhotonNetwork.player.name = System.Environment.UserName;
		}
	}

	public void LeaveMasterServer()
	{
		if (PhotonNetwork.connected)
		{
			PhotonNetwork.Disconnect();
		}
	}

	public void CreateMultiplayerRoom()
	{
		switch (gameTypeDropdown.value)
		{
			case 0:
			GameManager.GetInstance().CurrentGameType = GameTypes.ZombieMode;
			break;
			case 1:
			GameManager.GetInstance().CurrentGameType = GameTypes.TeamDeathMatch;
			break;
			case 2:
			GameManager.GetInstance().CurrentGameType = GameTypes.TTT;
			GameManager.GetInstance().TerroristSpawnRate = traitorPercentageSlider.value;
			GameManager.GetInstance().DetectiveSpawnRate = detectivePercentageSlider.value;
			break;
		}

		GameManager.GetInstance().GetNetworkManager().CreateRoom(true, true, (int)maxPlayerSlider.value);
		multiplayerStartGameButton.gameObject.SetActive(true);
	}

	public void GetRoomList()
	{
		RoomInfo[] rooms = PhotonNetwork.GetRoomList();
		for (int i = 0; i < rooms.Length; i++)
		{
			if (rooms[i].playerCount == rooms[i].maxPlayers)
			{
				continue;
			}

			RoomSelector room = Instantiate(roomSelectorPrefab);
			room.RoomName = rooms[i].name;

			if (rooms[i].customProperties.ContainsKey("gameType") == true)
			{
				switch ((GameTypes)rooms[i].customProperties["gameType"])
				{
					case GameTypes.ZombieMode:
					room.GameType = "Zombies!";
					break;
					case GameTypes.TeamDeathMatch:
					room.GameType = "Team Deathmatch";
					break;
					case GameTypes.TTT:
					room.GameType = "Trouble In Terrorist Town";
					break;
				}
			}

			room.CurrentPlayerCount = rooms[i].playerCount.ToString();
			room.MaxPlayerCount = rooms[i].maxPlayers.ToString();
			room.transform.SetParent(roomSelectorParent.transform);
			room.transform.localScale = Vector3.one;
			room.GetComponent<Button>().onClick.AddListener(() => JoinRoom(room.RoomName));
			room.GetComponent<Button>().onClick.AddListener(() => SwitchToInRoom());
		}

		roomSelectorParent.sizeDelta = new Vector2(0, roomSelectorPrefab.GetComponent<RectTransform>().rect.height * rooms.Length);
	}

	public void RefreshRoomList()
	{
		ClearRoomList();
		GetRoomList();
	}

	public void ClearRoomList()
	{
		for (int i = roomSelectorParent.childCount - 1; i >= 0; i--)
		{
			roomSelectorParent.GetChild(i).GetComponent<Button>().onClick.RemoveAllListeners();
			Destroy(roomSelectorParent.GetChild(i).gameObject);
		}
	}

	public void LeaveRoom()
	{
		if (PhotonNetwork.inRoom == true)
		{
			ClearRoomList();
			currentPlayersInRoom = 0;
			roomNameText.text = "";
			GameManager.GetInstance().GetNetworkManager().LeaveRoom();
		}
	}

	public void UpdateRoomCreationSettingsMenu()
	{
		if(gameTypeDropdown.value == 0)			//Zombies!
		{
			if (gameSettingsMenu.gameObject.activeInHierarchy == true)
			{
				gameSettingsMenu.gameObject.SetActive(false);
			}
		}
		else if (gameTypeDropdown.value == 1)	//Team Deathmatch
		{
			if (gameSettingsMenu.gameObject.activeInHierarchy == true)
			{
				gameSettingsMenu.gameObject.SetActive(false); 
			}
		}
		else if(gameTypeDropdown.value == 2)	//Trouble in Terrorist Town
		{
			if (gameSettingsMenu.gameObject.activeInHierarchy == false)
			{
				gameSettingsMenu.gameObject.SetActive(true);
			}

			if(traitorPercentageSlider.value == 0)
			{
				traitorPercentageText.text = "0";
			}
			else if(traitorPercentageSlider.value == 1)
			{
				traitorPercentageText.text = "1";
			}
			else
			{
				traitorPercentageText.text = traitorPercentageSlider.value.ToString("n3"); 
			}

			if (detectivePercentageSlider.value == 0)
			{
				detectivePercentageText.text = "0";
			}
			else if (detectivePercentageSlider.value == 1)
			{
				detectivePercentageText.text = "1";
			}
			else
			{
				detectivePercentageText.text = detectivePercentageSlider.value.ToString("n3");
			}
		}
	}

	public void ResetRoomCreationSettingsMenu()
	{
		if(gameTypeDropdown.value == 0)			//Zombies!
		{
			return;
		}
		else if(gameTypeDropdown.value == 1)	//Team Deathmatch
		{
			return;
		}
		else if(gameTypeDropdown.value == 2)	//Trouble in Terrorist Town
		{
			traitorPercentageSlider.value = 0.250F;
			detectivePercentageSlider.value = 0.125F;
			return;
		}
	}

	void JoinRoom(string roomName)
	{
		for (int i = 0; i < PhotonNetwork.GetRoomList().Length; i++)
		{
			if (PhotonNetwork.GetRoomList()[i].name == roomName)
			{
				if (PhotonNetwork.GetRoomList()[i].visible == false)
				{
					RefreshRoomList();
					return;
				}
			}
		}

		GameManager.GetInstance().GetNetworkManager().JoinRoom(roomName);
		multiplayerStartGameButton.gameObject.SetActive(false);
	}

	void SwitchToInRoom()
	{
		inRoomMenu.gameObject.SetActive(true);
		roomSelectorMenu.gameObject.SetActive(false);
	}

	public void UpdateMaxPlayerSliderText()
	{
		maxPlayerSliderText.text = maxPlayerSlider.value.ToString();
	}

	public void OnFullscreenToggle()
	{
		int savedValue = screenResolutionDropdown.value;

		if (useFullscreenToggle.isOn == true)
		{
			screenResolutionDropdown.ClearOptions();

			for (int i = 0; i < Screen.resolutions.Length; i++)
			{
				if (Screen.resolutions[i].height < 480)
				{
					continue;
				}

				if (Screen.resolutions[i].height == Screen.resolutions[Screen.resolutions.Length - 1].height)
				{
					continue;
				}

				screenResolutionDropdown.options.Add(new Dropdown.OptionData() { text = Screen.resolutions[i].width + " X " + Screen.resolutions[i].height });
			}

			if (screenResolutionDropdown.options.Count >= savedValue)
			{
				screenResolutionDropdown.value = savedValue;
				screenResolutionDropdown.captionText.text = Screen.resolutions[savedValue + skippedResolutions].width + " X " + Screen.resolutions[savedValue + skippedResolutions].height;
			}
			else
			{
				screenResolutionDropdown.value = screenResolutionDropdown.options.Count - 1;
				screenResolutionDropdown.captionText.text = screenResolutionDropdown.options[screenResolutionDropdown.options.Count - 1].text;
			}
		}
		else
		{
			screenResolutionDropdown.ClearOptions();

			for (int i = 0; i < Screen.resolutions.Length; i++)
			{
				if (Screen.resolutions[i].height < 480)
				{
					continue;
				}

				screenResolutionDropdown.options.Add(new Dropdown.OptionData() { text = Screen.resolutions[i].width + " X " + Screen.resolutions[i].height });
			}

			int temp = 0;
			for (int i = 0; i < savedValue; i++)
			{
				if (Screen.resolutions[skippedResolutions + i].height >= Screen.resolutions[Screen.resolutions.Length - 1].height)
				{
					temp++;
				}
			}

			if (screenResolutionDropdown.options[savedValue + temp] != null)
			{
				screenResolutionDropdown.value = savedValue + temp;
				screenResolutionDropdown.captionText.text = Screen.resolutions[savedValue + temp + skippedResolutions].width + " X " + Screen.resolutions[savedValue + temp + skippedResolutions].height;
			}
			else
			{
				screenResolutionDropdown.value = screenResolutionDropdown.options.Count;
				screenResolutionDropdown.captionText.text = Screen.resolutions[screenResolutionDropdown.options.Count + skippedResolutions].width + " X " + Screen.resolutions[screenResolutionDropdown.options.Count + skippedResolutions].height;
			}
		}
	}

	public void ChangeVideoSettings()
	{
		bool isChanged = false;

		if (QualitySettings.GetQualityLevel() != gameQualityDropdown.value)
		{
			QualitySettings.SetQualityLevel(gameQualityDropdown.value);
			isChanged = true;
		}

		if (useFullscreenToggle.isOn == true)
		{
			Screen.fullScreen = false;
		}
		else
		{
			Screen.fullScreen = true;
		}

		if (Screen.resolutions[screenResolutionDropdown.value + skippedResolutions].height != Screen.currentResolution.height || Screen.resolutions[screenResolutionDropdown.value + skippedResolutions].width != Screen.currentResolution.width)
		{
			Screen.SetResolution(Screen.resolutions[screenResolutionDropdown.value + skippedResolutions].width, Screen.resolutions[screenResolutionDropdown.value + skippedResolutions].height, !useFullscreenToggle.isOn);
			isChanged = true;
		}

		if (isChanged == true)
		{
			PlayerPrefs.SetInt("QualityLevel", QualitySettings.GetQualityLevel());
			PlayerPrefs.SetInt("ScreenResolution", screenResolutionDropdown.value);
			PlayerPrefs.SetInt("IsFullScreen", useFullscreenToggle.isOn == true ? 0 : 1);
			PlayerPrefs.Save();
		}
	}

	void OnApplicationQuit()
	{
		PlayerPrefs.SetInt("QualityLevel", QualitySettings.GetQualityLevel());
		PlayerPrefs.SetInt("ScreenResolution", screenResolutionDropdown.value);
		PlayerPrefs.SetInt("IsFullScreen", useFullscreenToggle.isOn == true ? 0 : 1);
		PlayerPrefs.Save();
	}

	public void ChangeBinding(KeyValuePair<InControl.PlayerAction, InControl.BindingSource> actionAndBinding)
	{
		//UnityEngine.EventSystems.EventSystem.current.
		foreach (InControl.PlayerAction action in playerActions.Actions)
		{
			action.StopListeningForBinding();
		}

		InControl.BindingListenOptions options = new InControl.BindingListenOptions();

		foreach (Dictionary<InControl.PlayerAction, InControl.BindingSource> i in bindingButtons.Values)
		{
			foreach (var value in i.Values)
			{
				if (value != actionAndBinding.Value)
				{
					continue;
				}

				options.IncludeModifiersAsFirstClassKeys = true;

				if (value.BindingSourceType == InControl.BindingSourceType.DeviceBindingSource)
				{
					options.IncludeControllers = true;
					options.IncludeKeys = false;
					options.IncludeMouseButtons = false;
				}
				else if (value.BindingSourceType == InControl.BindingSourceType.KeyBindingSource)
				{
					options.IncludeControllers = false;
					options.IncludeKeys = true;
					options.IncludeMouseButtons = true;
				}
				else
				{
					options.IncludeControllers = false;
					options.IncludeKeys = true;
					options.IncludeMouseButtons = true;
				}

				actionAndBinding.Key.ListenOptions = options;

				actionAndBinding.Key.ListenForBindingReplacing(actionAndBinding.Value);
			}
		}
	}

	public void SaveBindings()
	{
		PlayerPrefs.SetString("Bindings", playerActions.Save());
	}

	public void ResetBindings()
	{
		playerActions.Reset();
		RedrawControllsMenu();
	}

	public void ExitGame()
	{
		Application.Quit();
	}
}