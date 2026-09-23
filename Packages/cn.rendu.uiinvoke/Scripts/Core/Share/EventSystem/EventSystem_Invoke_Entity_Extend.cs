namespace RD.Core
{
    public static class EventSystem_InvokeEntity_UIExtension
    {
        public static void UIInvokeEntity<A>(this EventSystem self, Entity entity, long invokeType, A args) where A : struct
        {
            self.InvokeEntity(entity, invokeType, args);
        }

        public static T UIInvokeEntity<A, T>(this EventSystem self, Entity entity, long invokeType, A args) where A : struct
        {
            return self.InvokeEntity<A, T>(entity, invokeType, args);
        }

        public static void UIInvokeEntitySync<A>(this EventSystem self, Entity entity, A args) where A : struct
        {
            self.UIInvokeEntity(entity, EUIInvokeType.Sync, args);
        }

        public static T UIInvokeEntitySync<A, T>(this EventSystem self, Entity entity, A args) where A : struct
        {
            return self.UIInvokeEntity<A, T>(entity, EUIInvokeType.Sync, args);
        }

        public static void UIInvokeEntityAsync<A>(this EventSystem self, Entity entity, A args) where A : struct
        {
            self.UIInvokeEntity(entity, EUIInvokeType.Async, args);
        }

        public static T UIInvokeEntityAsync<A, T>(this EventSystem self, Entity entity, A args) where A : struct
        {
            return self.UIInvokeEntity<A, T>(entity, EUIInvokeType.Async, args);
        }
    }
}