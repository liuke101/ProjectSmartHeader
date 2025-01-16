using System;
using System.Collections;
using JsonStruct;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;
using Random = UnityEngine.Random;

public class DamageDecalManager : MonoBehaviour
{
    [Header("Decal Settings")]
    public DecalProjector decalProjector;  // 单个 DecalProjector 组件
    // public Vector3[] damagePositions;      // 存储五个不同的损伤位置
    
    [SerializeField] // 让数组仍然可以在 Inspector 中显示，但不暴露
    private Vector3[] damagePositions;      // 存储五个不同的损伤位置
    
    [Header("Damage Materials")]
    public Material[] damageMaterials;     // 存储不同损伤等级的材质

    [Header("Crack Decal Data")]
    public CrackDecalData crackDecalData;  // 引用 CrackDecalData

    // private Vector3[] minRanges;
    // private Vector3[] maxRanges;
    // private void Start()
    // {
        // // 设定每个贴花位置的最小和最大范围
        // // minRanges = new Vector3[5]
        // Vector3[] minRanges = new Vector3[5]
        // {
        //     new Vector3(-3.82f, 144.3f, -10.89f),   // 第一个位置的最小范围
        //     new Vector3(4.58f, 144.78f, -7.77f),   // 第二个位置的最小范围
        //     new Vector3(6.69f, 144.78f, -7f),    // 第三个位置的最小范围
        //     new Vector3(9f, 144.78f, -7f),    // 第四个位置的最小范围
        //     new Vector3(11.64f, 145.21f, -7f)     // 第五个位置的最小范围
        // };
        //
        // // maxRanges = new Vector3[5]
        // Vector3[] maxRanges = new Vector3[5]
        // {
        //     new Vector3(6f, 145.85f, -10.89f),     // 第一个位置的最大范围
        //     new Vector3(4.58f, 145.81f, -7.11f),     // 第二个位置的最大范围
        //     new Vector3(7.82f, 145.89f, -7f),     // 第三个位置的最大范围
        //     new Vector3(10.42f, 145.89f, -7f),     // 第四个位置的最大范围
        //     new Vector3(13.85f, 146.23f, -7f)     // 第五个位置的最大范围
        // };
        //
        // // 生成五个随机的损伤位置
        // damagePositions = new Vector3[5];
        // for (int i = 0; i < damagePositions.Length; i++)
        // {
        //     // 生成一个随机位置，范围在 minRange 和 maxRange 之间
        //     float randomX = Random.Range(minRanges[i].x, maxRanges[i].x);
        //     float randomY = Random.Range(minRanges[i].y, maxRanges[i].y);
        //     float randomZ = Random.Range(minRanges[i].z, maxRanges[i].z);
        //
        //     // 将生成的随机位置赋值给 damagePositions 数组
        //     damagePositions[i] = new Vector3(randomX, randomY, randomZ);
        //     Debug.Log(damagePositions[i]);
        // }
    // }

    private void Start()
{
    InitializeDamagePositions();
}

private void InitializeDamagePositions()
{
    Vector3[] minRanges = new Vector3[5]
    {
        new Vector3(4.03f, 144.679f, -10.649f),
        new Vector3(4.58f, 144.78f, -7.77f),
        new Vector3(6.69f, 144.78f, -7f),
        new Vector3(9f, 144.78f, -7f),
        new Vector3(11.64f, 145.21f, -7f)
    };

    Vector3[] maxRanges = new Vector3[5]
    {
        new Vector3(6f, 145.85f, -10.89f),
        new Vector3(4.58f, 145.81f, -7.11f),
        new Vector3(7.82f, 145.89f, -7f),
        new Vector3(10.42f, 145.89f, -7f),
        new Vector3(13.85f, 146.23f, -7f)
    };

    damagePositions = new Vector3[5];
    for (int i = 0; i < damagePositions.Length; i++)
    {
        float randomX = Random.Range(minRanges[i].x, maxRanges[i].x);
        float randomY = Random.Range(minRanges[i].y, maxRanges[i].y);
        float randomZ = Random.Range(minRanges[i].z, maxRanges[i].z);
        damagePositions[i] = new Vector3(randomX, randomY, randomZ);
        // Debug.Log("awake"+damagePositions[i]);
    }
}

    // 当接收到损伤数据时，触发此方法
    public void OnDamageMessageReceived(DamageData damageData)
    {
        // // 生成五个随机的损伤位置
        // Vector3[] damagePositions = new Vector3[5];
        // // for (int i = 0; i < minRanges.Length; i++)
        // for (int i = 0; i < damagePositions.Length; i++)
        // {
        //     // 生成一个随机位置，范围在 minRange 和 maxRange 之间
        //     float randomX = Random.Range(minRanges[i].x, maxRanges[i].x);
        //     float randomY = Random.Range(minRanges[i].y, maxRanges[i].y);
        //     float randomZ = Random.Range(minRanges[i].z, maxRanges[i].z);
        //
        //     // 将生成的随机位置赋值给 damagePositions 数组
        //     damagePositions[i] = new Vector3(randomX, randomY, randomZ);
        // }
        // Debug.Log($"Received DamageData: {damageData.damage_percent_1}, {damageData.damage_percent_2}, {damageData.damage_percent_3},{damageData.damage_percent_4},{damageData.damage_percent_5}");
        InitializeDamagePositions();
        if (crackDecalData == null)
        {
            Debug.LogError("CrackDecalData is not assigned!");
            return;
        }
        // Debug.Log("已分配CrackDecalData,继续应用贴花.");
        
        // 假设 damage_percent_1, damage_percent_2, 等等，代表不同部位的损伤百分比
        // for (int i = 0; i < minRanges.Length; i++)
        for (int i = 0; i < damagePositions.Length; i++)
        {
            // 根据损伤百分比选择合适的材质
            Material selectedMaterial = GetMaterialForDamageLevel(damageData.GetDamagePercent(i));

            // 在对应位置应用 Decal
            ApplyDamageDecal(damagePositions[i], selectedMaterial);
            // Debug.Log("函数"+damagePositions[i]);
            if (decalProjector != null && decalProjector.gameObject.activeSelf)
            {
                decalProjector.transform.position = damagePositions[i];  // 设置位置
                decalProjector.material = selectedMaterial;  // 设置材质
                decalProjector.enabled = true;  // 启用 DecalProjector
            }
            else
            {
                Debug.LogError("DecalProjector is not active or not assigned!");
            }

            // 获取裂纹贴花信息并应用
            int damageLevel = GetDamageLevel(damageData.GetDamagePercent(i)); // 计算损伤等级
            var crackDecalInfo = crackDecalData.GetCrackDecalInfo(damageLevel);
            ApplyCrackDecal(damagePositions[i], crackDecalInfo.CrackDecalPrefab, crackDecalInfo.DecalSize,i);
        }
    }

    // 根据损伤百分比选择材质
    private Material GetMaterialForDamageLevel(float damagePercent)
    {
        if (damagePercent >= 70)
        {
            return damageMaterials[2];  // 高损伤等级的材质
        }
        else if (damagePercent >= 30)
        {
            return damageMaterials[1];  // 中等损伤等级的材质
        }
        else
        {
            return damageMaterials[0];  // 低损伤等级的材质
        }
    }

    // 根据损伤百分比计算损伤等级
    private int GetDamageLevel(float damagePercent)
    {
        if (damagePercent >= 70)
            return 1;  // 高损伤等级
        else if (damagePercent >= 30)
            return 2;  // 中等损伤等级
        else
            return 3;  // 低损伤等级
    }

    // 应用损伤 Decal
    private void ApplyDamageDecal(Vector3 damagePosition, Material decalMaterial)
    {
        // decalProjector.transform.position = damagePosition;  // 设置位置
        // decalProjector.material = decalMaterial;  // 设置材质
        // decalProjector.enabled = true;  // 启用 DecalProjector
        //
        // // 启动一个协程，在指定时间后关闭 DecalProjector
        // StartCoroutine(RemoveDecalAfterDelay(10f));
        if (decalProjector != null && decalProjector.gameObject.activeSelf)
        {
            decalProjector.transform.position = damagePosition;  // 设置位置
            decalProjector.material = decalMaterial;  // 设置材质
            decalProjector.enabled = true;  // 启用 DecalProjector
        }
        else
        {
            Debug.LogError("DecalProjector is not active or not assigned!");
        }
    }

    // 应用裂纹贴花
    private void ApplyCrackDecal(Vector3 position, GameObject crackDecalPrefab, float decalSize, int i)
    {
        if (crackDecalPrefab == null)
        {
            Debug.LogError("CrackDecalPrefab is not assigned!");
            return;
        }
        // 确保 decalSize 不为零，避免缩放无效
        if (decalSize <= 0f)
            decalSize = 1f;
        // 创建一个旋转变量
        Quaternion decalRotation = Quaternion.identity;  // 默认旋转为无旋转
        if (i == 0)
        {
            decalRotation = Quaternion.Euler(0, -36.9f, 0); // 设置 Y 轴旋转-36.9度
        }
        // 判断是否是第二个 CrackDecal，如果是，则应用旋转
        if (i == 1)
        {
            decalRotation = Quaternion.Euler(0, 90, 0); // 设置 Y 轴旋转90度
        }
        // 动态实例化裂纹贴花Prefab，并应用旋转
        GameObject newCrackDecal = Instantiate(crackDecalPrefab, position, decalRotation);
        // 将新生成的 CrackDecal 激活
        newCrackDecal.SetActive(true); 
        // 根据 decalSize 调整裂纹贴花的大小
        newCrackDecal.transform.localScale = new Vector3(decalSize, decalSize, decalSize);
        // 确保 CrackDecal 的位置正确
        newCrackDecal.transform.position = position;
        // 销毁裂纹贴花，在60秒后销毁
        Destroy(newCrackDecal, 60f);
    }
}