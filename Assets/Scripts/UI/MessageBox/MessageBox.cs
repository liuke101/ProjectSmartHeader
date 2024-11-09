using JsonStruct;
using TMPro;
using UnityEngine;

public class MessageBox : MonoSingleton<MessageBox>
{
    public TMP_Text _textMeshPro;
    
    public void PrintExplosionData(ExplosiveSourceData explosionData)
    {
        PrintMessage($"检测到爆源! 类型:{explosionData.type}; 打击等级:{explosionData.strike_level}; 坐标:({explosionData.x_coordinate:F3},{explosionData.y_coordinate:F3})");
    }

    public void PrintMessage(string message)
    {
        if (_textMeshPro)
        {
            //换行
            _textMeshPro.text += "\n";
                
            _textMeshPro.text += $"<color=green>{System.DateTime.Now} </color> <color=white>{message}</color>";
            //设置字体颜色
            _textMeshPro.color = Color.red;
            
            //todo:清理多余的行
        }
    }
}