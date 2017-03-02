using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Steamworks;
using System;
using System.Linq;
using System.Collections.Generic;

public class MainMenuManager : Photon.MonoBehaviour
{
	[SerializeField]
	private Slider maxPlayerSlider = null;
	[SerializeField]
	private Text maxPlayerSliderText = null;
	[SerializeField]
	private Text roomNameText = null;
	[SerializeField]
	private Text currentPlayersInRoomText = null;
	[SerializeField]
	private Text inRoomPlayerName = null;
	[SerializeField]
	private GameObject findCreateMenu = null;
	[SerializeField]
	private Button findGameButton = null;
	[SerializeField]
	private Button createGameButton = null;
	[SerializeField]
	private Text connectionStateText = null;
	[SerializeField]
	private RectTransform roomSelectorParent = null;
	[SerializeField]
	private RoomSelector roomSelectorPrefab = null;
	[SerializeField]
	private RectTransform roomSelectorMenu = null;
	[SerializeField]
	private RectTransform inRoomMenu = null;
	[SerializeField]
	private RectTransform multiPlayerLoadingScreenMenu = null;
	[SerializeField]
	private RectTransform playerControllsHolder = null;
	[SerializeField]
	private ControllChanger playerControllerChangerPrefab = null;
	//[SerializeField]
	//private Scrollbar controlsMenuSlider = null;
	[SerializeField]
	private Image loadingBar = null;
	[SerializeField]
	private Text loadingBarText = null;
	[SerializeField]
	private LoadingBar multiplayerLoadingBar = null;
	[SerializeField]
	private Button multiplayerStartGameButton = null;
	[SerializeField]
	private Dropdown screenResolutionDropdown = null;
	//[SerializeField]
	//private RectTransform screenResolutionPrefab = null;
	[SerializeField]
	private RectTransform gameSettingsMenu = null;
	[SerializeField]
	private Toggle useFullscreenToggle = null;
	[SerializeField]
	private Dropdown gameQualityDropdown = null;
	[SerializeField]
	private Dropdown gameTypeDropdown = null;
	[SerializeField]
	private AudioSource audioSource = null;
	[SerializeField]
	private AudioClip[] audioClips = null;
	[SerializeField]
	private Slider SFXVolumeSlider = null;
	[SerializeField]
	private Slider musicVolumeSlider = null;
	[SerializeField]
	private Slider traitorPercentageSlider = null;
	[SerializeField]
	private Slider detectivePercentageSlider = null;
	[SerializeField]
	private Text SFXVolumeText = null;
	[SerializeField]
	private Text musicVolumeText = null;
	[SerializeField]
	private Text traitorPercentageText = null;
	[SerializeField]
	private Text detectivePercentageText = null;

	private int currentPlayersInRoom;
	private int skippedResolutions;
	private int currentSongPlaying = -1;
	private float sceneActivationWaitTimer = 3;
	private bool isLoadingScene;
	private AsyncOperation asyncOperation;
	private PlayerActions playerActions;

	private Dictionary<Button, Dictionary<InControl.PlayerAction, InControl.BindingSource>> bindingButtons = new Dictionary<Button, Dictionary<InControl.PlayerAction, InControl.BindingSource>>();
	private Dictionary<int, float> playerprogresses = new Dictionary<int, float>();
	private List<Text> inRoomPlayerNames = new List<Text>();
	private List<LoadingBar> multiplayerLoadingBars = new List<LoadingBar>();

	private void Start()
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
				Screen.SetResolution(Screen.resolutions[PlayerPrefs.GetInt("ScreenResolution") + skippedResolutions].width, Screen.resolutions[PlayerPrefs.GetInt("ScreenResolution") + skippedResolutions].height, Convert.ToBoolean(PlayerPrefs.GetInt("IsFullScreen")));
			}
		}
		gameQualityDropdown.template.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(0, 50 * QualitySettings.names.Length);
		gameQualityDropdown.ClearOptions();
		screenResolutionDropdown.template.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(0, 50 * Screen.resolutions.Length);
		screenResolutionDropdown.ClearOptions();

		for (int i = 0; i < QualitySettings.names.Length; i++)
		{
			gameQualityDropdown.options.Add(new Dropdown.OptionData() { text = QualitySettings.names[i] });

			if (QualitySettings.GetQualityLevel() != i) return;

			gameQualityDropdown.value = i;
			gameQualityDropdown.captionText.text = QualitySettings.names[i];
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

			if (Application.platform == RuntimePlatform.WindowsEditor) return;

			if (Screen.resolutions[i].height == Screen.resolutions[PlayerPrefs.GetInt("ScreenResolution") + skippedResolutions].height && Screen.resolutions[i].width == Screen.resolutions[PlayerPrefs.GetInt("ScreenResolution") + skippedResolutions].width)
			{
				screenResolutionDropdown.value = i;
				screenResolutionDropdown.captionText.text = Screen.resolutions[PlayerPrefs.GetInt("ScreenResolution") + skippedResolutions].width + " X " + Screen.resolutions[PlayerPrefs.GetInt("ScreenResolution") + skippedResolutions].height;
			}
		}

		useFullscreenToggle.isOn = !Screen.fullScreen;

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

		inRoomPlayerNames.Add(inRoomPlayerName);
		multiplayerLoadingBars.Add(multiplayerLoadingBar);
	}

	private void InputManager_OnDeviceAttached(InControl.InputDevice obj)
	{
		RedrawControllsMenu();
	}

	private void Update()
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
					if (multiplayerLoadingBars.Count < PhotonNetwork.room.PlayerCount)
					{
						LoadingBar newMultiplayerLoadingBar = Instantiate(multiplayerLoadingBar);
						newMultiplayerLoadingBar.gameObject.transform.SetParent(multiplayerLoadingBars[0].transform.parent);

						multiplayerLoadingBars.Add(newMultiplayerLoadingBar);
					}

					if (multiplayerLoadingBars[i].transform.localScale != Vector3.one)
					{
						multiplayerLoadingBars[i].transform.localScale = Vector3.one;
					}

					multiplayerLoadingBars[i].OwnerText = PhotonNetwork.playerList[i].NickName;
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
			if (currentPlayersInRoom != PhotonNetwork.room.PlayerCount)
			{
				playerprogresses.Clear();

				GameManager.GetInstance().CurrentGameType = (GameTypes)PhotonNetwork.room.CustomProperties["gameType"];

				string roomName = "Room name: " + PhotonNetwork.room.Name + Environment.NewLine + "Game mode: ";

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
				currentPlayersInRoomText.text = PhotonNetwork.room.PlayerCount + "/" + PhotonNetwork.room.MaxPlayers + " players";

				int counter = 0;
				for (int i = 0; i < PhotonNetwork.room.PlayerCount; i++)
				{
					if (inRoomPlayerNames.Count < PhotonNetwork.room.PlayerCount)
					{
						Text newPlayerNameHolder = Instantiate(inRoomPlayerName);
						newPlayerNameHolder.transform.localScale = Vector3.one;
						newPlayerNameHolder.gameObject.transform.SetParent(inRoomPlayerNames[0].transform.parent);

						inRoomPlayerNames.Add(newPlayerNameHolder);
					}

					if (PhotonNetwork.playerList[i].Equals(PhotonNetwork.masterClient))
					{
						inRoomPlayerNames[i].text = PhotonNetwork.playerList[i].NickName + " (Host)";
					}
					else
					{
						inRoomPlayerNames[i].text = PhotonNetwork.playerList[i].NickName;
					}
					counter++;
				}

				for (int i = counter; i < inRoomPlayerNames.Count; i++)
				{
					inRoomPlayerNames[i].text = "";
				}

				playerprogresses = new Dictionary<int, float>(PhotonNetwork.room.PlayerCount);

				foreach (PhotonPlayer player in PhotonNetwork.playerList)
				{
					playerprogresses.Add(player.ID, 0);
				}

				currentPlayersInRoom = PhotonNetwork.room.PlayerCount;
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

	private void RedrawControllsMenu()
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
			InControl.PlayerAction action = playerActions.Actions[i];

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
				var acti = new Dictionary<InControl.PlayerAction, InControl.BindingSource>()
				{
					{action, action.Bindings[j]}
				};

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
		PhotonNetwork.player.NickName = (SteamManager.Initialized == true)
			? SteamFriends.GetPersonaName()
			: Environment.UserName;

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
	}

	[PunRPC]
	private void SendLoadingData(int playerID, float progress)
	{
		if (playerprogresses.ContainsKey(playerID) == true)
		{
			//Debug.Log(playerID + " " + progress);
			playerprogresses[playerID] = progress;
		}
	}

	private void SetupMultiplayerLoadingBars()
	{
		for (int i = 0; i < PhotonNetwork.room.PlayerCount; i++)
		{
			multiplayerLoadingBars[i].gameObject.SetActive(true);
			multiplayerLoadingBars[i].OwnerText = PhotonNetwork.playerList[i].NickName;
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

		PhotonNetwork.player.NickName = (SteamManager.Initialized == true)
			? SteamFriends.GetPersonaName()
			: Environment.UserName;
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
			if (rooms[i].PlayerCount == rooms[i].MaxPlayers)
			{
				continue;
			}

			RoomSelector room = Instantiate(roomSelectorPrefab);
			room.RoomName = rooms[i].Name;

			if (rooms[i].CustomProperties.ContainsKey("gameType") == true)
			{
				switch ((GameTypes)rooms[i].CustomProperties["gameType"])
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

			room.CurrentPlayerCount = rooms[i].PlayerCount.ToString();
			room.MaxPlayerCount = rooms[i].MaxPlayers.ToString();
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
		if (PhotonNetwork.inRoom == false) return;

		ClearRoomList();
		currentPlayersInRoom = 0;
		roomNameText.text = "";
		GameManager.GetInstance().GetNetworkManager().LeaveRoom();
	}

	public void UpdateRoomCreationSettingsMenu()
	{
		switch (gameTypeDropdown.value)
		{
			case 0:
				if (gameSettingsMenu.gameObject.activeInHierarchy == true)
				{
					gameSettingsMenu.gameObject.SetActive(false);
				}

				if (maxPlayerSlider.maxValue != 4)
				{
					maxPlayerSlider.maxValue = 4;
				}
				break;
			case 1:
				if (gameSettingsMenu.gameObject.activeInHierarchy == true)
				{
					gameSettingsMenu.gameObject.SetActive(false);
				}

				if (maxPlayerSlider.maxValue != 8)
				{
					maxPlayerSlider.maxValue = 8;
				}
				break;
			case 2:
				if (gameSettingsMenu.gameObject.activeInHierarchy == false)
				{
					gameSettingsMenu.gameObject.SetActive(true);
				}

				if (maxPlayerSlider.maxValue != 12)
				{
					maxPlayerSlider.maxValue = 12;
				}

				if (traitorPercentageSlider.value == 0)
				{
					traitorPercentageText.text = "0";
				}
				else if (traitorPercentageSlider.value == 1)
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
				break;
		}
	}

	public void ResetRoomCreationSettingsMenu()
	{
		switch (gameTypeDropdown.value)
		{
			case 0:
				return;
			case 1:
				return;
			case 2:
				traitorPercentageSlider.value = 0.250F;
				detectivePercentageSlider.value = 0.125F;
				break;
		}
	}

	private void JoinRoom(string roomName)
	{
		for (int i = 0; i < PhotonNetwork.GetRoomList().Length; i++)
		{
			if (PhotonNetwork.GetRoomList()[i].Name != roomName) continue;
			if (PhotonNetwork.GetRoomList()[i].IsVisible == true) continue;

			RefreshRoomList();
			return;
		}

		GameManager.GetInstance().GetNetworkManager().JoinRoom(roomName);
		multiplayerStartGameButton.gameObject.SetActive(false);
	}

	private void SwitchToInRoom()
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

		Screen.fullScreen = !useFullscreenToggle.isOn;

		if (Screen.resolutions[screenResolutionDropdown.value + skippedResolutions].height != Screen.currentResolution.height || Screen.resolutions[screenResolutionDropdown.value + skippedResolutions].width != Screen.currentResolution.width)
		{
			Screen.SetResolution(Screen.resolutions[screenResolutionDropdown.value + skippedResolutions].width, Screen.resolutions[screenResolutionDropdown.value + skippedResolutions].height, !useFullscreenToggle.isOn);
			isChanged = true;
		}

		if (isChanged == false) return;

		PlayerPrefs.SetInt("QualityLevel", QualitySettings.GetQualityLevel());
		PlayerPrefs.SetInt("ScreenResolution", screenResolutionDropdown.value);
		PlayerPrefs.SetInt("IsFullScreen", useFullscreenToggle.isOn == true ? 0 : 1);
		PlayerPrefs.Save();
	}

	private void OnApplicationQuit()
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
			foreach (InControl.BindingSource value in i.Values)
			{
				if (value != actionAndBinding.Value)
				{
					continue;
				}

				options.IncludeModifiersAsFirstClassKeys = true;

				switch (value.BindingSourceType)
				{
					case InControl.BindingSourceType.DeviceBindingSource:
						options.IncludeControllers = true;
						options.IncludeKeys = false;
						options.IncludeMouseButtons = false;
					break;
					case InControl.BindingSourceType.KeyBindingSource:
						options.IncludeControllers = false;
						options.IncludeKeys = true;
						options.IncludeMouseButtons = true;
						break;
					default:
						options.IncludeControllers = false;
						options.IncludeKeys = true;
						options.IncludeMouseButtons = true;
					break;
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