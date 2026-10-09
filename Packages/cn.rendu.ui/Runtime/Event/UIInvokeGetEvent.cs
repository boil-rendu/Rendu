using System;
using UnityObject = UnityEngine.Object;
using RD.Core;

namespace RD.Client
{
    /// <summary>查询 UI 世界根节点的请求</summary>
    public struct UIInvokeEntity_GetUIRoot
    {
    }

    /// <summary>查询 Canvas 根节点的请求</summary>
    public struct UIInvokeEntity_GetUICanvasRoot
    {
    }

    /// <summary>查询层级根节点的请求</summary>
    public struct UIInvokeEntity_GetUILayerRoot
    {
    }

    /// <summary>查询 UI 相机的请求</summary>
    public struct UIInvokeEntity_GetUICamera
    {
    }

    /// <summary>查询 UI 根 Canvas 的请求</summary>
    public struct UIInvokeEntity_GetUICanvas
    {
    }
}