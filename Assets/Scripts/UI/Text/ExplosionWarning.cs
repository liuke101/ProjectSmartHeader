using System;
using TMPro;
using UnityEngine;

public class ExplosionWarning : MonoBehaviour
{
    private TMP_Text _textMeshPro;
    
    private void Awake()
    {
        _textMeshPro = GetComponent<TMP_Text>();
    }

    public void UpdateExplosionData(ExplosionData explosionData)
    {
        Debug.Log("ExplosionWarning: " + explosionData.BombType + " " + explosionData.StrikeLevel + " " + explosionData.X_Coordinate + " " + explosionData.Y_Coordinate);

        if (_textMeshPro)
        {
            string line1 = "检测到爆源！\n";
            string line2 = "类型：" + explosionData.BombType + "\n";
            string line3 = "打击等级：" + explosionData.StrikeLevel + "\n";
            string line4 = $"坐标：({explosionData.X_Coordinate},{explosionData.Y_Coordinate})";
            
            _textMeshPro.text = line1 + line2 + line3 + line4;
        }
    }
    
    
}