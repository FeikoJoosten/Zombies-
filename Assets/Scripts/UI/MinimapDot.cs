using UnityEngine;
using System.Collections;

public class MinimapDot : OverridableMonoBehaviour
{
	[SerializeField]
	private float destroyTime;
	[SerializeField]
	private float fadeoutTime;
	[SerializeField]
	private MeshRenderer image;

	private float currentFadeoutTime = 0;

	void Start()
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
