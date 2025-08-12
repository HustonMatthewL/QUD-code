using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Colorful/Threshold")]
public class CC_Threshold : CC_Base
{
	[Range(1f, 255f)]
	public float threshold = 128f;

	[Range(0f, 128f)]
	public float noiseRange = 48f;

	public bool useNoise;

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		base.material.SetFloat("_Threshold", threshold / 255f);
		base.material.SetFloat("_Range", noiseRange / 255f);
		Graphics.Blit(source, destination, base.material, useNoise ? 1 : 0);
	}
}
