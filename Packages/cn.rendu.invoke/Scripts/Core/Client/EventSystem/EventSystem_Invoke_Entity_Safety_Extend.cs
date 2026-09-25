namespace RD.Core
{
    public static class EventSystem_InvokeEntity_UISafetyExtension
    {
        public static void UIInvokeEntitySafety<A>(this EventSystem self, Entity entity, long invokeType, A args) where A : struct
        {
            if (self == null || entity == null || entity.IsDisposed) return;
            self.InvokeEntity(entity, invokeType, args);
        }

        public static T UIInvokeEntitySafety<A, T>(this EventSystem self, Entity entity, long invokeType, A args) where A : struct
        {
            if (self == null || entity == null || entity.IsDisposed) return default;
            return self.InvokeEntity<A, T>(entity, invokeType, args);
        }

        public static void UIInvokeEntitySyncSafety<A>(this EventSystem self, Entity entity, A args) where A : struct
        {
            if (self == null || entity == null || entity.IsDisposed) return;
            self.UIInvokeEntity(entity, EInvokeType.Sync, args);
        }

        public static T UIInvokeEntitySyncSafety<A, T>(this EventSystem self, Entity entity, A args) where A : struct
        {
            if (self == null || entity == null || entity.IsDisposed) return default;
            return self.UIInvokeEntity<A, T>(entity, EInvokeType.Sync, args);
        }

        public static void UIInvokeEntityAsyncSafety<A>(this EventSystem self, Entity entity, A args) where A : struct
        {
            if (self == null || entity == null || entity.IsDisposed) return;
            self.UIInvokeEntity(entity, EInvokeType.Async, args);
        }

        public static T UIInvokeEntityAsyncSafety<A, T>(this EventSystem self, Entity entity, A args) where A : struct
        {
            if (self == null || entity == null || entity.IsDisposed) return default;
            return self.UIInvokeEntity<A, T>(entity, EInvokeType.Async, args);
        }
    }
}