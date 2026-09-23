using DG.Tweening;
using UnityEngine;
using RD.Client;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// 简单的窗口动画
    /// 只是一个案例 不需要可以删除
    /// 这里用到dotween动画 ettask 简单的扩展
    /// 目前已知BUG 同时有多个人调用动画 其他异步还没有完成时 被其他的删除了就会错误
    /// 导致其他异步全部中断
    /// </summary>
    public static class WindowFadeAnim
    {
        //对dotween的异步扩展
        //临时方案 还不够完善
        //目前这个只是在UI动画上使用 其他地方请自行实现
        private static async Promise GetAwaiter(this Tweener tweener)
        {
            var task = Promise.Create(true);
            tweener.onComplete += () =>
            {
                task.SetResult();
            };
            await task;
        }

        //淡入
        public static async Promise In(GameObject gameObject, float time = 0.25f)
        {
            if (UIConstHelper.Const.BanDotweenAnim) return;

            if (gameObject == null) return;

            gameObject.SetActive(true);

            gameObject.transform.localScale = UIConstHelper.Const.DotweenAnimScale;

            await gameObject.transform.DOScale(Vector3.one, time);
        }

        //淡出
        public static async Promise Out(GameObject gameObject, float time = 0.25f)
        {
            if (UIConstHelper.Const.BanDotweenAnim) return;

            if (gameObject == null) return;

            gameObject.transform.localScale = Vector3.one;

            await gameObject.transform.DOScale(UIConstHelper.Const.DotweenAnimScale, time);

            if (gameObject == null) return;

            gameObject.SetActive(false);

            // ReSharper disable once Unity.InefficientPropertyAccess
            gameObject.transform.localScale = Vector3.one;
        }
    }
}
