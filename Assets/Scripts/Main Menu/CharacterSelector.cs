using UnityEngine;

public class CharacterSelector : OverridableMonoBehaviour {

	[SerializeField]
	private GameObject gameModel = null;
	[SerializeField]
	private Material gameModelMaterial = null;
	[SerializeField]
	private float rotationSpeed = 0;
	[SerializeField]
	private UnityEngine.UI.Dropdown characterSelector = null;

	private void Start()
	{
		if (PlayerPrefs.HasKey("SelectedCharacter") != true) return;

		gameModelMaterial.SetTexture("_EmissionMap", GameManager.GetInstance().AllPlayerSkins[PlayerPrefs.GetInt("SelectedCharacter")]);
		characterSelector.value = PlayerPrefs.GetInt("SelectedCharacter");
	}

	public void UpdateCharacterModel()
	{
		gameModelMaterial.SetTexture("_EmissionMap", GameManager.GetInstance().AllPlayerSkins[characterSelector.value]);
		PlayerPrefs.SetInt("SelectedCharacter", characterSelector.value);
		PlayerPrefs.Save();
		ExitGames.Client.Photon.Hashtable playerSettings = new ExitGames.Client.Photon.Hashtable
		{
			{"playerSkin", characterSelector.value}
		};

		PhotonNetwork.SetPlayerCustomProperties(playerSettings);
	}

	public override void UpdateMe()
	{
		gameModel.transform.Rotate(Vector3.up * Time.deltaTime * rotationSpeed);
		float size = (transform.position - gameModel.transform.position).magnitude;
		gameModel.transform.localScale = new Vector3(size, size, size);
	}
}
