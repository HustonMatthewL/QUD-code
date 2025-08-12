using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Colorful/Channel Mixer")]
public class CC_ChannelMixer : CC_Base
{
	[Range(-200f, 200f)]
	public float redR = 100f;

	[Range(-200f, 200f)]
	public float redG;

	[Range(-200f, 200f)]
	public float redB;

	[Range(-200f, 200f)]
	public float greenR;

	[Range(-200f, 200f)]
	public float greenG = 100f;

	[Range(-200f, 200f)]
	public float greenB;

	[Range(-200f, 200f)]
	public float blueR;

	[Range(-200f, 200f)]
	public float blueG;

	[Range(-200f, 200f)]
	public float blueB = 100f;

	[Range(-200f, 200f)]
	public float constantR;

	[Range(-200f, 200f)]
	public float constantG;

	[Range(-200f, 200f)]
	public float constantB;

	public int currentChannel;

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		base.material.SetVector("_Red", new Vector4(redR * 0.01f, greenR * 0.01f, blueR * 0.01f));
		base.material.SetVector("_Green", new Vector4(redG * 0.01f, greenG * 0.01f, blueG * 0.01f));
		base.material.SetVector("_Blue", new Vector4(redB * 0.01f, greenB * 0.01f, blueB * 0.01f));
		base.material.SetVector("_Constant", new Vector4(constantR * 0.01f, constantG * 0.01f, constantB * 0.01f));
		Graphics.Blit(source, destination, base.material);
	}
}
