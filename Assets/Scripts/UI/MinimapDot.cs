using UnityEngine;

public class MinimapDot : OverridableMonoBehaviour
{
	[SerializeField]
	private float destroyTime = 0;
	[SerializeField]
	private float fadeoutTime = 0;
	[SerializeField]
	private MeshRenderer image = null;

	private float currentFadeoutTime;

	private void Start()
	{
		currentFadeoutTime = destroyTime;
	}
	
	public override void UpdateMe()
	{
		if (currentFadeoutTime < fadeoutTime)
		{
			Color color = image.material.color;
			image.material.color = new Color(color.r, color.g, color.b, currentFadeoutTime / fadeoutTime); 
		}

		currentFadeoutTime -= Time.deltaTime;

		if(currentFadeoutTime <= 0)
		{
			Destroy(gameObject);
		}
	}
}
