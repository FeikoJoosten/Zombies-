using UnityEngine;

public class GameManager : OverridableMonoBehaviour
{
	[SerializeField]
	private AIManager AIManagerPrefab = null;
	[SerializeField]
	private NetworkManager networkManagerPrefab = null;
	[SerializeField]
	private AudioManager audioManagerPrefab = null;
	[SerializeField]
	private Texture[] allPlayerSkins = null;
	[SerializeField]
	private int tTTWarmupTime = 30;

	private static GameManager instance;
	private AIManager aIManager;
	private NetworkManager networkManager;
	private AudioManager audioManager;
	private bool inGame;
	private bool tTTWarmingUp = true;
	private bool tTTCoolingDown;
	private GameTypes currentGameType;
	private float terroristSpawnRate;
	private float detectiveSpawnRate;

	public bool InGame
	{
		get { return inGame; }
		set { inGame = value; }
	}
	public bool TTTWarmingUp
	{
		get { return tTTWarmingUp; }
		set { tTTWarmingUp = value; }
	}
	public bool TTTCoolingDown
	{
		get { return tTTCoolingDown; }
	}
	public Texture[] AllPlayerSkins
	{
		get { return allPlayerSkins; }
	}
	public GameTypes CurrentGameType
	{
		get { return currentGameType; }
		set { currentGameType = value; }
	}
	public float TerroristSpawnRate
	{
		get { return terroristSpawnRate; }
		set { terroristSpawnRate = value; }
	}
	public float DetectiveSpawnRate
	{
		get { return detectiveSpawnRate; }
		set { detectiveSpawnRate = value; }
	}
	public int TTTWarmupTime
	{
		get { return tTTWarmupTime; }
	}

	public static GameManager GetInstance()
	{
		return instance;
	}

	protected override void Awake()
	{
		base.Awake();

		GetAudioManager().UpdateAudioVolumes(PlayerPrefs.GetFloat("SFXVolume"), PlayerPrefs.GetFloat("MusicVolume"));
	}

	private void Start()
	{
		GameManager[] gameManagers = FindObjectsOfType<GameManager>();

		if (gameManagers.Length > 1)
		{
			Destroy(gameObject);
		}

		DontDestroyOnLoad(gameObject);

		if (instance == null)
		{
			instance = this;
		}
	}

	public override void UpdateMe()
	{
		if (inGame == false || (PhotonNetwork.offlineMode == false && PhotonNetwork.isMasterClient == false))
		{
			return;
		}

		if (currentGameType == GameTypes.ZombieMode)
		{
			if (GetAIManager().AllRemainingZombies.Count == 0)
			{
				aIManager.NextWave();
			}
		}
		else if (currentGameType == GameTypes.TTT)
		{
			if (tTTCoolingDown == true)
			{
				return;
			}

			if (GetNetworkManager().AllRemainingTraitorPlayers.Count > 0 && GetNetworkManager().AllRemainingInnocentPlayers.Count == 0) //Traitor victory
			{
				tTTCoolingDown = true;
				GetNetworkManager().LaunchTTTVictoryScreen();
			}
			else if (GetNetworkManager().AllRemainingTraitorPlayers.Count == 0 && GetNetworkManager().AllRemainingInnocentPlayers.Count > 0) //Innocent victory
			{
				tTTCoolingDown = true;
				GetNetworkManager().LaunchTTTVictoryScreen();
			}
		}
	}

	public AIManager GetAIManager()
	{
		if (aIManager == null)
		{
			if (PhotonNetwork.offlineMode == false)
			{
				if (PhotonNetwork.isMasterClient == true)
				{
					aIManager = PhotonNetwork.InstantiateSceneObject(AIManagerPrefab.name, Vector3.zero, Quaternion.Euler(Vector3.zero), 0, null).GetComponent<AIManager>();
				}
				else
				{
					if (FindObjectOfType<AIManager>() != null)
					{
						aIManager = FindObjectOfType<AIManager>();
					}
				}
			}
			else
			{
				aIManager = Instantiate(AIManagerPrefab);
			}
		}

		return aIManager;
	}

	public NetworkManager GetNetworkManager()
	{
		if (networkManager != null) return networkManager;

		networkManager = Instantiate(networkManagerPrefab);
		DontDestroyOnLoad(networkManager);

		return networkManager;
	}

	public AudioManager GetAudioManager()
	{
		if (audioManager != null) return audioManager;

		if (PhotonNetwork.inRoom == false)
		{
			audioManager = Instantiate(audioManagerPrefab);
		}
		else
		{
			audioManager = PhotonNetwork.isMasterClient == false ? FindObjectOfType<AudioManager>() : PhotonNetwork.InstantiateSceneObject(audioManagerPrefab.name, Vector3.zero, Quaternion.Euler(Vector3.zero), 0, null).GetComponent<AudioManager>();
		}

		return audioManager;
	}

	private void OnApplicationQuit()
	{
		PlayerPrefs.SetFloat("SFXVolume", GetAudioManager().SavedSFXVolume);
		PlayerPrefs.SetFloat("MusicVolume", GetAudioManager().SavedMusicVolume);
		PlayerPrefs.Save();
	}
}
