namespace JsonStruct
{
    // 爆源定位Json
    // {
    //     ThingID：edu.whut.cs.iot.se:explosion
    //         Feature：{
    //         type: 温压弹/堵口爆/核爆,
    //         strike_level: 一级/二级/三级/四级,	
    //         x_coordinate: 15.1,
    //         y_coordinate: 35.1
    //     }
    // }
    
    /* Json数据结构 */
    public class Feature
    {
        public Feature() { }
        
        public Feature(string type, string strike_level, float x_coordinate, float y_coordinate)
        {
            this.type = type;
            this.strike_level = strike_level;
            this.x_coordinate = x_coordinate;
            this.y_coordinate = y_coordinate;
        }

        public string type { get; set; } //温压弹/堵口爆/核爆
        public string strike_level { get; set; } //一级~四级
        public float x_coordinate { get; set; }
        public float y_coordinate { get; set; }
    }

    public class ExplosiveSourceData
    {
        public ExplosiveSourceData() { }
        
        public ExplosiveSourceData(string thingID, Feature feature)
        {
            ThingID = thingID;
            Feature = feature;
        }

        public string ThingID { get; set; }
        public Feature Feature { get; set; }
    }
}
