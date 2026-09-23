using System;
using RD;
using UnityEngine;
using UnityObject = UnityEngine.Object;
using RD.Core;

namespace RD.Client
{
    public struct UIInvokeEntity_GetAssetInfo
    {
        public string Location;
        public Type AssetType;
    }

    public struct UIInvokeEntity_GetAssetInfoByGUID
    {
        public string AssetGUID;
        public Type AssetType;
    }

    // 加载任意实例化 to Vo
    public struct UIInvokeEntity_LoadInstantiateByVo
    {
        public UIBindVo BindVo;
        public EntityRef<Entity> ParentEntity;
        public Transform ParentTransform;
    }

    //实例化一个GameObject
    public struct UIInvokeEntity_InstantiateGameObject
    {
        public string PkgName;
        public string ResName;
    }

    //回收UI实例化资源
    public struct UIInvokeEntity_ReleaseInstantiate
    {
        public GameObject obj;
    }

    // 回收UI资源
    public struct UIInvokeEntity_Release
    {
        public UnityObject obj;
    }

    // 加载任意
    public struct UIInvokeEntity_Load
    {
        public Type LoadType;
        public string PkgName;
        public string ResName;
    }

    // 加载图片
    public struct UIInvokeEntity_LoadSprite
    {
        public string ResName;
    }

    // 回收UI 精灵/图片 资源
    public struct UIInvokeEntity_ReleaseSprite
    {
        public Sprite obj;
    }

    public struct UIInvokeEntity_LoadTexture2D
    {
        public string ResName;
    }

    //屏蔽所有UI操作
    public struct UIInvokeEntity_BanLayerOptionForever
    {
    }

    //恢复指定屏蔽的操作
    public struct UIInvokeEntity_RecoverLayerOptionForever
    {
        public long ForeverCode;
    }

    //添加倒计时
    public struct UIInvokeEntity_CountDownAdd
    {
        public CountDownTimerCallback TimerCallback;
        public double TotalTime;
        public double Interval;
        public bool StartCallback;
        public bool Forever;
    }

    //移除倒计时
    public struct UIInvokeEntity_CountDownRemove
    {
        public CountDownTimerCallback TimerCallback;
    }

    //等一帧 (1毫秒)
    public struct UIInvokeEntity_WaitFrameAsync
    {
    }

    //等指定毫秒
    public struct UIInvokeEntity_WaitAsync
    {
        public long Time;
        public PromiseCancellationToken CancellationToken;
    }

    //等指定秒
    public struct UIInvokeEntity_WaitSecondAsync
    {
        public float Time;
        public PromiseCancellationToken CancellationToken;
    }

    //协程锁
    public struct UIInvokeEntity_CoroutineLock
    {
        public long LockType;
        public long Lock;
    }
}