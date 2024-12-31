using System.Collections.Generic;
using JsonStruct;
using UnityEngine;


public class DamagePercentTest : MonoBehaviour
{
    public List<GameObject> BombObjects; // 你的五个组件

    // 用于模拟损伤占比数据
    public void SpawnDamagePercent()
    {
        // 随机生成损伤占比数据
        float damagePercent1 = Random.Range(0f, 100f);
        float damagePercent2 = Random.Range(0f, 100f);
        float damagePercent3 = Random.Range(0f, 100f);
        float damagePercent4 = Random.Range(0f, 100f);
        float damagePercent5 = Random.Range(0f, 100f);

        // 打印损伤占比数据
        DamageData damageData = new DamageData()
        {
            damage_percent_1 = damagePercent1,
            damage_percent_2 = damagePercent2,
            damage_percent_3 = damagePercent3,
            damage_percent_4 = damagePercent4,
            damage_percent_5 = damagePercent5
        };

        // 打印到UI
        MessageBox.Instance.PrintDamagePercentData(damageData);

        // 处理损伤占比并更新颜色
        HandleDamagePercent(damagePercent1, BombObjects[0]);
        HandleDamagePercent(damagePercent2, BombObjects[1]);
        HandleDamagePercent(damagePercent3, BombObjects[2]);
        HandleDamagePercent(damagePercent4, BombObjects[3]);
        HandleDamagePercent(damagePercent5, BombObjects[4]);
    }

    // 处理损伤占比并改变颜色
    public void HandleDamagePercent(float damagePercent, GameObject component)
    {
        Color color;
        
        if (damagePercent <= 30)
        {
            color = Color.green; // 低损伤
        }
        else if (damagePercent <= 70)
        {
            color = Color.yellow; // 中等损伤
        }
        else
        {
            color = Color.red; // 高损伤
        }

        Renderer renderer = component.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = color;
        }
    }
}