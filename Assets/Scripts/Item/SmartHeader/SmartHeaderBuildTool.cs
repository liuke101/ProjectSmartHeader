using System;
using System.Collections.Generic;
using Item.SmartHeader.Architecture;
using Item.SmartHeader.Sensor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Item.SmartHeader
{
    public class SmartHeaderBuildTool : MonoBehaviour
    {
        public SmartHeader smartHeader;
        
        public HeaderModelData HeaderModelData; //材质数据
        
        public List<GameObject> Doors = new List<GameObject>(); //门
        public List<GameObject> Walls = new List<GameObject>();  //墙
        public List<GameObject> Floors = new List<GameObject>(); //楼板
        public List<GameObject> SemiconductorStrainGauges = new List<GameObject>(); //半导体应变片
        
        public List<GameObject> FddiTemperatureSensors = new List<GameObject>(); //光纤光栅温度传感器
        public List<GameObject> FddiAccelerationSensors = new List<GameObject>(); //光纤加速度传感器
        public List<GameObject> FddiPressureSensors = new List<GameObject>(); //光纤压力传感器
        public List<GameObject> FddiStrainSensors = new List<GameObject>();  //光纤应变传感器
        public List<GameObject> PiezoAccelerationSensor = new List<GameObject>(); //压电加速度传感器
        public List<GameObject> ElecVibrationVelocitySensors = new List<GameObject>(); //电类振动速度传感器
        public List<GameObject> Air_OverpressureSensors = new List<GameObject>(); //空气超压传感器

        private void Awake()
        {
            if(smartHeader == null) smartHeader = GetComponent<SmartHeader>();
        }

        [ContextMenu("初始化模型")]
        public void Init()
        {
            //清空所有数据
            Doors.Clear();
            Walls.Clear();
            Floors.Clear();
            SemiconductorStrainGauges.Clear();
            FddiTemperatureSensors.Clear();
            FddiAccelerationSensors.Clear();
            FddiPressureSensors.Clear();
            FddiStrainSensors.Clear();
            PiezoAccelerationSensor.Clear();
            ElecVibrationVelocitySensors.Clear();
            Air_OverpressureSensors.Clear();
            
            //从Resources文件夹中加载材质数据
            if (HeaderModelData == null)
            {
                HeaderModelData = Resources.Load<HeaderModelData>("ScriptableObjects/HeaderMaterialData");
            }
            
            if (smartHeader == null || HeaderModelData == null) return;
            
            //重新加载数据
            InitSubObjects<Door>(Doors, EModelType.门, true);
            InitSubObjects<Wall>(Walls, EModelType.墙, true);
            InitSubObjects<Floor>(Floors, EModelType.楼板, true);
            InitSubObjects<SemiconductorStrainGauge>(SemiconductorStrainGauges, EModelType.半导体应变片, true);
            InitSubObjects<FDDI_TemperatureSensor>(FddiTemperatureSensors, EModelType.光纤光栅温度传感器, true);
            InitSubObjects<FDDI_AccelerationSensor>(FddiAccelerationSensors, EModelType.光纤加速度传感器, true);
            InitSubObjects<FDDI_PressureSensor>(FddiPressureSensors, EModelType.光纤压力传感器, true);
            InitSubObjects<FDDI_StrainSensor>(FddiStrainSensors, EModelType.光纤应变传感器, true);
            InitSubObjects<Piezo_AccelerationSensor>(PiezoAccelerationSensor, EModelType.压电加速度传感器, true);
            InitSubObjects<Elec_VibrationVelocitySensor>(ElecVibrationVelocitySensors, EModelType.电类振动速度传感器, true);
            InitSubObjects<Air_OverpressureSensor>(Air_OverpressureSensors, EModelType.空气超压传感器, true);
        }
        
        //初始化数组对象
        private void InitSubObjects<T>(List<GameObject> subObjects, EModelType modelType, bool bAddMaterial = false) where T : Component
        {
            //收集所有子对象
            // 获取SmartHeader所有包含Name子字符串的子对象
            foreach (Transform child in smartHeader.transform)
            {
                if (child.name.Contains(HeaderModelData.GetNameSubstringByModelType(modelType)))
                {
                    subObjects.Add(child.gameObject);
                    
                    //添加组件
                    child.gameObject.AddComponent<T>();
                
                    //添加材质
                    if (bAddMaterial)
                    {
                        MeshRenderer meshRenderer = child.gameObject.GetComponent<MeshRenderer>();
                        if (meshRenderer)
                        {
                            meshRenderer.material = HeaderModelData.GetMaterialByModelType(modelType);
                        }
                    }
                }
            }
            
        }
        
    }
}