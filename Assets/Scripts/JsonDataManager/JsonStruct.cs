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
    public class ExplosiveSourceData
    {
        public ExplosiveSourceData() { }
        
        public ExplosiveSourceData(string thingId, Feature feature)
        {
            this.thingId = thingId;
            this.feature = feature;
        }
    
        public string thingId { get; set; }
        public Feature feature { get; set; }
    }
    public class Feature
    {
        public Feature() { }
        
        public Feature(string type, int strike_level, float x_coordinate, float y_coordinate)
        {
            this.type = type;
            this.strike_level = strike_level;
            this.x_coordinate = x_coordinate;
            this.y_coordinate = y_coordinate;
        }
    
        public string type { get; set; } //温压弹/堵口爆/核爆
        public int strike_level { get; set; } //一级~四级
        public float x_coordinate { get; set; }
        public float y_coordinate { get; set; }
    }
    
    
    
    /////////////////////////////////////////////////////////
    
    // public class ExplosiveSourceData
    // {
    //     public Headers headers { get; set; }
    //     public string path { get; set; }
    //     public string topic { get; set; }
    //     public Value value { get; set; }
    // }
    //
    // public class Headers
    // {
    //     public string correlation_id { get; set; }
    //     public string content_type { get; set; }
    // }
    //
    // public class Value
    // {
    //     public Features features { get; set; }
    //     public string policyId { get; set; }
    //     public string thingId { get; set; }
    // }
    //
    // public class Features
    // {
    //     public XCoordinate x_coordinate { get; set; }
    //     public StrikeLevel strike_level { get; set; }
    //     public Type type { get; set; }
    //     public YCoordinate y_coordinate { get; set; }
    // }
    //
    // public class XCoordinate
    // {
    //     public Properties properties { get; set; }
    // }
    //
    // public class StrikeLevel
    // {
    //     public Properties properties { get; set; }
    // }
    //
    // public class Type
    // {
    //     public Properties properties { get; set; }
    // }
    //
    // public class YCoordinate
    // {
    //     public Properties properties { get; set; }
    // }
    //
    // public class Properties
    // {
    //     public object value { get; set; }
    // }
}
