// Game Quality Settings
// AliyerEdon in Winter 2022
//				
//

// This script used for game settings menu
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Settings_Menu : MonoBehaviour
{

	#region Variables

	[Space(7)]
	[Header("FPS Display")]
	public Color fpsColor = Color.yellow;
	public TextAnchor alignment = TextAnchor.UpperRight;
	public FontStyle fontStyle = FontStyle.Bold;

	[Space(7)]
	[Header("Effects Toggles")]
	public Toggle displayFPS;
	public Toggle bloomEffect;
	public Toggle postEffects;
	public Toggle fogEffect;
	public Toggle dynamicResolution;
	public Toggle vSync;
	public Toggle HDR;

	[Space(7)]
	[Header("Dropdown")]
	public Dropdown qualityLevel;
	public Dropdown resolutionQuality;
	public Dropdown antiAliasing;

	[Space(7)]
	[Header("Effects Volume")]
	public UnityEngine.Rendering.Volume globalVolume;
	#endregion

	void Start ()
	{
        #region Load Settings
        // Read starting setting values
        // 		
        if (PlayerPrefs.GetInt ("Display FPS") == 1) // 1 = true , 0 = false
			displayFPS.isOn = true;
		else
			displayFPS.isOn = false;
		//_________________________________________________
		if (PlayerPrefs.GetInt("Bloom Effect") == 1) // 1 = true , 0 = false
			bloomEffect.isOn = true;
		else
			bloomEffect.isOn = false;
		//_________________________________________________
		if (PlayerPrefs.GetInt("Post Effects") == 1) // 1 = true , 0 = false
			postEffects.isOn = true;
		else
			postEffects.isOn = false;
		//_________________________________________________
		if (PlayerPrefs.GetInt("Fog Effect") == 1) // 1 = true , 0 = false
			fogEffect.isOn = true;
		else
			fogEffect.isOn = false;
		//_________________________________________________
		if (PlayerPrefs.GetInt("vSync") == 1) // 1 = true , 0 = false
			vSync.isOn = true;
		else
			vSync.isOn = false;
		//_________________________________________________
		if (PlayerPrefs.GetInt("HDR") == 1) // 1 = true , 0 = false
			HDR.isOn = true;
		else
			HDR.isOn = false;
		//_________________________________________________
		if (PlayerPrefs.GetInt("Dynamic Resolution") == 1) // 1 = true , 0 = false
			dynamicResolution.isOn = true;
		else
			dynamicResolution.isOn = false; 
		//_________________________________________________

		qualityLevel.value = PlayerPrefs.GetInt("Quality Level");
		
		resolutionQuality.value = PlayerPrefs.GetInt("Resolution Quality");

		antiAliasing.value = PlayerPrefs.GetInt("Anti Aliasing");
		//_________________________________________________

		#endregion
	}

    #region Quality Level
    // Public functions to use on UI.Button's OnClick() event
    public void Set_Quality_Level ()
	{
		PlayerPrefs.SetInt ("Quality Level", qualityLevel.value);

		QualitySettings.SetQualityLevel (PlayerPrefs.GetInt("Quality Level"), false);
	}
	//_________________________________________________
	#endregion

	#region Resolution Quality
	public void Set_Resolution_Quality ()
	{
		if (resolutionQuality.value == 3)
		{
			Screen.SetResolution((int)(PlayerPrefs.GetInt("OriginalX") * 0.3f),
				(int)(PlayerPrefs.GetInt("OriginalY") * 0.4f), true);
		}
		if (resolutionQuality.value == 2)
		{
			Screen.SetResolution((int)(PlayerPrefs.GetInt("OriginalX") * 0.5f),
				(int)(PlayerPrefs.GetInt("OriginalY") * 0.5f), true);
		}
		if (resolutionQuality.value == 1)
		{
			Screen.SetResolution((int)(PlayerPrefs.GetInt("OriginalX") * 0.7f),
				(int)(PlayerPrefs.GetInt("OriginalY") * 0.7f), true);
		}
		if (resolutionQuality.value == 0)
		{
			Screen.SetResolution((int)(PlayerPrefs.GetInt("OriginalX") * 1),
				(int)(PlayerPrefs.GetInt("OriginalY") * 1), true);
		}

		PlayerPrefs.SetInt("Resolution Quality", resolutionQuality.value);

	}
	//______________________________________________
	#endregion

	#region Anti-Aliasing
	public void Set_Anti_Aliasing()
	{
		PlayerPrefs.SetInt("Anti Aliasing", antiAliasing.value);
		UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset urpAsset0
				= (UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset)QualitySettings.GetRenderPipelineAssetAt(0);
		UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset urpAsset1
			= (UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset)QualitySettings.GetRenderPipelineAssetAt(1);
		UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset urpAsset2
						= (UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset)QualitySettings.GetRenderPipelineAssetAt(2);

		if (PlayerPrefs.GetInt("Anti Aliasing") == 0)
		{
			urpAsset0.msaaSampleCount = 0;
			urpAsset1.msaaSampleCount = 0;
			urpAsset2.msaaSampleCount = 0;
		}
		if (PlayerPrefs.GetInt("Anti Aliasing") == 1)
		{
			urpAsset0.msaaSampleCount = 2;
			urpAsset1.msaaSampleCount = 2;
			urpAsset2.msaaSampleCount = 2;
		}
		if (PlayerPrefs.GetInt("Anti Aliasing") == 2)
		{
			urpAsset0.msaaSampleCount = 4;
			urpAsset1.msaaSampleCount = 4;
			urpAsset2.msaaSampleCount = 4;
		}
		if (PlayerPrefs.GetInt("Anti Aliasing") == 3)
		{
			urpAsset0.msaaSampleCount = 8;
			urpAsset1.msaaSampleCount = 8;
			urpAsset2.msaaSampleCount = 8;
		}
	}
	//______________________________________________
	#endregion

	#region Display FPS
	public void Set_Display_FPS ()
	{
		StartCoroutine (Display_FPS());
	}

	IEnumerator Display_FPS()
	{
		yield return new WaitForEndOfFrame();
		if (displayFPS.isOn)
			PlayerPrefs.SetInt ("Display FPS", 1);  // 1 = true;
		else
			PlayerPrefs.SetInt ("Display FPS", 0);// 0 = false;

		if (GameObject.FindObjectOfType<Load_Settings>())
			GameObject.FindObjectOfType<Load_Settings>().Update_DisplayFPS();

	}
	//______________________________________________
	#endregion

	#region Bloom
	public void Set_Bloom_Effect()
	{
		StartCoroutine(Bloom_Effect());
	}

	IEnumerator Bloom_Effect()
	{
		yield return new WaitForEndOfFrame();
		if (bloomEffect.isOn)
			PlayerPrefs.SetInt("Bloom Effect", 1);  // 1 = true;
		else
			PlayerPrefs.SetInt("Bloom Effect", 0);// 0 = false;

		
		UnityEngine.Rendering.Universal.Bloom vBloom;

		GameObject.FindObjectOfType<UnityEngine.Rendering.Volume>().sharedProfile.TryGet(out vBloom);
		if (vBloom != null)
		{
			if (PlayerPrefs.GetInt("Bloom Effect") == 1)
				vBloom.active = true;
			else
				vBloom.active = false;
		}

	}
	//______________________________________________
	#endregion

	#region Post Effects
	public void Set_Post_Effects ()
	{
		StartCoroutine (Post_Effects_Save ());
	}

	IEnumerator Post_Effects_Save()
	{
		Camera[] cams = GameObject.FindObjectsOfType<Camera>();

		yield return new WaitForEndOfFrame();
		if (postEffects.isOn)
			PlayerPrefs.SetInt ("Post Effects", 1);  // 1 = true;
		else
			PlayerPrefs.SetInt ("Post Effects", 0);// 0 = false;

		if (PlayerPrefs.GetInt("Post Effects") == 1)
		{
			for (int a = 0; a < cams.Length; a++)
			{
				if (cams[a].GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>())
					cams[a].GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>().renderPostProcessing = true;
			}
		}
		else
		{
			for (int a = 0; a < cams.Length; a++)
			{
				if (cams[a].GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>())
					cams[a].GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>().renderPostProcessing = false;
			}
		}
	}
	//______________________________________________
	#endregion
		
	#region Dynamic Resolution
	public void Set_Dynamic_Resolution()
	{
		StartCoroutine(Dynamic_Resolution());
	}

	IEnumerator Dynamic_Resolution()
	{
		yield return new WaitForEndOfFrame();
		if (dynamicResolution.isOn)
			PlayerPrefs.SetInt("Dynamic Resolution", 1);  // 1 = true;
		else
			PlayerPrefs.SetInt("Dynamic Resolution", 0);// 0 = false;

		if (PlayerPrefs.GetInt("Dynamic Resolution") == 1)
		{
			Camera[] cams = GameObject.FindObjectsOfType<Camera>();
			for (int a = 0; a < cams.Length; a++)
				Camera.main.allowDynamicResolution = true;
		}
		else
		{
			Camera[] cams = GameObject.FindObjectsOfType<Camera>();
			for (int a = 0; a < cams.Length; a++)
				Camera.main.allowDynamicResolution = false;
		}
	}
	//______________________________________________
	#endregion

	#region Fog
	public void Set_Fog_Effect()
	{
		StartCoroutine(Fog_Effect());
	}

	IEnumerator Fog_Effect()
	{
		yield return new WaitForEndOfFrame();
		if (fogEffect.isOn)
			PlayerPrefs.SetInt("Fog Effect", 1);  // 1 = true;
		else
			PlayerPrefs.SetInt("Fog Effect", 0);// 0 = false;

		if (PlayerPrefs.GetInt("Fog Effect") == 1)
			RenderSettings.fog = true;
		else
			RenderSettings.fog = false;
	}
	//______________________________________________
	#endregion

	#region V-Sync
	public void Set_vSync()
	{
		StartCoroutine(v_Sync());
	}

	IEnumerator v_Sync()
	{
		yield return new WaitForEndOfFrame();
		if (vSync.isOn)
			PlayerPrefs.SetInt("vSync", 1);  // 1 = true;
		else
			PlayerPrefs.SetInt("vSync", 0);// 0 = false;

		if (PlayerPrefs.GetInt("vSync") == 0)
			QualitySettings.vSyncCount = 0;
		else
			QualitySettings.vSyncCount = 1;
	}
	//______________________________________________
	#endregion

	#region HDR
	public void Set_HDR()
	{
		StartCoroutine(Toggle_HDR());
	}

	IEnumerator Toggle_HDR()
	{
		yield return new WaitForEndOfFrame();
		if (HDR.isOn)
			PlayerPrefs.SetInt("HDR", 1);  // 1 = true;
		else
			PlayerPrefs.SetInt("HDR", 0);// 0 = false;

		UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset urpAsset0
				= (UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset)QualitySettings.GetRenderPipelineAssetAt(0);
		UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset urpAsset1
			= (UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset)QualitySettings.GetRenderPipelineAssetAt(1);
		UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset urpAsset2
						= (UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset)QualitySettings.GetRenderPipelineAssetAt(2);

		if (PlayerPrefs.GetInt("HDR") == 1)
		{
			urpAsset0.supportsHDR = true;
			urpAsset1.supportsHDR = true;
			urpAsset2.supportsHDR = true;
		}
		if (PlayerPrefs.GetInt("HDR") == 0)
		{
			urpAsset0.supportsHDR = false;
			urpAsset1.supportsHDR = false;
			urpAsset2.supportsHDR = false;
		}
	}
	//______________________________________________
	#endregion

	#region Close Window
	public void Disable_Object(GameObject target)
    {
		target.SetActive(false);

	}
    #endregion
}
