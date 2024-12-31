namespace JsonStruct
{
    public class ExplosiveSourceData
    {
        public ExplosiveSourceData() { }
        
        public string type { get; set; }
        public float strike_level { get; set; }
        public float x_coordinate { get; set; }
        public float y_coordinate { get; set; }
    }
    // 用于存储从 JSON 中解析出来的 damage_percent 的值
    public class DamageData
    {
        public DamageData(){ }
        public float damage_percent_1 { get; set; }
        public float damage_percent_2 { get; set; }
        public float damage_percent_3 { get; set; }
        public float damage_percent_4 { get; set; }
        public float damage_percent_5 { get; set; }
    }
}
