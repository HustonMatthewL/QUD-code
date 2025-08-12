using UnityEngine;
using UnityEngine.UI;

public class PlatformTextSwitcher : MonoBehaviour
{
	public Text m_Label;

	public string m_WindowsText = "";

	public string m_iOSText = "";

	public string m_AndroidText = "";

	private void OnEnable()
	{
		if (!(m_Label == null))
		{
			m_Label.text = m_WindowsText;
		}
	}
}
