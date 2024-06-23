using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class Bomb : MonoBehaviour
{
    [Header("炸弹参数")]
    public EBombType BombType; //炸弹类型
    public float LifeTime = 10.0f; //生命周期，防止未发生碰撞炸弹一直存在
    public StrikeLevelData StrikeLevelData;
    public BombTypeData BombTypeData;
    
    [HideInInspector]public Vector3 TargetPosition;  // 目标点Transform
    public float speed = 10; // 运动速度
    public float MinDistance = 0.5f; // 最小接近距离, 以停止运动
    private float DistanceToTarget;
    private bool bCanMove = true;
    [HideInInspector]public int StrikeLevel;
    private bool bHasBoom = false;  //已爆炸过
    
    
    private void Awake()
    {
        gameObject.layer = LayerMask.NameToLayer("Bomb"); //设置炸弹层
        GetComponent<Rigidbody>().excludeLayers = LayerMask.GetMask("Bomb"); //排除炸弹之间的碰撞
        GetComponent<Collider>().excludeLayers = LayerMask.GetMask("Bomb"); //排除炸弹之间的碰撞
        GetComponent<Collider>().isTrigger = true; //设置为触发器
    }

    private void Start()
    {
        Destroy(gameObject, LifeTime);  //生命周期结束后销毁
        
        //抛物线发射
        DistanceToTarget = Vector3.Distance(transform.position, TargetPosition);
        StartCoroutine(Launch());
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (bHasBoom==false)
        {
            Explosion(); //碰撞后爆炸
        }
        
        GetComponent<Collider>().isTrigger = false; //关闭触发器
        
        bHasBoom = true;
    }
    
    protected virtual void Explosion()
    {
        //爆炸特效和爆炸冲击波
        GameObject ExplosionVFX = BombTypeData?.GetAssetByBombType(BombType).ExplosionVFX;
        if(ExplosionVFX!=null)
        {
            GameObject NewExplosionVFX = Instantiate(ExplosionVFX, transform.position, Quaternion.identity);
            if (NewExplosionVFX != null)
            {
                HS_ShakeOnCollision shakeOnCollision = NewExplosionVFX.GetComponentInChildren<HS_ShakeOnCollision>();
                if(shakeOnCollision !=null && StrikeLevel>0 && StrikeLevelData.BombHoleDecalSize.Count >= 4)
                {
                    //根据等级设置ShakeWave影响范围
                    shakeOnCollision.explosionFinalRadious = StrikeLevelData.ShockWaveRange[StrikeLevel-1];
                
                    //TODO:振动频率等参数待调整
                }
            }
            Destroy(NewExplosionVFX.gameObject, 20.0f); //销毁时间待定
        }
        
        //扫描线
        if (SpawnBombController.Instance.ScanFX)
        {
            //设置扫描线的新位置
            SpawnBombController.Instance.ScanFX.PassCustomScanOriginProperties(transform);
            
            // TODO:目前同时只能有一个扫描线
            if (SpawnBombController.Instance.ScanFX.ScansLeft <= 0)
            {
                SpawnBombController.Instance.ScanFX.StartScan(1);
            }
        }

        //爆炸弹坑Decal
        GameObject BombHoleDecal = BombTypeData?.GetAssetByBombType(BombType).BombHoleDecal;
        if(BombHoleDecal!=null)
        {
            GameObject NewDecal = Instantiate(BombHoleDecal, transform.position, Quaternion.Euler(90, 0, 0)); //生成炸弹贴花，绕X轴旋转90度，正对地面
            
            //根据等级设置弹坑贴花大小
            if (StrikeLevel>0 && StrikeLevelData.BombHoleDecalSize.Count >= 4)
            {
                float DecalSize = StrikeLevelData.BombHoleDecalSize[StrikeLevel-1];
                NewDecal.GetComponent<DecalProjector>().size = new Vector3(DecalSize, DecalSize, DecalSize);
            }
            
            Destroy(NewDecal.gameObject, 60.0f);
        }

        //销毁炮弹
        Destroy(gameObject, 1.0f);
    }
    
    
    IEnumerator Launch()
    {
        while (bCanMove)
        {
            Vector3 targetPos = TargetPosition;
            // 朝向目标, 以计算运动
            transform.LookAt(targetPos);
            // 根据距离衰减 角度
            float angle = Mathf.Min(1, Vector3.Distance(transform.position, targetPos) / DistanceToTarget) * 45;
            // 旋转对应的角度（线性插值一定角度，然后每帧绕X轴旋转）
            transform.rotation *= Quaternion.Euler(Mathf.Clamp(-angle, -42, 42), 0, 0);
            // 当前距离目标点
            float currentDist = Vector3.Distance(transform.position, TargetPosition);
            // 很接近目标了, 准备结束循环
            if (currentDist < MinDistance)
            {
                bCanMove = false; 
            }
            // 平移 (朝向Z轴移动)
            transform.Translate(Vector3.forward * Mathf.Min(speed * Time.deltaTime, currentDist));
            // 暂停执行, 等待下一帧再执行while
            yield return null;
        }
        if (bCanMove == false)
        {
            // 使自己的位置, 跟[目标点]重合
            transform.position = TargetPosition;
            // [停止]当前协程任务,参数是协程方法名
            StopCoroutine(Launch());
        }
    }

    
}