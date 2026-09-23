namespace RD.Client
{
    public static class UIWaitHelper
    {
        public static void NotifyWait(this UIWindowComponent self, EHashWaitError error)
        {
            var waitComponent = self.GetComponent<UIWaitComponent>();
            if (waitComponent == null) return;
            waitComponent.Notify(error);
        }
    }
}