using System.Collections.Generic;

namespace RD
{
    public static partial class Definition
    {
        public const string EntityType = "RD.Entity";

        public const string LSEntityType = "RD.LSEntity";

        public const string Promise = "Promise";

        public const string PromiseFullName = "RD.Promise";

        public static readonly string[] AddChildMethods = { "AddChild", "AddChildWithId" };

        public static readonly string[] ComponentMethod = {"AddComponent","GetComponent"};

        public const string ISystemType = "RD.ISystemType";
        
        public const string ObjectSystemAttribute = "RD.SystemAttribute";

        public const string EnableMethodAttribute = "RD.EnableMethodAttribute";
        
        public const string FriendOfAttribute = "RD.FriendOfAttribute";
        
        public const string UniqueIdAttribute = "RD.UniqueIdAttribute";

        public const string ChildOfAttribute = "RD.ChildOfAttribute";

        public const string ComponentOfAttribute = "RD.ComponentOfAttribute";
        
        public const string EnableAccessEntiyChildAttribute = "RD.EnableAccessEntiyChildAttribute";

        public const string StaticFieldAttribute = "RD.StaticFieldAttribute";

        public const string PromiseCancellationToken = "RD.PromiseCancellationToken";

        public const string PromiseCompleteTask = "Promise.CompletedTask";

        public const string RDClientNameSpace = "RD.Client";

        public const string ClientDirInServer = @"Unity\Assets\Scripts\Hotfix\Client\";

        public const string EntitySystemAttribute = "EntitySystem";
        public const string EntitySystemAttributeMetaName = "RD.EntitySystemAttribute";

        public const string EntitySystemOfAttribute = "RD.EntitySystemOfAttribute";
        public const string EntitySystemInterfaceSequence = "EntitySystemInterfaceSequence";

        public static string[] FriendAttributes =
        [
            FriendOfAttribute,
            EntitySystemOfAttribute,
            LSEntitySystemOfAttribute
        ];   

        public const string IAwakeInterface = "RD.IAwake";
        public const string AwakeMethod = "Awake";

        public const string IUpdateInterface = "RD.IUpdate";
        public const string UpdateMethod = "Update";

        public const string IDestroyInterface = "RD.IDestroy";
        public const string DestroyMethod = "Destroy";

        public const string IAddComponentInterface = "RD.IAddComponent";
        public const string AddComponentMethod = "AddComponent";

        public const string IDeserializeInterface = "RD.IDeserialize";
        public const string DeserializeMethod = "Deserialize";

        public const string IGetComponentInterface = "RD.IGetComponentSys";
        public const string GetComponentMethod = "GetComponentSys";

        public const string ILoadInterface = "RD.ILoad";
        public const string LoadMethod = "Load";

        public const string ILateUpdateInterface = "RD.ILateUpdate";
        public const string LateUpdateMethod = "LateUpdate";

        public const string ISerializeInterface = "RD.ISerialize";
        public const string SerializeMethod = "Serialize";

        public const string LSEntitySystemAttribute = "LSEntitySystem";
        public const string LSEntitySystemAttributeMetaName = "RD.LSEntitySystemAttribute";
        public const string LSEntitySystemOfAttribute = "RD.LSEntitySystemOfAttribute";

        public const string ILSRollbackInterface = "RD.ILSRollback";
        public const string LSRollbackMethod = "LSRollback";

        public const string ILSUpdateInterface = "RD.ILSUpdate";
        public const string LSUpdateMethod = "LSUpdate";

        public const string RDLog = "RD.Log";

        public const string IMessageInterface = "RD.IMessage";

        public const string EntityRefType = "EntityRef";
        public const string SystemType = "System.Type";
        
        public const string EntityWeakRefType = "EntityWeakRef";

        public const string DisableNewAttribute = "RD.DisableNewAttribute";

        public const string EnableClassAttribute = "RD.EnableClassAttribute";
        
        public const string IAwakeAsyncInterface      = "RD.IAwakeAsync";
        public const string AwakeAsyncMethod          = "AwakeAsync";
        public const string IDynamicEventInterface    = "RD.IDynamicEvent";
        public const string DynamicEventMethod        = "DynamicEvent";
    }
}

