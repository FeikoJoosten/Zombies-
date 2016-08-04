using UnityEngine;
using System.Collections;

public class CharacterSelector : OverridableMonoBehaviour {

	[SerializeField]
	private GameObject gameModel;
	[SerializeField]
	private Material gameModelMaterial;
	[SerializeField]
	private float rotationSpeed;
	[SerializeField]
	private UnityEngine.UI.Dropdown characterSelector;

	void Start()
	{
		if(PlayerPrefs.HasKey("SelectedCharacter") == true)
		{
			gameModelMaterial.SetTexture("_EmissionMap", GameManager.GetInstance().AllPlayerSkins[PlayerPrefs.GetInt("SelectedCharacter")]);
			characterSelector.value = PlayerPrefs.GetInt("SelectedCharacter");
		}
	}

	public void UpdateCharacterModel()
	{
		gameModelMaterial.SetTexture("_EmissionMap", GameManager.GetInstance().AllPlayerSkins[characterSelector.value]);
		PlayerPrefs.SetInt("SelectedCharacter", characterSelector.value);
		PlayerPrefs.Save();
		ExitGames.Client.Photon.Hashtable playerSettings = new ExitGames.Client.Photon.Hashtable();
		playerSettings.Add("playerSkin", characterSelector.value);
		PhotonNetwork.SetPlayerCustomProperties(playerSettings);
	}

	public override void UpdateMe()
	{
		gameModel.transform.Rotate(Vector3.up * Time.deltaTime * rotationSpeed);
		float size = (transform.position - gameModel.transform.position).magnitude;
		gameModel.transform.localScale = new Vector3(size, size, size);
	}
}
