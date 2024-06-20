using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CameraManager :  MonoSingleton<CameraManager>
{
    public GameObject MainCamera;
    public List<GameObject> SceneCameras;
    
    public UnityEvent<GameObject> SwitchCameraEvent;

    private void Start()
    {
        if (MainCamera != null)
        {
            SwitchCameraEvent?.Invoke(MainCamera);
        }
    }

    public void SwitchCamera(GameObject cam)
    {
        if (SceneCameras.Count > 0)
        {
            foreach (var Tmp in SceneCameras)
            {
                if (Tmp.activeSelf)
                {
                    Tmp.SetActive(false);
                }
            }
        }

        if (cam != null)
        {
            cam.SetActive(true);
            SwitchCameraEvent?.Invoke(cam);
        }
    }
}