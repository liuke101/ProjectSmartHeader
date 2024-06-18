using System;
using System.Collections;
using System.Collections.Generic;
using INab.WorldScanFX;
using INab.WorldScanFX.URP;
using UnityEngine;
using UnityEngine.Serialization;

public enum EBombType 
{
    温压弹,
    堵口爆,
    核弹,
}

public enum EBombLevel
{
    ONE,
    TWO,
    THREE,
    FOUR,
}

public class Bomb : MonoBehaviour
{
    [FormerlySerializedAs("BombType")] [Header("炸弹参数")]
    public EBombType EBombType; //炸弹类型
    public float LifeTime = 10.0f; //生命周期，防止未发生碰撞炸弹一直存在
    public ParticleSystem ExplosionParticle; //爆炸粒子
    public AudioClip ExplosionAudio; //爆炸音效
    private float BombRange = 10.0f;  //根据爆炸等级设置炸弹范围,TODO:影响范围显示
    private Rigidbody Rigidbody;
    private EBombLevel BombLevel;
    
    [Header("抛物线")]
    // 目标点Transform
    private Transform TargetPointTransform; 
    // 运动速度
    public float speed = 10;
    // 最小接近距离, 以停止运动
    public float MinDistance = 0.5f;
    private float DistanceToTarget;
    private bool bCanMove = true;

    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody>();
        gameObject.layer = LayerMask.NameToLayer("Bomb"); //设置炸弹层
        Rigidbody.excludeLayers = LayerMask.GetMask("Bomb"); //排除炸弹之间的碰撞
    }

    private void Start()
    {
        Destroy(gameObject, LifeTime);  //生命周期结束后销毁
        
        //抛物线发射
        DistanceToTarget = Vector3.Distance(transform.position, TargetPointTransform.position);
        StartCoroutine(Launch());
    }

    private void Update()
    {
    }

    private void OnCollisionEnter(Collision other)
    {
       
    }

    private void OnTriggerEnter(Collider other)
    {
        Explosion(); //碰撞后爆炸
    }

    public virtual void SpawnBomb(Transform SpawnPointTransform, Transform TargetPointTransfrom, EBombLevel level)
    {
        //生成炸弹
        Bomb bomb = Instantiate(gameObject.GetComponent<Bomb>(), SpawnPointTransform.position, SpawnPointTransform.rotation);
        bomb.TargetPointTransform = TargetPointTransfrom;
        bomb.BombLevel = level;
        
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
        if (SpawnBombController.Instance.ScanFX)
        {
            //根据等级设置扫描范围
            switch (BombLevel)
            {
                case EBombLevel.ONE:
                    SpawnBombController.Instance.ScanFX.MaskRadius = 1.0f;
                    break;
                case EBombLevel.TWO:
                    SpawnBombController.Instance.ScanFX.MaskRadius = 5.0f;
                    break;
                case EBombLevel.THREE:
                    SpawnBombController.Instance.ScanFX.MaskRadius = 10.0f;
                    break;
                case EBombLevel.FOUR:
                    SpawnBombController.Instance.ScanFX.MaskRadius = 20.0f;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            //设置扫描线的新位置
            SpawnBombController.Instance.ScanFX.PassCustomScanOriginProperties(transform);
            
            // TODO:目前同时只能有一个扫描线
            if (SpawnBombController.Instance.ScanFX.ScansLeft <= 0)
            {
                SpawnBombController.Instance.ScanFX.StartScan(1);
            }
        }

        //销毁炮弹
        Destroy(gameObject, 10.0f);
    }
    
    
    IEnumerator Launch()
    {
        while (bCanMove)
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
            transform.position = TargetPointTransform.position;
            // [停止]当前协程任务,参数是协程方法名
            StopCoroutine(Launch());
            // 销毁脚本
            GameObject.Destroy(this);
        }
    }
}