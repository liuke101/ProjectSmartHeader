using System;
using System.Collections.Generic;
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
    public BombType BombType; //炸弹类型
    public float LifeTime = 10.0f; //生命周期，防止未发生碰撞炸弹一直存在
    public ParticleSystem ExplosionParticle; //爆炸粒子
    public AudioClip ExplosionAudio; //爆炸音效
    private float BombRange;  //根据爆炸等级设置炸弹范围,TODO:影响范围显示
    
    private void Awake()
    {
        gameObject.layer = LayerMask.NameToLayer("Bomb"); //设置炸弹层
        GetComponent<Rigidbody>().excludeLayers = LayerMask.GetMask("Bomb"); //排除炸弹之间的碰撞
    }

    private void Start()
    {
        Destroy(gameObject, LifeTime);  //生命周期结束后销毁
    }
    
    private void OnCollisionEnter(Collision other)
    {
        Explosion(); //碰撞后爆炸
    }

    public virtual void SpawnBomb(Vector3 position, BombLevel level)
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
        GameObject bomb = Instantiate(gameObject, position, Quaternion.identity);
    }
    
    protected virtual void Explosion()
    {
        if(ExplosionParticle!=null)
        {
            ParticleSystem Explosion = Instantiate(ExplosionParticle, transform.position, Quaternion.identity); //爆炸粒子
            Destroy(Explosion, 2.0f);
            GetComponent<AudioSource>().PlayOneShot(ExplosionAudio); //爆炸音效
        }

        Destroy(gameObject, 0.2f);
    }
}