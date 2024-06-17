using System.Collections.Generic;
using UnityEngine;

/* Json数据结构 */
public class Feature
{
    public Feature() { }

    public string type;
    public string strike_level { get; set; } //一级~四级
    public double x_coordinate { get; set; }
    public double y_coordinate { get; set; }
}

public class ExplosiveSourceData
{
    public ExplosiveSourceData() { }

    public string ThingID { get; set; }
    public Feature Feature { get; set; }
}

public class ExplosiveSourceLocalization : MonoBehaviour
{
    //存储所有发送来的数据，并在处理后删除
    protected List<ExplosiveSourceData> ExplosiveSourceDatas; 
    void Start()
    {
        //1. 反序列化，将Json字符串转换为类对象
        ExplosiveSourceData data = JsonDataManager.Instance.LoadData<ExplosiveSourceData>("TestData", JsonType.LitJson);
    
        //2. 获取data对象的信息
        print(data.ThingID);
        print(data.Feature.type);
        print(data.Feature.strike_level);
        print(data.Feature.x_coordinate);
        print(data.Feature.y_coordinate);
    }

    void Update()
    {
    
    }
}
