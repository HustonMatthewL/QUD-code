using UnityEngine;
using UnityEngine.UI;
using XRL.UI;

public class FontSizeUIScalar : MonoBehaviour
{
	public UnityEngine.UI.Text text;

	public int target;

	private string lastScale;

	private void Start()
	{
	}

	private void Update()
	{
		if (lastScale != Options.GetOption("OptionPrereleaseStageScale", "1.0"))
		{
			lastScale = Options.GetOption("OptionPrereleaseStageScale", "1.0");
			if (text != null)
			{
				text.fontSize = (int)((double)target / Options.StageScale);
			}
		}
	}
}
