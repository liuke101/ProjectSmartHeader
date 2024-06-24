using UnityEngine;
using System.Collections;

public class Display_FPS : MonoBehaviour
{
	float deltaTime = 0.0f;
	public Color color = Color.yellow;
	public TextAnchor alignment = TextAnchor.UpperLeft;
	public FontStyle fontStyle = FontStyle.Normal;

	void Update()
	{
		deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
	}

	void OnGUI()
	{
		int h = Screen.height;
		int w = Screen.width;

		GUIStyle style = new GUIStyle();

		Rect rect = new Rect(0, 0, w, h * 2 / 30);
		style.alignment = alignment;
		style.fontSize = h * 2 / 30;
		style.normal.textColor = color;
		style.fontStyle = fontStyle;

		float msec = deltaTime * 1000.0f;
		float fps = 1.0f / deltaTime;
		string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
		GUI.Label(rect, text, style);
	}
}

