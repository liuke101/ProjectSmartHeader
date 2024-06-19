using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

namespace Common
{
    public class CameraManager : MonoSingleton<CameraManager>
    {
        public Camera MainCamera;
        public List<GameObject> OtherCameras;
        
        private void Start()
        {
            MainCamera = Camera.main;
        }

        //切换到主相机
        public void SwitchMainCamera()
        {
            MainCamera.enabled = true;

            foreach (var cam in OtherCameras)
            {
                if (cam.activeSelf)
                {
                    cam.SetActive(false);
                }
            }
        }
        
        //切换到指定震动相机
        public void SwitchCamera(GameObject cam)
        {
            foreach (var othercam in OtherCameras)
            {
                if (othercam.activeSelf)
                {
                    othercam.SetActive(false);
                }
            }
            MainCamera.enabled = false;
            cam.SetActive(true);
        }

        
    }
}