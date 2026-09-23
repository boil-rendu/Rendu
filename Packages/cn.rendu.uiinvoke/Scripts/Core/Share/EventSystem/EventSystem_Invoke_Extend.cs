namespace RD.Core
{
    public static class EventSystem_UIExtension
    {
        public static void UIInvoke<A>(this EventSystem self, long invokeType, A args) where A : struct
        {
            self?.Invoke(invokeType, args);
        }

        public static T UIInvoke<A, T>(this EventSystem self, long invokeType, A args) where A : struct
        {
            return self.Invoke<A, T>(invokeType, args);
        }

        public static void UIInvokeSync<A>(this EventSystem self, A args) where A : struct
        {
            self.UIInvoke(EUIInvokeType.Sync, args);
        }

        public static T UIInvokeSync<A, T>(this EventSystem self, A args) where A : struct
        {
            return self.UIInvoke<A, T>(EUIInvokeType.Sync, args);
        }

        public static void UIInvokeAsync<A>(this EventSystem self, A args) where A : struct
        {
            self.UIInvoke(EUIInvokeType.Async, args);
        }

        public static T UIInvokeAsync<A, T>(this EventSystem self, A args) where A : struct
        {
            return self.UIInvoke<A, T>(EUIInvokeType.Async, args);
        }
    }
}