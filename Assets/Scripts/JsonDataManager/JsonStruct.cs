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
        public DamageData() { }
        public float GetDamagePercent(int index)
        {
            switch (index)
            {
                case 0: return damage_percent_1;
                case 1: return damage_percent_2;
                case 2: return damage_percent_3;
                case 3: return damage_percent_4;
                case 4: return damage_percent_5;
                default: return 0f;
            }
        }
        public float damage_percent_1 { get; set; }
        public float damage_percent_2 { get; set; }
        public float damage_percent_3 { get; set; }
        public float damage_percent_4 { get; set; }
        public float damage_percent_5 { get; set; }
    }
    public class SensorData
    {
        public SensorData() { }
        
        public float channel1 { get; set; }
        public float channel2 { get; set; }
        public float channel3 { get; set; }
        public float channel4 { get; set; }
        public float channel5 { get; set; }
        public float channel6 { get; set; }
        public float channel7 { get; set; }
        public float channel8 { get; set; }
        public float channel9 { get; set; }
        public float channel10 { get; set; }
        public float channel11 { get; set; }
        public float channel12 { get; set; }
        public float channel13 { get; set; }
        public float channel14 { get; set; }
        public float channel15 { get; set; }
        public float channel16 { get; set; }
        public float channel17 { get; set; }
        public float channel18 { get; set; }
        public float channel19 { get; set; }
        public float channel20 { get; set; }
        public float channel21 { get; set; }
        public float channel22 { get; set; }
        public float channel23 { get; set; }
        public float channel24 { get; set; }
        public float channel25 { get; set; }
        public float channel26 { get; set; }
        public float channel27 { get; set; }
        public float channel28 { get; set; }
        public float channel29 { get; set; }
        public float channel30 { get; set; }
        public float channel31 { get; set; }
        public float channel32 { get; set; }
        public float channel33 { get; set; }
        public float channel34 { get; set; }
        public float channel35 { get; set; }
        public float channel36 { get; set; }
        public float channel37 { get; set; }
        public float channel38 { get; set; }
        public float channel39 { get; set; }
        public float channel40 { get; set; }
        public float channel41 { get; set; }
        public float channel42 { get; set; }
        public float channel43 { get; set; }
        public float channel44 { get; set; }
        public float channel45 { get; set; }
        public float channel46 { get; set; }
        public float channel47 { get; set; }
        public float channel48 { get; set; }
        public float channel49 { get; set; }
        public float channel50 { get; set; }
        public float channel51 { get; set; }
        public float channel52 { get; set; }
        public float channel53 { get; set; }
        public float channel54 { get; set; }
        public float channel55 { get; set; }
        public float channel56 { get; set; }
        public float channel57 { get; set; }
        public float channel58 { get; set; }
        public float channel59 { get; set; }
        public float channel60 { get; set; }
        public float channel61 { get; set; }
        public float channel62 { get; set; }
        public float channel63 { get; set; }
        public float channel64 { get; set; }
        public float channel65 { get; set; }
        public float channel66 { get; set; }
        public float channel67 { get; set; }
        public float channel68 { get; set; }
        public float channel69 { get; set; }
        public float channel70 { get; set; }
        public float channel71 { get; set; }
        public float channel72 { get; set; }
        public float channel73 { get; set; }
        public float channel74 { get; set; }
        public float channel75 { get; set; }
        public float channel76 { get; set; }
        public float channel77 { get; set; }
        public float channel78 { get; set; }
        public float channel79 { get; set; }
        public float channel80 { get; set; }
        public float channel81 { get; set; }
        public float channel82 { get; set; }
        public float channel83 { get; set; }
        public float channel84 { get; set; }
        public float channel85 { get; set; }
        public float channel86 { get; set; }
        public float channel87 { get; set; }
        public float channel88 { get; set; }
        public float channel89 { get; set; }
        public float channel90 { get; set; }
        public float channel91 { get; set; }
        public float channel92 { get; set; }
        public float channel93 { get; set; }
        public float channel94 { get; set; }
        public float channel95 { get; set; }
        public float channel96 { get; set; }
        public float wavelength1 { get; set; }
        public float wavelength2 { get; set; }
        public float wavelength3 { get; set; }
        public float wavelength4 { get; set; }
        public float wavelength5 { get; set; }
        public float wavelength6 { get; set; }
        public float wavelength7 { get; set; }
        public float wavelength8 { get; set; }
        public float wavelength9 { get; set; }
        public float wavelength10 { get; set; }
        public float wavelength11 { get; set; }
        public float wavelength12 { get; set; }
        public float wavelength13 { get; set; }
        public float wavelength14 { get; set; }
        public float wavelength15 { get; set; }
        public float wavelength16 { get; set; }
        public float wavelength17 { get; set; }
        public float wavelength18 { get; set; }
        public float wavelength19 { get; set; }
        public float wavelength20 { get; set; }
        public float wavelength21 { get; set; }
        public float wavelength22 { get; set; }
        public float wavelength23 { get; set; }
        public float wavelength24 { get; set; }
        public float wavelength25 { get; set; }
        public float wavelength26 { get; set; }
        public float wavelength27 { get; set; }
        public float wavelength28 { get; set; }
        public float wavelength29 { get; set; }
        public float wavelength30 { get; set; }
        public float wavelength31 { get; set; }
        public float wavelength32 { get; set; }
        public float wavelength33 { get; set; }
        public float wavelength34 { get; set; }
        public float wavelength35 { get; set; }
        public float wavelength36 { get; set; }
        public float wavelength37 { get; set; }
        public float wavelength38 { get; set; }
        public float wavelength39 { get; set; }
        public float wavelength40 { get; set; }
    }
}