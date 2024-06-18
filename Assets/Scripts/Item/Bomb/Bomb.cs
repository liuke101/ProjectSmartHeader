using System.Collections;
using System.Collections.Generic;
using INab.WorldScanFX;
using INab.WorldScanFX.URP;
using UnityEngine;
using UnityEngine.Serialization;

public enum BombType 
{
    温压弹,
    堵口爆,
    核弹,
}

public enum BombLevel
{
    ONE,
    TWO,
    THREE,
    FOUR,
}

public class Bomb : MonoBehaviour
{
    [Header("炸弹参数")]
    public BombType BombType; //炸弹类型
    public float LifeTime = 10.0f; //生命周期，防止未发生碰撞炸弹一直存在
    public ParticleSystem ExplosionParticle; //爆炸粒子
    public AudioClip ExplosionAudio; //爆炸音效
    private float BombRange = 10.0f;  //根据爆炸等级设置炸弹范围,TODO:影响范围显示
    private Rigidbody Rigidbody;
    [Header("扫描线")]
    public ScanFX ScanFX;

    [FormerlySerializedAs("target_trans")] [Header("抛物线")]
    // 目标点Transform
    private Transform TargetPointTransform; 
    // 运动速度
    public float speed = 10;
    // 最小接近距离, 以停止运动
    public float MinDistance = 0.5f;
    private float DistanceToTarget;
    private bool move_flag = true;
    //private Transform m_trans;

    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody>();
        gameObject.layer = LayerMask.NameToLayer("Bomb"); //设置炸弹层
        Rigidbody.excludeLayers = LayerMask.GetMask("Bomb"); //排除炸弹之间的碰撞
        
        //从子对象上获取ScanFX组件
        ScanFX = GetComponentInChildren<ScanFX>(); 
        if (ScanFX)
        {
            //获取场景中类型为ScanFXHighlight的物体
            List<ScanFXHighlight> highlightObjects = new List<ScanFXHighlight>();
            foreach (var item in FindObjectsOfType<ScanFXHighlight>())
            {
                highlightObjects.Add(item);
                print(item);
            }

            //保证highlightObjects在Start前不为null
            if (highlightObjects.Count > 0)
            {
                ScanFX.highlightObjects = highlightObjects;
            }
        }
    }

    private void Start()
    {
        Destroy(gameObject, LifeTime);  //生命周期结束后销毁
        
        //m_trans = this.transform; 
        DistanceToTarget = Vector3.Distance(transform.position, TargetPointTransform.position);
        StartCoroutine(BombProjector());
    }

    private void Update()
    {
    }

    private void OnCollisionEnter(Collision other)
    {
        Explosion(); //碰撞后爆炸
    }

    public virtual void SpawnBomb(Transform SpawnPointTransform, Transform TargetPointTransfrom, BombLevel level)
    {
        switch (level)
        {
            case BombLevel.ONE:
                this.BombRange = 10;
                break;
            case BombLevel.TWO:
                this.BombRange = 20;
                break;
            case BombLevel.THREE:
                this.BombRange = 30;
                break;
            case BombLevel.FOUR:
                this.BombRange = 40;
                break;
        }
        
        //生成炸弹
        Bomb bomb = Instantiate(gameObject.GetComponent<Bomb>(), SpawnPointTransform.position, SpawnPointTransform.rotation);
        bomb.TargetPointTransform = TargetPointTransfrom;
        
        //根据爆炸等级设置MaskRadius和Size
        if(bomb.GetComponent<ScanFX>())
        {
            // bomb.GetComponent<ScanFX>().MaskRadius = 
            // bomb.GetComponent<ScanFX>().Size = 
        }
    }
    
    protected virtual void Explosion()
    {
        //爆炸粒子
        if(ExplosionParticle)
        {
            ParticleSystem Explosion = Instantiate(ExplosionParticle, transform.position, Quaternion.identity); 
            Destroy(Explosion, 2.0f);
        }
        
        //爆炸音效
        if (ExplosionAudio)
        {
            GetComponent<AudioSource>().PlayOneShot(ExplosionAudio); 
        }
        
        //扫描线
        if (ScanFX)
        {
            //TOOD:同时扫描
            ScanFX TempScanFX = Instantiate(ScanFX, transform.position, Quaternion.identity);
            TempScanFX.StartScan(1);
            Destroy(TempScanFX,TempScanFX.scanDuration);
        }

        //销毁炮弹
        Destroy(gameObject, 0.2f);
    }
    
    
    IEnumerator BombProjector()
    {
        while (move_flag)
        {
            Vector3 targetPos = TargetPointTransform.position;
            // 朝向目标, 以计算运动
            transform.LookAt(targetPos);
            // 根据距离衰减 角度
            float angle = Mathf.Min(1, Vector3.Distance(transform.position, targetPos) / DistanceToTarget) * 45;
            // 旋转对应的角度（线性插值一定角度，然后每帧绕X轴旋转）
            transform.rotation = transform.rotation * Quaternion.Euler(Mathf.Clamp(-angle, -42, 42), 0, 0);
            // 当前距离目标点
            float currentDist = Vector3.Distance(transform.position, TargetPointTransform.position);
            // 很接近目标了, 准备结束循环
            if (currentDist < MinDistance)
            {
                move_flag = false; 
            }
            // 平移 (朝向Z轴移动)
            transform.Translate(Vector3.forward * Mathf.Min(speed * Time.deltaTime, currentDist));
            // 暂停执行, 等待下一帧再执行while
            yield return null;
        }
        if (move_flag == false)
        {
            // 使自己的位置, 跟[目标点]重合
            transform.position = TargetPointTransform.position;
            // [停止]当前协程任务,参数是协程方法名
            StopCoroutine(BombProjector());
            // 销毁脚本
            GameObject.Destroy(this);
        }
    }
}