/*
 * Json数据管理类 主要用于进行 Json的序列化和反序列化
 * 需要引用 LitJson 库
 * 需要创建StreamingAssets文件夹
 * 
 * 序列化：JsonDataManager.Instance.SaveData(data, "fileName");
 * 反序列化：MyData data = JsonDataManager.Instance.LoadData<MyData>("fileName", JsonType.LitJson);
 */

using System.IO;
using LitJson;
using UnityEngine;

/// <summary>
/// Json序列化方案
/// </summary>
public enum JsonType
{
    JsonUtlity,
    LitJson,
}

/// <summary>
/// Json数据管理类 主要用于进行 Json的序列化和反序列化
/// </summary>
public class JsonDataManager
{
    private JsonDataManager() { }
        
    //单例模式

    public static JsonDataManager Instance { get; } = new JsonDataManager();


    //序列化，存储Json数据到硬盘
    public void SaveData(object data, string fileName, JsonType type = JsonType.LitJson)
    {
        //1. 确定存储路径
        string path = Application.persistentDataPath + "/" + fileName + ".json";
        
        //2. 序列化 得到Json字符串
        string jsonStr = "";
        switch (type)
        {
            case JsonType.JsonUtlity:
                jsonStr = JsonUtility.ToJson(data);
                break;
            case JsonType.LitJson:
                jsonStr = JsonMapper.ToJson(data);
                break;
        }
        //3. 把Json字符串存储到硬盘指定路径的文件中
        File.WriteAllText(path, jsonStr);
    }

    public T LoadData<T>(string fileName, JsonType type = JsonType.LitJson) where T : new() //限制泛型存在无参公共构造函数
    {
        //1. 确定从哪个路径读取
        //首先先判断，流动资源文件夹中是否有我们想要的数据，如果有就从中获取
        string path = Application.streamingAssetsPath + "/" + fileName + ".json";
        //如果没有，就从持久数据文件夹中获取
        if (!File.Exists(path))
        {
            path = Application.persistentDataPath + "/" + fileName + ".json";
        }
        //如果持久数据文件夹中也没有,那就返回默认对象
        if (!File.Exists(path))
        {
            return new T();
        }
        
        //2. 读取文件中的Json字符串
        string jsonStr = File.ReadAllText(path);
        
        //3. 反序列化，将Json字符串转换为类对象
        T data =  default(T);
        switch (type)
        {
            case JsonType.JsonUtlity:
                data = JsonUtility.FromJson<T>(jsonStr);
                break;
            case JsonType.LitJson:
                data = JsonMapper.ToObject<T>(jsonStr);
                break;
        }
        
        //4. 返回对象，通过该对象即可获取对象信息
        return data;
    }
}