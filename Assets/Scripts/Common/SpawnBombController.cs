using System.Collections.Generic;
using JsonStruct;
using UnityEngine;

public class SpawnBombController : MonoSingleton<SpawnBombController>
{
    public List<Bomb> Bombs;
    
    public void SpawnBomb()
    {
        //1. 创建一个类对象
        
        //创建一个string，随机生成一个炸弹类型
        string[] Types = new string[] {"温压弹", "堵口爆", "核弹"};
        string Type = Types[Random.Range(0, 3)];
        
        //随机等级
        string[] Levels = new string[] {"一级", "二级", "三级", "四级"};
        string Level = Levels[Random.Range(0, 4)];
        
        Feature feature = new Feature(Type,Level, Random.Range(0, 10), Random.Range(0, 10));
        ExplosiveSourceData data = new ExplosiveSourceData("edu.whut.cs.iot.se:explosion", feature);
        
        //如果有，处理数据，落点，爆炸。。。
        print(data.ThingID);
        print(data.Feature.type);
        print(data.Feature.strike_level);
        print(data.Feature.x_coordinate);
        print(data.Feature.y_coordinate);
        
        //根据strike_level获取对应的BombLevel枚举
        BombLevel bombLevel = BombLevel.ONE;
        switch (data.Feature.strike_level)
        {
            case "一级":
                bombLevel = BombLevel.ONE;
                break;
            case "二级":
                bombLevel = BombLevel.TWO;
                break;
            case "三级":
                bombLevel = BombLevel.THREE;
                break;
            case "四级":
                bombLevel = BombLevel.FOUR;
                break;
        }

        switch (data.Feature.type)
        {
            case "温压弹":
                Bombs.Find(bomb => bomb.BombType == BombType.温压弹).SpawnBomb(new Vector3(data.Feature.x_coordinate, 10, data.Feature.y_coordinate), bombLevel);
                break;
            case "堵口爆":
                Bombs.Find(bomb => bomb.BombType == BombType.堵口爆)
                    .SpawnBomb(new Vector3(data.Feature.x_coordinate, 10, data.Feature.y_coordinate), bombLevel);
                break;
            case "核弹":
                Bombs.Find(bomb => bomb.BombType == BombType.核弹)
                    .SpawnBomb(new Vector3(data.Feature.x_coordinate, 10, data.Feature.y_coordinate), bombLevel);
                break;
        }
    }
}