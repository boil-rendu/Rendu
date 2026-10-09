namespace RD.Client
{
    /// <summary>UI 等待提示扩展（UIWindowComponent）</summary>
    public static class UIWaitHelper
    {
        /// <summary>通知等待组件结束等待并携带错误码（无 UIWaitComponent 时静默忽略）</summary>
        public static void NotifyWait(this UIWindowComponent self, EHashWaitError error)
        {
            var waitComponent = self.GetComponent<UIWaitComponent>();
            if (waitComponent == null) return;
            waitComponent.Notify(error);
        }
    }
}