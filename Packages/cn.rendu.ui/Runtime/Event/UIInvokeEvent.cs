using System;
using RD;
using UnityEngine;
using UnityObject = UnityEngine.Object;
using RD.Core;

namespace RD.Client
{
    /// <summary>查询资源信息的请求</summary>
    public struct UIInvokeEntity_GetAssetInfo
    {
        public string Location;
        public Type AssetType;
    }

    /// <summary>按 GUID 查询资源信息的请求</summary>
    public struct UIInvokeEntity_GetAssetInfoByGUID
    {
        public string AssetGUID;
        public Type AssetType;
    }

    // 加载任意实例化 to Vo
    /// <summary>按 UIBindVo 加载并实例化的请求（附带父实体与父节点）</summary>
    public struct UIInvokeEntity_LoadInstantiateByVo
    {
        public UIBindVo BindVo;
        public EntityRef<Entity> ParentEntity;
        public Transform ParentTransform;
    }

    //实例化一个GameObject
    /// <summary>实例化一个预制体的请求</summary>
    public struct UIInvokeEntity_InstantiateGameObject
    {
        public string PkgName;
        public string ResName;
    }

    //回收UI实例化资源
    /// <summary>回收 UI 实例化资源的请求</summary>
    public struct UIInvokeEntity_ReleaseInstantiate
    {
        public GameObject obj;
    }

    // 回收UI资源
    /// <summary>回收 UI 资源的请求</summary>
    public struct UIInvokeEntity_Release
    {
        public UnityObject obj;
    }

    // 加载任意
    /// <summary>加载任意类型资源的请求</summary>
    public struct UIInvokeEntity_Load
    {
        public Type LoadType;
        public string PkgName;
        public string ResName;
    }

    // 加载图片
    /// <summary>加载 Sprite 的请求</summary>
    public struct UIInvokeEntity_LoadSprite
    {
        public string ResName;
    }

    // 回收UI 精灵/图片 资源
    /// <summary>回收 Sprite 的请求</summary>
    public struct UIInvokeEntity_ReleaseSprite
    {
        public Sprite obj;
    }

    /// <summary>加载 Texture2D 的请求</summary>
    public struct UIInvokeEntity_LoadTexture2D
    {
        public string ResName;
    }

    //屏蔽所有UI操作
    /// <summary>永久屏蔽所有 UI 操作的请求</summary>
    public struct UIInvokeEntity_BanLayerOptionForever
    {
    }

    //恢复指定屏蔽的操作
    /// <summary>按 ForeverCode 恢复指定永久屏蔽的请求</summary>
    public struct UIInvokeEntity_RecoverLayerOptionForever
    {
        public long ForeverCode;
    }

    //添加倒计时
    /// <summary>添加倒计时的请求</summary>
    public struct UIInvokeEntity_CountDownAdd
    {
        public CountDownTimerCallback TimerCallback;
        public double TotalTime;
        public double Interval;
        public bool StartCallback;
        public bool Forever;
    }

    //移除倒计时
    /// <summary>移除倒计时的请求</summary>
    public struct UIInvokeEntity_CountDownRemove
    {
        public CountDownTimerCallback TimerCallback;
    }

    //等一帧 (1毫秒)
    /// <summary>等待一帧的请求</summary>
    public struct UIInvokeEntity_WaitFrameAsync
    {
    }

    //等指定毫秒
    /// <summary>等待指定毫秒的请求（可带取消令牌）</summary>
    public struct UIInvokeEntity_WaitAsync
    {
        public long Time;
        public PromiseCancellationToken CancellationToken;
    }

    //等指定秒
    /// <summary>等待指定秒数的请求（可带取消令牌）</summary>
    public struct UIInvokeEntity_WaitSecondAsync
    {
        public float Time;
        public PromiseCancellationToken CancellationToken;
    }

    //协程锁
    /// <summary>请求协程锁的入参（锁类型 + 锁键）</summary>
    public struct UIInvokeEntity_CoroutineLock
    {
        public long LockType;
        public long Lock;
    }
}