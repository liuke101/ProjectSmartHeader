using UnityEngine;
using UnityEditor; // 引入编辑器命名空间

// 确保这个脚本在编辑器文件夹中
#if UNITY_EDITOR
[ExecuteInEditMode]
public class AddMeshColliderToSensors : MonoBehaviour
{
    // 可设置为想要批量添加 MeshCollider 的传感器类型
    public string sensorTag = "Sensor";  // 根据标签识别传感器
    public bool addColliderToChildren = true;  // 是否批量为子物体添加
    
    // 自定义编辑器类
    [CustomEditor(typeof(AddMeshColliderToSensors))]
    public class AddMeshColliderEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // 绘制默认的Inspector界面
            DrawDefaultInspector();
            
            // 获取脚本实例
            AddMeshColliderToSensors script = (AddMeshColliderToSensors)target;
            
            // 添加一个按钮
            if (GUILayout.Button("添加MeshCollider到所有传感器", GUILayout.Height(30)))
            {
                script.AddCollidersToSensors();
            }
        }
    }
    
    // 公开方法，用于添加碰撞器
    public void AddCollidersToSensors()
    {
        // 获取所有场景中的传感器对象，可以通过标签来过滤
        GameObject[] sensors = GameObject.FindGameObjectsWithTag(sensorTag);
        
        int addedCount = 0;
        
        // 遍历所有传感器对象
        foreach (GameObject sensor in sensors)
        {
            // 检查传感器是否已经有 MeshCollider，如果没有，则添加
            if (sensor.GetComponent<MeshCollider>() == null && sensor.GetComponent<MeshFilter>() != null)
            {
                Undo.RecordObject(sensor, "Add MeshCollider"); // 记录操作以支持撤销
                MeshCollider meshCollider = sensor.AddComponent<MeshCollider>();
                meshCollider.convex = true;  // 确保启用凸形态，方便物理计算
                meshCollider.isTrigger = true;  // 设置为触发器
                addedCount++;
                Debug.Log($"MeshCollider (isTrigger) added to: {sensor.name}");
            }

            // 可选：为传感器的所有子物体添加 MeshCollider
            if (addColliderToChildren)
            {
                foreach (Transform child in sensor.transform)
                {
                    if (child.GetComponent<MeshCollider>() == null && child.GetComponent<MeshFilter>() != null)
                    {
                        Undo.RecordObject(child.gameObject, "Add MeshCollider"); // 记录操作以支持撤销
                        MeshCollider meshCollider = child.gameObject.AddComponent<MeshCollider>();
                        meshCollider.convex = true;
                        meshCollider.isTrigger = true;  // 设置为触发器
                        addedCount++;
                        Debug.Log($"MeshCollider (isTrigger) added to child: {child.name}");
                    }
                }
            }
        }
        
        EditorUtility.DisplayDialog("操作完成", $"已添加 {addedCount} 个 MeshCollider 组件到传感器对象。", "确定");
    }
}
#endif
