using System.Collections.Generic;
public class AOTGenericReferences : UnityEngine.MonoBehaviour
{

	// {{ AOT assemblies
	public static readonly IReadOnlyList<string> PatchedAOTAssemblyList = new List<string>
	{
		"CommandLine.dll",
		"MongoDB.Bson.dll",
		"RD.Core.dll",
		"RD.Model.dll",
		"RD.ModelView.dll",
		"RD.UI.dll",
		"RD.YooAsset.dll",
		"System.Core.dll",
		"UnityEngine.CoreModule.dll",
		"mscorlib.dll",
	};
	// }}

	// {{ constraint implement type
	// }} 

	// {{ AOT generic types
	// <>f__AnonymousType17<int,object>
	// <>f__AnonymousType1<object,object>
	// CSharpx.Just<byte>
	// CSharpx.Just<object>
	// CSharpx.Maybe<byte>
	// CSharpx.Maybe<object>
	// CSharpx.Nothing<object>
	// CommandLine.Core.InstanceBuilder.<>c__1<object>
	// CommandLine.Core.InstanceBuilder.<>c__DisplayClass1_0<object>
	// CommandLine.Core.InstanceBuilder.<>c__DisplayClass1_1<object>
	// CommandLine.Core.ReflectionExtensions.<>c__0<object>
	// CommandLine.Core.ReflectionExtensions.<>c__DisplayClass0_0<object>
	// CommandLine.NotParsed<object>
	// CommandLine.Parser.<>c__DisplayClass17_0<object>
	// CommandLine.ParserResult<object>
	// MongoDB.Bson.Serialization.IBsonSerializer<object>
	// MongoDB.Bson.Serialization.Serializers.SerializerBase<Unity.Mathematics.float2>
	// MongoDB.Bson.Serialization.Serializers.SerializerBase<Unity.Mathematics.float3>
	// MongoDB.Bson.Serialization.Serializers.SerializerBase<Unity.Mathematics.float4>
	// MongoDB.Bson.Serialization.Serializers.SerializerBase<Unity.Mathematics.quaternion>
	// MongoDB.Bson.Serialization.Serializers.StructSerializerBase<Unity.Mathematics.float2>
	// MongoDB.Bson.Serialization.Serializers.StructSerializerBase<Unity.Mathematics.float3>
	// MongoDB.Bson.Serialization.Serializers.StructSerializerBase<Unity.Mathematics.float4>
	// MongoDB.Bson.Serialization.Serializers.StructSerializerBase<Unity.Mathematics.quaternion>
	// RD.Client.IUIOpen<object,object,object,object,object>
	// RD.Client.IUIOpen<object,object,object,object>
	// RD.Client.IUIOpen<object,object,object>
	// RD.Client.IUIOpen<object,object>
	// RD.Client.IUIOpen<object>
	// RD.Client.UIBindSystem<object>
	// RD.Client.UIInitializeSystem<object>
	// RD.Client.UIOpenSystem<object>
	// RD.Client.UIWindowCloseSystem<object,byte>
	// RD.Core.AEvent<object,RD.AfterCreateClientScene>
	// RD.Core.AEvent<object,RD.AfterCreateCurrentScene>
	// RD.Core.AEvent<object,RD.AfterUnitCreate>
	// RD.Core.AEvent<object,RD.AppStartInitFinish>
	// RD.Core.AEvent<object,RD.ChangePosition>
	// RD.Core.AEvent<object,RD.ChangeRotation>
	// RD.Core.AEvent<object,RD.Client.LoginFinish>
	// RD.Core.AEvent<object,RD.Client.UIEventInitializeBefore>
	// RD.Core.AEvent<object,RD.Client.UIEventPanelCloseAfter>
	// RD.Core.AEvent<object,RD.Client.UIEventPanelCloseBefore>
	// RD.Core.AEvent<object,RD.Client.UIEventPanelDestroy>
	// RD.Core.AEvent<object,RD.Client.UIEventPanelOpenAfter>
	// RD.Core.AEvent<object,RD.Client.UIEventPanelOpenBefore>
	// RD.Core.AEvent<object,RD.Client.UIEvent_YooAssetsLoad_Completed>
	// RD.Core.AEvent<object,RD.EnterMapFinish>
	// RD.Core.AEvent<object,RD.InitClient>
	// RD.Core.AEvent<object,RD.InitServer>
	// RD.Core.AEvent<object,RD.InitShare>
	// RD.Core.AEvent<object,RD.MoveStart>
	// RD.Core.AEvent<object,RD.MoveStop>
	// RD.Core.AEvent<object,RD.NumericChange>
	// RD.Core.AEvent<object,RD.SceneChangeFinish>
	// RD.Core.AEvent<object,RD.SceneChangeStart>
	// RD.Core.AEvent<object,RD.Server.UnitEnterSightRange>
	// RD.Core.AEvent<object,RD.Server.UnitLeaveSightRange>
	// RD.Core.AInvokeEntityHandler<RD.Client.UIInvokeEntity_BanLayerOptionForever,long>
	// RD.Core.AInvokeEntityHandler<RD.Client.UIInvokeEntity_ClosePanel,object>
	// RD.Core.AInvokeEntityHandler<RD.Client.UIInvokeEntity_ClosePanel>
	// RD.Core.AInvokeEntityHandler<RD.Client.UIInvokeEntity_CoroutineLock,object>
	// RD.Core.AInvokeEntityHandler<RD.Client.UIInvokeEntity_CountDownAdd,byte>
	// RD.Core.AInvokeEntityHandler<RD.Client.UIInvokeEntity_CountDownAdd>
	// RD.Core.AInvokeEntityHandler<RD.Client.UIInvokeEntity_CountDownRemove,byte>
	// RD.Core.AInvokeEntityHandler<RD.Client.UIInvokeEntity_CountDownRemove>
	// RD.Core.AInvokeEntityHandler<RD.Client.UIInvokeEntity_DestroyPanel>
	// RD.Core.AInvokeEntityHandler<RD.Client.UIInvokeEntity_GetAssetInfo,object>
	// RD.Core.AInvokeEntityHandler<RD.Client.UIInvokeEntity_GetAssetInfoByGUID,object>
	// RD.Core.AInvokeEntityHandler<RD.Client.UIInvokeEntity_GetUICamera,object>
	// RD.Core.AInvokeEntityHandler<RD.Client.UIInvokeEntity_GetUICanvas,object>
	// RD.Core.AInvokeEntityHandler<RD.Client.UIInvokeEntity_GetUICanvasRoot,object>
	// RD.Core.AInvokeEntityHandler<RD.Client.UIInvokeEntity_GetUILayerRoot,object>
	// RD.Core.AInvokeEntityHandler<RD.Client.UIInvokeEntity_GetUIRoot,object>
	// RD.Core.AInvokeEntityHandler<RD.Client.UIInvokeEntity_HomePanel,object>
	// RD.Core.AInvokeEntityHandler<RD.Client.UIInvokeEntity_HomePanel>
	// RD.Core.AInvokeEntityHandler<RD.Client.UIInvokeEntity_InstantiateGameObject,object>
	// RD.Core.AInvokeEntityHandler<RD.Client.UIInvokeEntity_Load,object>
	// RD.Core.AInvokeEntityHandler<RD.Client.UIInvokeEntity_LoadInitialize,object>
	// RD.Core.AInvokeEntityHandler<RD.Client.UIInvokeEntity_LoadInstantiate,object>
	// RD.Core.AInvokeEntityHandler<RD.Client.UIInvokeEntity_LoadInstantiateByVo,object>
	// RD.Core.AInvokeEntityHandler<RD.Client.UIInvokeEntity_LoadSprite,object>
	// RD.Core.AInvokeEntityHandler<RD.Client.UIInvokeEntity_LoadTexture2D,object>
	// RD.Core.AInvokeEntityHandler<RD.Client.UIInvokeEntity_RecoverLayerOptionForever>
	// RD.Core.AInvokeEntityHandler<RD.Client.UIInvokeEntity_Release>
	// RD.Core.AInvokeEntityHandler<RD.Client.UIInvokeEntity_ReleaseInstantiate>
	// RD.Core.AInvokeEntityHandler<RD.Client.UIInvokeEntity_ReleaseSprite>
	// RD.Core.AInvokeEntityHandler<RD.Client.UIInvokeEntity_RemoveUIReset>
	// RD.Core.AInvokeEntityHandler<RD.Client.UIInvokeEntity_SceneOpenPanel,object>
	// RD.Core.AInvokeEntityHandler<RD.Client.UIInvokeEntity_SceneOpenPanel>
	// RD.Core.AInvokeEntityHandler<RD.Client.UIInvokeEntity_ViewClosePanel,object>
	// RD.Core.AInvokeEntityHandler<RD.Client.UIInvokeEntity_ViewClosePanel>
	// RD.Core.AInvokeEntityHandler<RD.Client.UIInvokeEntity_WaitAsync,object>
	// RD.Core.AInvokeEntityHandler<RD.Client.UIInvokeEntity_WaitFrameAsync,object>
	// RD.Core.AInvokeEntityHandler<RD.Client.UIInvokeEntity_WaitSecondAsync,object>
	// RD.Core.AInvokeEntityHandler<RD.Core.InvokeArgs,object>
	// RD.Core.AInvokeHandler<RD.ConfigLoader.GetAllConfigBytes,object>
	// RD.Core.AInvokeHandler<RD.ConfigLoader.GetOneConfigBytes,object>
	// RD.Core.AInvokeHandler<RD.Core.FiberInit,object>
	// RD.Core.AInvokeHandler<RD.MailBoxInvoker>
	// RD.Core.AInvokeHandler<RD.NetComponentOnRead>
	// RD.Core.AInvokeHandler<RD.Server.NavmeshComponent.RecastFileLoader,object>
	// RD.Core.AInvokeHandler<RD.TimerCallback>
	// RD.Core.AsyncPromiseMethodBuilder<RD.Client.Wait_CreateMyUnit>
	// RD.Core.AsyncPromiseMethodBuilder<RD.Client.Wait_SceneChangeFinish>
	// RD.Core.AsyncPromiseMethodBuilder<RD.Client.Wait_UnitStop>
	// RD.Core.AsyncPromiseMethodBuilder<RD.Core.ActorId>
	// RD.Core.AsyncPromiseMethodBuilder<System.ValueTuple<object,int>>
	// RD.Core.AsyncPromiseMethodBuilder<System.ValueTuple<uint,object>>
	// RD.Core.AsyncPromiseMethodBuilder<byte>
	// RD.Core.AsyncPromiseMethodBuilder<int>
	// RD.Core.AsyncPromiseMethodBuilder<long>
	// RD.Core.AsyncPromiseMethodBuilder<object>
	// RD.Core.AsyncPromiseMethodBuilder<uint>
	// RD.Core.AwakeSystem<object,RD.Client.UIBindVo,object>
	// RD.Core.AwakeSystem<object,RD.Core.ActorId,object>
	// RD.Core.AwakeSystem<object,int,Unity.Mathematics.float3>
	// RD.Core.AwakeSystem<object,int>
	// RD.Core.AwakeSystem<object,long,long,int>
	// RD.Core.AwakeSystem<object,long>
	// RD.Core.AwakeSystem<object,object,object>
	// RD.Core.AwakeSystem<object,object>
	// RD.Core.AwakeSystem<object>
	// RD.Core.EventSystem<object,RD.Core.LateUpdateEvent>
	// RD.Core.EventSystem<object,RD.Core.UpdateEvent>
	// RD.Core.DestroySystem<object>
	// RD.Core.EntityRef<object>
	// RD.Core.IAwakeSystem<RD.Client.UIBindVo,object>
	// RD.Core.IAwakeSystem<RD.Core.ActorId,object>
	// RD.Core.IAwakeSystem<int,Unity.Mathematics.float3>
	// RD.Core.IAwakeSystem<int>
	// RD.Core.IAwakeSystem<long,long,int>
	// RD.Core.IAwakeSystem<long>
	// RD.Core.IAwakeSystem<object,object>
	// RD.Core.IAwakeSystem<object>
	// RD.Core.LateUpdateSystem<object>
	// RD.Core.ListComponent<Unity.Mathematics.float3>
	// RD.Core.ListComponent<long>
	// RD.Core.ListComponent<object>
	// RD.Core.MultiMap<long,long>
	// RD.Core.Promise<RD.Client.Wait_CreateMyUnit>
	// RD.Core.Promise<RD.Client.Wait_SceneChangeFinish>
	// RD.Core.Promise<RD.Client.Wait_UnitStop>
	// RD.Core.Promise<RD.Core.ActorId>
	// RD.Core.Promise<System.ValueTuple<object,int>>
	// RD.Core.Promise<System.ValueTuple<uint,object>>
	// RD.Core.Promise<byte>
	// RD.Core.Promise<int>
	// RD.Core.Promise<long>
	// RD.Core.Promise<object>
	// RD.Core.Promise<uint>
	// RD.Core.Singleton<object>
	// RD.Core.SortedDictionary.<>c__DisplayClass34_0<long,object>
	// RD.Core.SortedDictionary.<>c__DisplayClass34_1<long,object>
	// RD.Core.SortedDictionary.Enumerator<long,object>
	// RD.Core.SortedDictionary.KeyCollection.<>c__DisplayClass5_0<long,object>
	// RD.Core.SortedDictionary.KeyCollection.<>c__DisplayClass6_0<long,object>
	// RD.Core.SortedDictionary.KeyCollection.Enumerator<long,object>
	// RD.Core.SortedDictionary.KeyCollection<long,object>
	// RD.Core.SortedDictionary.KeyValuePairComparer<long,object>
	// RD.Core.SortedDictionary.ValueCollection.<>c__DisplayClass5_0<long,object>
	// RD.Core.SortedDictionary.ValueCollection.<>c__DisplayClass6_0<long,object>
	// RD.Core.SortedDictionary.ValueCollection.Enumerator<long,object>
	// RD.Core.SortedDictionary.ValueCollection<long,object>
	// RD.Core.SortedDictionary<long,object>
	// RD.Core.SortedSet.<>c__DisplayClass43_0<System.Collections.Generic.KeyValuePair<long,object>>
	// RD.Core.SortedSet.<>c__DisplayClass44_0<System.Collections.Generic.KeyValuePair<long,object>>
	// RD.Core.SortedSet.<>c__DisplayClass76_0<System.Collections.Generic.KeyValuePair<long,object>>
	// RD.Core.SortedSet.<Reverse>d__85<System.Collections.Generic.KeyValuePair<long,object>>
	// RD.Core.SortedSet.Enumerator<System.Collections.Generic.KeyValuePair<long,object>>
	// RD.Core.SortedSet.Node<System.Collections.Generic.KeyValuePair<long,object>>
	// RD.Core.SortedSet.TreeSubSet.<>c__DisplayClass10_0<System.Collections.Generic.KeyValuePair<long,object>>
	// RD.Core.SortedSet.TreeSubSet<System.Collections.Generic.KeyValuePair<long,object>>
	// RD.Core.SortedSet<System.Collections.Generic.KeyValuePair<long,object>>
	// RD.Core.SortedSetEqualityComparer<System.Collections.Generic.KeyValuePair<long,object>>
	// RD.Core.StateMachineWrap<RD.Client.A2NetClient_MessageHandler.<Run>d__0>
	// RD.Core.StateMachineWrap<RD.Client.A2NetClient_RequestHandler.<Run>d__0>
	// RD.Core.StateMachineWrap<RD.Client.AI_Attack.<Execute>d__1>
	// RD.Core.StateMachineWrap<RD.Client.AI_Patrol.<Execute>d__1>
	// RD.Core.StateMachineWrap<RD.Client.AfterCreateClientScene_AddComponent.<Run>d__0>
	// RD.Core.StateMachineWrap<RD.Client.AfterCreateCurrentScene_AddComponent.<Run>d__0>
	// RD.Core.StateMachineWrap<RD.Client.AfterUnitCreate_CreateUnitView.<Run>d__0>
	// RD.Core.StateMachineWrap<RD.Client.AppStartInitFinish_CreateLoginUI.<Run>d__0>
	// RD.Core.StateMachineWrap<RD.Client.ChangePosition_SyncGameObjectPos.<Run>d__0>
	// RD.Core.StateMachineWrap<RD.Client.ChangeRotation_SyncGameObjectRotation.<Run>d__0>
	// RD.Core.StateMachineWrap<RD.Client.ClientInitEvent.<Run>d__0>
	// RD.Core.StateMachineWrap<RD.Client.ClientSenderComponentSystem.<Call>d__6>
	// RD.Core.StateMachineWrap<RD.Client.ClientSenderComponentSystem.<DisposeAsync>d__3>
	// RD.Core.StateMachineWrap<RD.Client.ClientSenderComponentSystem.<LoginAsync>d__4>
	// RD.Core.StateMachineWrap<RD.Client.ClientSenderComponentSystem.<RemoveFiberAsync>d__2>
	// RD.Core.StateMachineWrap<RD.Client.EnterMapHelper.<EnterMapAsync>d__0>
	// RD.Core.StateMachineWrap<RD.Client.GetAllConfigBytes.<Handle>d__0>
	// RD.Core.StateMachineWrap<RD.Client.GetOneConfigBytes.<Handle>d__0>
	// RD.Core.StateMachineWrap<RD.Client.LobbyPanelComponentSystem.<OnEventEnterMapInvoke>d__6>
	// RD.Core.StateMachineWrap<RD.Client.LobbyPanelComponentSystem.<UIOpen>d__5>
	// RD.Core.StateMachineWrap<RD.Client.LoginFinish_CreateLobbyUI.<Run>d__0>
	// RD.Core.StateMachineWrap<RD.Client.LoginFinish_RemoveLoginUI.<Run>d__0>
	// RD.Core.StateMachineWrap<RD.Client.LoginHelper.<Login>d__0>
	// RD.Core.StateMachineWrap<RD.Client.LoginPanelComponentSystem.<DoLogin>d__9>
	// RD.Core.StateMachineWrap<RD.Client.LoginPanelComponentSystem.<OnEventLoginInvoke>d__6>
	// RD.Core.StateMachineWrap<RD.Client.LoginPanelComponentSystem.<OnEventQQInvoke>d__8>
	// RD.Core.StateMachineWrap<RD.Client.LoginPanelComponentSystem.<OnEventWeChatInvoke>d__7>
	// RD.Core.StateMachineWrap<RD.Client.LoginPanelComponentSystem.<UIOpen>d__5>
	// RD.Core.StateMachineWrap<RD.Client.M2C_CreateMyUnitHandler.<Run>d__0>
	// RD.Core.StateMachineWrap<RD.Client.M2C_CreateUnitsHandler.<Run>d__0>
	// RD.Core.StateMachineWrap<RD.Client.M2C_PathfindingResultHandler.<Run>d__0>
	// RD.Core.StateMachineWrap<RD.Client.M2C_RemoveUnitsHandler.<Run>d__0>
	// RD.Core.StateMachineWrap<RD.Client.M2C_StartSceneChangeHandler.<Run>d__0>
	// RD.Core.StateMachineWrap<RD.Client.M2C_StopHandler.<Run>d__0>
	// RD.Core.StateMachineWrap<RD.Client.Main2NetClient_LoginHandler.<Run>d__0>
	// RD.Core.StateMachineWrap<RD.Client.MainPanelComponentSystem.<UIOpen>d__5>
	// RD.Core.StateMachineWrap<RD.Client.MoveHelper.<MoveToAsync>d__0>
	// RD.Core.StateMachineWrap<RD.Client.MoveHelper.<MoveToAsync>d__1>
	// RD.Core.StateMachineWrap<RD.Client.NetClient2Main_SessionDisposeHandler.<Run>d__0>
	// RD.Core.StateMachineWrap<RD.Client.NetClientFiberInit.<Handle>d__0>
	// RD.Core.StateMachineWrap<RD.Client.On_UIEventInitializeBefore_CountDownHandler.<Run>d__0>
	// RD.Core.StateMachineWrap<RD.Client.On_UIEvent_YooAssetsLoad_Completed_SpriteHandler.<Run>d__0>
	// RD.Core.StateMachineWrap<RD.Client.OperaComponentSystem.<Test1>d__3>
	// RD.Core.StateMachineWrap<RD.Client.OperaComponentSystem.<Test2>d__4>
	// RD.Core.StateMachineWrap<RD.Client.OperaComponentSystem.<TestCancelAfter>d__5>
	// RD.Core.StateMachineWrap<RD.Client.PingComponentSystem.<PingAsync>d__2>
	// RD.Core.StateMachineWrap<RD.Client.ResourcesLoaderComponentSystem.<LoadAllAssetsAsync>d__4<object>>
	// RD.Core.StateMachineWrap<RD.Client.ResourcesLoaderComponentSystem.<LoadAssetAsync>d__3<object>>
	// RD.Core.StateMachineWrap<RD.Client.ResourcesLoaderComponentSystem.<LoadSceneAsync>d__5>
	// RD.Core.StateMachineWrap<RD.Client.RouterAddressComponentSystem.<GetAllRouter>d__2>
	// RD.Core.StateMachineWrap<RD.Client.RouterAddressComponentSystem.<Init>d__1>
	// RD.Core.StateMachineWrap<RD.Client.RouterAddressComponentSystem.<WaitTenMinGetAllRouter>d__3>
	// RD.Core.StateMachineWrap<RD.Client.RouterCheckComponentSystem.<CheckAsync>d__1>
	// RD.Core.StateMachineWrap<RD.Client.RouterHelper.<Connect>d__2>
	// RD.Core.StateMachineWrap<RD.Client.RouterHelper.<CreateRouterSession>d__0>
	// RD.Core.StateMachineWrap<RD.Client.RouterHelper.<GetRouterAddress>d__1>
	// RD.Core.StateMachineWrap<RD.Client.SceneChangeFinishEvent_CreateUIHelp.<Run>d__0>
	// RD.Core.StateMachineWrap<RD.Client.SceneChangeHelper.<SceneChangeTo>d__0>
	// RD.Core.StateMachineWrap<RD.Client.SceneChangeStart_AddComponent.<Run>d__0>
	// RD.Core.StateMachineWrap<RD.Client.UIChildSystem.<>c__DisplayClass12_0.<<OpenAllChild>g__Open|0>d>
	// RD.Core.StateMachineWrap<RD.Client.UIChildSystem.<>c__DisplayClass14_0.<<CloseAllChild>g__Close|0>d>
	// RD.Core.StateMachineWrap<RD.Client.UIChildSystem.<CloseAllChild>d__14>
	// RD.Core.StateMachineWrap<RD.Client.UIChildSystem.<OpenAllChild>d__12>
	// RD.Core.StateMachineWrap<RD.Client.UICloseCommonComponentSystem.<CloseUI>d__5>
	// RD.Core.StateMachineWrap<RD.Client.UICloseCommonComponentSystem.<OnEventCloseInvoke>d__6>
	// RD.Core.StateMachineWrap<RD.Client.UIEventPanelCloseAfterHandler.<Run>d__0>
	// RD.Core.StateMachineWrap<RD.Client.UIEventPanelCloseBeforeHandler.<Run>d__0>
	// RD.Core.StateMachineWrap<RD.Client.UIEventPanelDestroyHandler.<Run>d__0>
	// RD.Core.StateMachineWrap<RD.Client.UIEventPanelOpenAfterHandler.<Run>d__0>
	// RD.Core.StateMachineWrap<RD.Client.UIEventPanelOpenBeforeHandler.<Run>d__0>
	// RD.Core.StateMachineWrap<RD.Client.UIFactory.<CreateAsync>d__13>
	// RD.Core.StateMachineWrap<RD.Client.UIFactory.<CreateGameObjectAsync>d__15>
	// RD.Core.StateMachineWrap<RD.Client.UIFactory.<CreatePanelAsync>d__12>
	// RD.Core.StateMachineWrap<RD.Client.UIFactory.<CreatePanelGameObjectAsync>d__14>
	// RD.Core.StateMachineWrap<RD.Client.UIFactory.<InstantiateAsync>d__10>
	// RD.Core.StateMachineWrap<RD.Client.UIFactory.<InstantiateAsync>d__11>
	// RD.Core.StateMachineWrap<RD.Client.UIFactory.<InstantiateAsync>d__6<object>>
	// RD.Core.StateMachineWrap<RD.Client.UIFactory.<InstantiateAsync>d__7<object>>
	// RD.Core.StateMachineWrap<RD.Client.UIFactory.<InstantiateAsync>d__8>
	// RD.Core.StateMachineWrap<RD.Client.UIFactory.<InstantiateAsync>d__9>
	// RD.Core.StateMachineWrap<RD.Client.UIFactory.<InstantiateGameObjectAsync>d__1>
	// RD.Core.StateMachineWrap<RD.Client.UIFactory.<InstantiateGameObjectAsync>d__27>
	// RD.Core.StateMachineWrap<RD.Client.UIInvokeClosePanelAsyncHandler.<Handle>d__0>
	// RD.Core.StateMachineWrap<RD.Client.UIInvokeCoroutineLockHandler.<Handle>d__0>
	// RD.Core.StateMachineWrap<RD.Client.UIInvokeHomePanelWaitHandler.<Handle>d__0>
	// RD.Core.StateMachineWrap<RD.Client.UIInvokeInstantiateGameObjectAsyncHandler.<Handle>d__0>
	// RD.Core.StateMachineWrap<RD.Client.UIInvokeLoadAsyncHandler.<Handle>d__0>
	// RD.Core.StateMachineWrap<RD.Client.UIInvokeLoadInstantiateAsyncHandler.<Handle>d__0>
	// RD.Core.StateMachineWrap<RD.Client.UIInvokeLoadInstantiateByVoAsyncHandler.<Handle>d__0>
	// RD.Core.StateMachineWrap<RD.Client.UIInvokeLoadSpriteAsyncHandler.<Handle>d__0>
	// RD.Core.StateMachineWrap<RD.Client.UIInvokeLoadTexture2DAsyncHandler.<Handle>d__0>
	// RD.Core.StateMachineWrap<RD.Client.UIInvokeRootOpenPanelAsyncHandler.<Handle>d__0>
	// RD.Core.StateMachineWrap<RD.Client.UIInvokeTimerComponentWaitAsyncHandler.<Handle>d__0>
	// RD.Core.StateMachineWrap<RD.Client.UIInvokeTimerComponentWaitFrameAsyncHandler.<Handle>d__0>
	// RD.Core.StateMachineWrap<RD.Client.UIInvokeTimerComponentWaitSecondAsyncHandler.<Handle>d__0>
	// RD.Core.StateMachineWrap<RD.Client.UIInvokeViewClosePanelAsyncHandler.<Handle>d__0>
	// RD.Core.StateMachineWrap<RD.Client.UIInvokeYooAssetsHandler.<Handle>d__0>
	// RD.Core.StateMachineWrap<RD.Client.UILoadComponentSystem.<Initialize>d__2>
	// RD.Core.StateMachineWrap<RD.Client.UILoadComponentSystem.<LoadAssetAsync>d__15>
	// RD.Core.StateMachineWrap<RD.Client.UILoadComponentSystem.<LoadAssetAsync>d__19<object>>
	// RD.Core.StateMachineWrap<RD.Client.UILoadComponentSystem.<LoadAssetAsync>d__23>
	// RD.Core.StateMachineWrap<RD.Client.UILoadComponentSystem.<LoadAssetAsync>d__3<object>>
	// RD.Core.StateMachineWrap<RD.Client.UILoadComponentSystem.<LoadAssetAsyncAction>d__17>
	// RD.Core.StateMachineWrap<RD.Client.UILoadComponentSystem.<LoadAssetAsyncAction>d__5<object>>
	// RD.Core.StateMachineWrap<RD.Client.UILoadComponentSystem.<LoadAssetAsyncInstantiate>d__7>
	// RD.Core.StateMachineWrap<RD.Client.UILoadComponentSystem.<LoadAssetAsyncInstantiate>d__8>
	// RD.Core.StateMachineWrap<RD.Client.UIMgrComponentSystem.<AddUICloseElse>d__12>
	// RD.Core.StateMachineWrap<RD.Client.UIMgrComponentSystem.<BanLayerOptionCountDown>d__17>
	// RD.Core.StateMachineWrap<RD.Client.UIMgrComponentSystem.<CloseAll>d__36>
	// RD.Core.StateMachineWrap<RD.Client.UIMgrComponentSystem.<CloseLayerTopPanelAsync>d__33>
	// RD.Core.StateMachineWrap<RD.Client.UIMgrComponentSystem.<ClosePanelAsync>d__25>
	// RD.Core.StateMachineWrap<RD.Client.UIMgrComponentSystem.<ClosePanelAsync>d__28<object>>
	// RD.Core.StateMachineWrap<RD.Client.UIMgrComponentSystem.<CloseTopPanelAsync>d__35>
	// RD.Core.StateMachineWrap<RD.Client.UIMgrComponentSystem.<HomePanel>d__30>
	// RD.Core.StateMachineWrap<RD.Client.UIMgrComponentSystem.<HomePanel>d__31<object>>
	// RD.Core.StateMachineWrap<RD.Client.UIMgrComponentSystem.<InitRoot>d__84>
	// RD.Core.StateMachineWrap<RD.Client.UIMgrComponentSystem.<Initialize>d__43>
	// RD.Core.StateMachineWrap<RD.Client.UIMgrComponentSystem.<OpenPanelAfter>d__49>
	// RD.Core.StateMachineWrap<RD.Client.UIMgrComponentSystem.<OpenPanelAsync>d__50<object>>
	// RD.Core.StateMachineWrap<RD.Client.UIMgrComponentSystem.<OpenPanelAsync>d__52<object,object>>
	// RD.Core.StateMachineWrap<RD.Client.UIMgrComponentSystem.<OpenPanelAsync>d__53<object,object,object>>
	// RD.Core.StateMachineWrap<RD.Client.UIMgrComponentSystem.<OpenPanelAsync>d__54<object,object,object,object>>
	// RD.Core.StateMachineWrap<RD.Client.UIMgrComponentSystem.<OpenPanelAsync>d__55<object,object,object,object,object>>
	// RD.Core.StateMachineWrap<RD.Client.UIMgrComponentSystem.<OpenPanelAsync>d__56<object,object,object,object,object,object>>
	// RD.Core.StateMachineWrap<RD.Client.UIMgrComponentSystem.<OpenPanelAsync>d__65>
	// RD.Core.StateMachineWrap<RD.Client.UIMgrComponentSystem.<OpenPanelAsync>d__67<object>>
	// RD.Core.StateMachineWrap<RD.Client.UIMgrComponentSystem.<OpenPanelAsync>d__68<object,object>>
	// RD.Core.StateMachineWrap<RD.Client.UIMgrComponentSystem.<OpenPanelAsync>d__69<object,object,object>>
	// RD.Core.StateMachineWrap<RD.Client.UIMgrComponentSystem.<OpenPanelAsync>d__70<object,object,object,object>>
	// RD.Core.StateMachineWrap<RD.Client.UIMgrComponentSystem.<OpenPanelAsync>d__71<object,object,object,object,object>>
	// RD.Core.StateMachineWrap<RD.Client.UIMgrComponentSystem.<OpenPanelBefore>d__48>
	// RD.Core.StateMachineWrap<RD.Client.UIMgrComponentSystem.<OpenPanelParamAsync>d__51<object>>
	// RD.Core.StateMachineWrap<RD.Client.UIMgrComponentSystem.<OpenPanelParamAsync>d__66>
	// RD.Core.StateMachineWrap<RD.Client.UIMgrComponentSystem.<OpenPanelStartAsync>d__47>
	// RD.Core.StateMachineWrap<RD.Client.UIMgrComponentSystem.<OpenPanelWaitAsync>d__58<object>>
	// RD.Core.StateMachineWrap<RD.Client.UIMgrComponentSystem.<OpenPanelWaitAsync>d__60<object,object>>
	// RD.Core.StateMachineWrap<RD.Client.UIMgrComponentSystem.<OpenPanelWaitAsync>d__61<object,object,object>>
	// RD.Core.StateMachineWrap<RD.Client.UIMgrComponentSystem.<OpenPanelWaitAsync>d__62<object,object,object,object>>
	// RD.Core.StateMachineWrap<RD.Client.UIMgrComponentSystem.<OpenPanelWaitAsync>d__63<object,object,object,object,object>>
	// RD.Core.StateMachineWrap<RD.Client.UIMgrComponentSystem.<OpenPanelWaitAsync>d__64<object,object,object,object,object,object>>
	// RD.Core.StateMachineWrap<RD.Client.UIMgrComponentSystem.<OpenPanelWaitAsync>d__72>
	// RD.Core.StateMachineWrap<RD.Client.UIMgrComponentSystem.<OpenPanelWaitAsync>d__74<object>>
	// RD.Core.StateMachineWrap<RD.Client.UIMgrComponentSystem.<OpenPanelWaitAsync>d__75<object,object>>
	// RD.Core.StateMachineWrap<RD.Client.UIMgrComponentSystem.<OpenPanelWaitAsync>d__76<object,object,object>>
	// RD.Core.StateMachineWrap<RD.Client.UIMgrComponentSystem.<OpenPanelWaitAsync>d__77<object,object,object,object>>
	// RD.Core.StateMachineWrap<RD.Client.UIMgrComponentSystem.<OpenPanelWaitAsync>d__78<object,object,object,object,object>>
	// RD.Core.StateMachineWrap<RD.Client.UIMgrComponentSystem.<OpenPanelWaitParamAsync>d__59<object>>
	// RD.Core.StateMachineWrap<RD.Client.UIMgrComponentSystem.<OpenPanelWaitParamAsync>d__73>
	// RD.Core.StateMachineWrap<RD.Client.UIMgrComponentSystem.<PanelWait>d__57>
	// RD.Core.StateMachineWrap<RD.Client.UIMgrComponentSystem.<PreLoadPanelAsync>d__79<object>>
	// RD.Core.StateMachineWrap<RD.Client.UIMgrComponentSystem.<PreLoadPanelAsync>d__80>
	// RD.Core.StateMachineWrap<RD.Client.UIMgrComponentSystem.<RemoveUIAddElse>d__13>
	// RD.Core.StateMachineWrap<RD.Client.UIMgrComponentSystem.<RemoveUIToHome>d__14>
	// RD.Core.StateMachineWrap<RD.Client.UIPanelComponentSystem.<CloseAllView>d__49>
	// RD.Core.StateMachineWrap<RD.Client.UIPanelComponentSystem.<CloseAllViewTween>d__50>
	// RD.Core.StateMachineWrap<RD.Client.UIPanelComponentSystem.<CloseAsync>d__4>
	// RD.Core.StateMachineWrap<RD.Client.UIPanelComponentSystem.<CloseLastView>d__48>
	// RD.Core.StateMachineWrap<RD.Client.UIPanelComponentSystem.<CloseViewAsync>d__10>
	// RD.Core.StateMachineWrap<RD.Client.UIPanelComponentSystem.<CloseViewAsync>d__11>
	// RD.Core.StateMachineWrap<RD.Client.UIPanelComponentSystem.<CloseViewAsync>d__9<object>>
	// RD.Core.StateMachineWrap<RD.Client.UIPanelComponentSystem.<DoCountDownDestroyPanel>d__36>
	// RD.Core.StateMachineWrap<RD.Client.UIPanelComponentSystem.<GetView>d__42>
	// RD.Core.StateMachineWrap<RD.Client.UIPanelComponentSystem.<GetView>d__43<object>>
	// RD.Core.StateMachineWrap<RD.Client.UIPanelComponentSystem.<Home>d__5<object>>
	// RD.Core.StateMachineWrap<RD.Client.UIPanelComponentSystem.<Home>d__6>
	// RD.Core.StateMachineWrap<RD.Client.UIPanelComponentSystem.<Open>d__27>
	// RD.Core.StateMachineWrap<RD.Client.UIPanelComponentSystem.<Open>d__28>
	// RD.Core.StateMachineWrap<RD.Client.UIPanelComponentSystem.<Open>d__29<object>>
	// RD.Core.StateMachineWrap<RD.Client.UIPanelComponentSystem.<Open>d__30<object,object>>
	// RD.Core.StateMachineWrap<RD.Client.UIPanelComponentSystem.<Open>d__31<object,object,object>>
	// RD.Core.StateMachineWrap<RD.Client.UIPanelComponentSystem.<Open>d__32<object,object,object,object>>
	// RD.Core.StateMachineWrap<RD.Client.UIPanelComponentSystem.<Open>d__33<object,object,object,object,object>>
	// RD.Core.StateMachineWrap<RD.Client.UIPanelComponentSystem.<OpenAllViewTween>d__51>
	// RD.Core.StateMachineWrap<RD.Client.UIPanelComponentSystem.<OpenViewAfter>d__47>
	// RD.Core.StateMachineWrap<RD.Client.UIPanelComponentSystem.<OpenViewAsync>d__12<object>>
	// RD.Core.StateMachineWrap<RD.Client.UIPanelComponentSystem.<OpenViewAsync>d__14<object,object>>
	// RD.Core.StateMachineWrap<RD.Client.UIPanelComponentSystem.<OpenViewAsync>d__15<object,object,object>>
	// RD.Core.StateMachineWrap<RD.Client.UIPanelComponentSystem.<OpenViewAsync>d__16<object,object,object,object>>
	// RD.Core.StateMachineWrap<RD.Client.UIPanelComponentSystem.<OpenViewAsync>d__17<object,object,object,object,object>>
	// RD.Core.StateMachineWrap<RD.Client.UIPanelComponentSystem.<OpenViewAsync>d__18<object,object,object,object,object,object>>
	// RD.Core.StateMachineWrap<RD.Client.UIPanelComponentSystem.<OpenViewAsync>d__19>
	// RD.Core.StateMachineWrap<RD.Client.UIPanelComponentSystem.<OpenViewAsync>d__21<object>>
	// RD.Core.StateMachineWrap<RD.Client.UIPanelComponentSystem.<OpenViewAsync>d__22<object,object>>
	// RD.Core.StateMachineWrap<RD.Client.UIPanelComponentSystem.<OpenViewAsync>d__23<object,object,object>>
	// RD.Core.StateMachineWrap<RD.Client.UIPanelComponentSystem.<OpenViewAsync>d__24<object,object,object,object>>
	// RD.Core.StateMachineWrap<RD.Client.UIPanelComponentSystem.<OpenViewAsync>d__25<object,object,object,object,object>>
	// RD.Core.StateMachineWrap<RD.Client.UIPanelComponentSystem.<OpenViewBefore>d__46>
	// RD.Core.StateMachineWrap<RD.Client.UIPanelComponentSystem.<OpenViewParamAsync>d__13<object>>
	// RD.Core.StateMachineWrap<RD.Client.UIPanelComponentSystem.<OpenViewParamAsync>d__20>
	// RD.Core.StateMachineWrap<RD.Client.UIPanelComponentSystem.<UseBaseOpen>d__26>
	// RD.Core.StateMachineWrap<RD.Client.UIRootComponentSystem.<OpenPanelAsync>d__15>
	// RD.Core.StateMachineWrap<RD.Client.UIRootComponentSystem.<OpenPanelAsync>d__17<object>>
	// RD.Core.StateMachineWrap<RD.Client.UIRootComponentSystem.<OpenPanelAsync>d__18<object,object>>
	// RD.Core.StateMachineWrap<RD.Client.UIRootComponentSystem.<OpenPanelAsync>d__19<object,object,object>>
	// RD.Core.StateMachineWrap<RD.Client.UIRootComponentSystem.<OpenPanelAsync>d__1<object>>
	// RD.Core.StateMachineWrap<RD.Client.UIRootComponentSystem.<OpenPanelAsync>d__20<object,object,object,object>>
	// RD.Core.StateMachineWrap<RD.Client.UIRootComponentSystem.<OpenPanelAsync>d__21<object,object,object,object,object>>
	// RD.Core.StateMachineWrap<RD.Client.UIRootComponentSystem.<OpenPanelAsync>d__3<object,object>>
	// RD.Core.StateMachineWrap<RD.Client.UIRootComponentSystem.<OpenPanelAsync>d__4<object,object,object>>
	// RD.Core.StateMachineWrap<RD.Client.UIRootComponentSystem.<OpenPanelAsync>d__5<object,object,object,object>>
	// RD.Core.StateMachineWrap<RD.Client.UIRootComponentSystem.<OpenPanelAsync>d__6<object,object,object,object,object>>
	// RD.Core.StateMachineWrap<RD.Client.UIRootComponentSystem.<OpenPanelAsync>d__7<object,object,object,object,object,object>>
	// RD.Core.StateMachineWrap<RD.Client.UIRootComponentSystem.<OpenPanelParamAsync>d__16>
	// RD.Core.StateMachineWrap<RD.Client.UIRootComponentSystem.<OpenPanelParamAsync>d__2<object>>
	// RD.Core.StateMachineWrap<RD.Client.UIRootComponentSystem.<OpenPanelWaitAsync>d__10<object,object>>
	// RD.Core.StateMachineWrap<RD.Client.UIRootComponentSystem.<OpenPanelWaitAsync>d__11<object,object,object>>
	// RD.Core.StateMachineWrap<RD.Client.UIRootComponentSystem.<OpenPanelWaitAsync>d__12<object,object,object,object>>
	// RD.Core.StateMachineWrap<RD.Client.UIRootComponentSystem.<OpenPanelWaitAsync>d__13<object,object,object,object,object>>
	// RD.Core.StateMachineWrap<RD.Client.UIRootComponentSystem.<OpenPanelWaitAsync>d__14<object,object,object,object,object,object>>
	// RD.Core.StateMachineWrap<RD.Client.UIRootComponentSystem.<OpenPanelWaitAsync>d__22>
	// RD.Core.StateMachineWrap<RD.Client.UIRootComponentSystem.<OpenPanelWaitAsync>d__24<object>>
	// RD.Core.StateMachineWrap<RD.Client.UIRootComponentSystem.<OpenPanelWaitAsync>d__25<object,object>>
	// RD.Core.StateMachineWrap<RD.Client.UIRootComponentSystem.<OpenPanelWaitAsync>d__26<object,object,object>>
	// RD.Core.StateMachineWrap<RD.Client.UIRootComponentSystem.<OpenPanelWaitAsync>d__27<object,object,object,object>>
	// RD.Core.StateMachineWrap<RD.Client.UIRootComponentSystem.<OpenPanelWaitAsync>d__28<object,object,object,object,object>>
	// RD.Core.StateMachineWrap<RD.Client.UIRootComponentSystem.<OpenPanelWaitAsync>d__8<object>>
	// RD.Core.StateMachineWrap<RD.Client.UIRootComponentSystem.<OpenPanelWaitParamAsync>d__23>
	// RD.Core.StateMachineWrap<RD.Client.UIRootComponentSystem.<OpenPanelWaitParamAsync>d__9<object>>
	// RD.Core.StateMachineWrap<RD.Client.UIRootComponentSystem.<PreLoadPanelAsync>d__29<object>>
	// RD.Core.StateMachineWrap<RD.Client.UIRootComponentSystem.<PreLoadPanelAsync>d__30>
	// RD.Core.StateMachineWrap<RD.Client.UIViewComponentSystem.<CloseAsync>d__4>
	// RD.Core.StateMachineWrap<RD.Client.UIViewComponentSystem.<ClosePanelAsync>d__6>
	// RD.Core.StateMachineWrap<RD.Client.UIViewComponentSystem.<Open>d__11>
	// RD.Core.StateMachineWrap<RD.Client.UIViewComponentSystem.<Open>d__12>
	// RD.Core.StateMachineWrap<RD.Client.UIViewComponentSystem.<Open>d__13<object>>
	// RD.Core.StateMachineWrap<RD.Client.UIViewComponentSystem.<Open>d__14<object,object>>
	// RD.Core.StateMachineWrap<RD.Client.UIViewComponentSystem.<Open>d__15<object,object,object>>
	// RD.Core.StateMachineWrap<RD.Client.UIViewComponentSystem.<Open>d__16<object,object,object,object>>
	// RD.Core.StateMachineWrap<RD.Client.UIViewComponentSystem.<Open>d__17<object,object,object,object,object>>
	// RD.Core.StateMachineWrap<RD.Client.UIViewComponentSystem.<UseBaseOpen>d__10>
	// RD.Core.StateMachineWrap<RD.Client.UIWaitComponentSystem.<UIWindowClose>d__2>
	// RD.Core.StateMachineWrap<RD.Client.UIWindowComponentSystem.<InternalOnWindowCloseTween>d__3>
	// RD.Core.StateMachineWrap<RD.Client.UIWindowComponentSystem.<InternalOnWindowOpenTween>d__2>
	// RD.Core.StateMachineWrap<RD.Client.UIWindowComponentSystem.<OnCloseTween>d__9>
	// RD.Core.StateMachineWrap<RD.Client.UIWindowComponentSystem.<OnOpenTween>d__8>
	// RD.Core.StateMachineWrap<RD.Client.UIWindowComponentSystem.<SealedOnWindowCloseTween>d__7>
	// RD.Core.StateMachineWrap<RD.Client.UIWindowComponentSystem.<SealedOnWindowOpenTween>d__6>
	// RD.Core.StateMachineWrap<RD.Client.UIYooAssetsLoadComponentSystem.<Initialize>d__2>
	// RD.Core.StateMachineWrap<RD.Client.UIYooAssetsLoadComponentSystem.<LoadAssetAsyncFunc>d__4>
	// RD.Core.StateMachineWrap<RD.Client.UIYooAssetsSpriteComponentSystem.<GetSpriteAsync>d__3>
	// RD.Core.StateMachineWrap<RD.Client.WindowFadeAnim.<GetAwaiter>d__0>
	// RD.Core.StateMachineWrap<RD.Client.WindowFadeAnim.<In>d__1>
	// RD.Core.StateMachineWrap<RD.Client.WindowFadeAnim.<Out>d__2>
	// RD.Core.StateMachineWrap<RD.CoroutineLockComponentSystem.<Wait>d__3>
	// RD.Core.StateMachineWrap<RD.CoroutineLockQueueSystem.<Wait>d__2>
	// RD.Core.StateMachineWrap<RD.CoroutineLockQueueTypeSystem.<Wait>d__4>
	// RD.Core.StateMachineWrap<RD.HashWaitSystem.<Notify>d__4>
	// RD.Core.StateMachineWrap<RD.HashWaitSystem.<Wait>d__2>
	// RD.Core.StateMachineWrap<RD.HashWaitSystem.<WaitObject>d__5>
	// RD.Core.StateMachineWrap<RD.MailBoxType_UnOrderedMessageHandler.<HandleAsync>d__1>
	// RD.Core.StateMachineWrap<RD.MessageHandler.<Handle>d__1<object,object,object>>
	// RD.Core.StateMachineWrap<RD.MessageHandler.<Handle>d__1<object,object>>
	// RD.Core.StateMachineWrap<RD.MessageSessionHandler.<HandleAsync>d__2<object,object>>
	// RD.Core.StateMachineWrap<RD.MessageSessionHandler.<HandleAsync>d__2<object>>
	// RD.Core.StateMachineWrap<RD.MoveComponentSystem.<MoveToAsync>d__5>
	// RD.Core.StateMachineWrap<RD.NumericChangeEvent_NotifyWatcher.<Run>d__0>
	// RD.Core.StateMachineWrap<RD.ObjectWaitSystem.<Wait>d__2<object>>
	// RD.Core.StateMachineWrap<RD.ProcessInnerSenderSystem.<>c__DisplayClass10_0.<<Call>g__Timeout|0>d>
	// RD.Core.StateMachineWrap<RD.ProcessInnerSenderSystem.<Call>d__10>
	// RD.Core.StateMachineWrap<RD.PromiseCancellationTokenHelper.<AddCancel>d__1>
	// RD.Core.StateMachineWrap<RD.PromiseCancellationTokenHelper.<AddCancel>d__2<object>>
	// RD.Core.StateMachineWrap<RD.PromiseCancellationTokenHelper.<TimeoutAsync>d__0>
	// RD.Core.StateMachineWrap<RD.PromiseCancellationTokenHelper.<TimeoutAsync>d__3>
	// RD.Core.StateMachineWrap<RD.PromiseCancellationTokenHelper.<TimeoutAsync>d__4<object>>
	// RD.Core.StateMachineWrap<RD.PromiseCancellationTokenHelper.<TimeoutAsync>d__5>
	// RD.Core.StateMachineWrap<RD.PromiseCancellationTokenHelper.<TimeoutAsync>d__6<object>>
	// RD.Core.StateMachineWrap<RD.PromiseHelperExtend.<WaitUntil>d__0>
	// RD.Core.StateMachineWrap<RD.PromiseHelperExtend.<WaitUntil>d__1>
	// RD.Core.StateMachineWrap<RD.Server.A2NetInner_MessageHandler.<Run>d__0>
	// RD.Core.StateMachineWrap<RD.Server.A2NetInner_RequestHandler.<Run>d__0>
	// RD.Core.StateMachineWrap<RD.Server.C2G_EnterMapHandler.<Run>d__0>
	// RD.Core.StateMachineWrap<RD.Server.C2G_LoginGateHandler.<Run>d__0>
	// RD.Core.StateMachineWrap<RD.Server.C2G_PingHandler.<Run>d__0>
	// RD.Core.StateMachineWrap<RD.Server.C2M_PathfindingResultHandler.<Run>d__0>
	// RD.Core.StateMachineWrap<RD.Server.C2M_StopHandler.<Run>d__0>
	// RD.Core.StateMachineWrap<RD.Server.C2M_TestRobotCaseHandler.<Run>d__0>
	// RD.Core.StateMachineWrap<RD.Server.C2M_TransferMapHandler.<Run>d__0>
	// RD.Core.StateMachineWrap<RD.Server.C2R_LoginHandler.<CloseSession>d__1>
	// RD.Core.StateMachineWrap<RD.Server.C2R_LoginHandler.<Run>d__0>
	// RD.Core.StateMachineWrap<RD.Server.ChangePosition_NotifyAOI.<Run>d__0>
	// RD.Core.StateMachineWrap<RD.Server.ConsoleComponentSystem.<Start>d__1>
	// RD.Core.StateMachineWrap<RD.Server.CreateRobotConsoleHandler.<Run>d__0>
	// RD.Core.StateMachineWrap<RD.Server.G2M_SessionDisconnectHandler.<Run>d__0>
	// RD.Core.StateMachineWrap<RD.Server.GateFiberInit.<Handle>d__0>
	// RD.Core.StateMachineWrap<RD.Server.GateMapFactory.<Create>d__0>
	// RD.Core.StateMachineWrap<RD.Server.GateSessionKeyComponentSystem.<TimeoutRemoveKey>d__3>
	// RD.Core.StateMachineWrap<RD.Server.HttpComponentSystem.<Accept>d__2>
	// RD.Core.StateMachineWrap<RD.Server.HttpComponentSystem.<Handle>d__3>
	// RD.Core.StateMachineWrap<RD.Server.HttpGetRouterHandler.<Handle>d__0>
	// RD.Core.StateMachineWrap<RD.Server.LocationFiberInit.<Handle>d__0>
	// RD.Core.StateMachineWrap<RD.Server.LocationOneTypeSystem.<>c__DisplayClass3_0.<<Lock>g__TimeWaitAsync|0>d>
	// RD.Core.StateMachineWrap<RD.Server.LocationOneTypeSystem.<Add>d__1>
	// RD.Core.StateMachineWrap<RD.Server.LocationOneTypeSystem.<Get>d__5>
	// RD.Core.StateMachineWrap<RD.Server.LocationOneTypeSystem.<Lock>d__3>
	// RD.Core.StateMachineWrap<RD.Server.LocationOneTypeSystem.<Remove>d__2>
	// RD.Core.StateMachineWrap<RD.Server.LocationProxyComponentSystem.<Add>d__1>
	// RD.Core.StateMachineWrap<RD.Server.LocationProxyComponentSystem.<AddLocation>d__6>
	// RD.Core.StateMachineWrap<RD.Server.LocationProxyComponentSystem.<Get>d__5>
	// RD.Core.StateMachineWrap<RD.Server.LocationProxyComponentSystem.<Lock>d__2>
	// RD.Core.StateMachineWrap<RD.Server.LocationProxyComponentSystem.<Remove>d__4>
	// RD.Core.StateMachineWrap<RD.Server.LocationProxyComponentSystem.<RemoveLocation>d__7>
	// RD.Core.StateMachineWrap<RD.Server.LocationProxyComponentSystem.<UnLock>d__3>
	// RD.Core.StateMachineWrap<RD.Server.M2M_UnitTransferRequestHandler.<Run>d__0>
	// RD.Core.StateMachineWrap<RD.Server.MailBoxType_OrderedMessageHandler.<HandleInner>d__1>
	// RD.Core.StateMachineWrap<RD.Server.MapFiberInit.<Handle>d__0>
	// RD.Core.StateMachineWrap<RD.Server.MessageLocationHandler.<Handle>d__1<object,object,object>>
	// RD.Core.StateMachineWrap<RD.Server.MessageLocationHandler.<Handle>d__1<object,object>>
	// RD.Core.StateMachineWrap<RD.Server.MessageLocationSenderComponentSystem.<Call>d__10>
	// RD.Core.StateMachineWrap<RD.Server.MessageLocationSenderComponentSystem.<Call>d__8>
	// RD.Core.StateMachineWrap<RD.Server.MessageLocationSenderComponentSystem.<CallInner>d__11>
	// RD.Core.StateMachineWrap<RD.Server.MessageLocationSenderComponentSystem.<SendInner>d__7>
	// RD.Core.StateMachineWrap<RD.Server.MessageSenderSystem.<Call>d__2>
	// RD.Core.StateMachineWrap<RD.Server.MoveHelper.<FindPathMoveToAsync>d__0>
	// RD.Core.StateMachineWrap<RD.Server.NetComponentOnReadInvoker_Gate.<HandleAsync>d__1>
	// RD.Core.StateMachineWrap<RD.Server.NetInnerFiberInit.<Handle>d__0>
	// RD.Core.StateMachineWrap<RD.Server.ObjectAddRequestHandler.<Run>d__0>
	// RD.Core.StateMachineWrap<RD.Server.ObjectGetRequestHandler.<Run>d__0>
	// RD.Core.StateMachineWrap<RD.Server.ObjectLockRequestHandler.<Run>d__0>
	// RD.Core.StateMachineWrap<RD.Server.ObjectRemoveRequestHandler.<Run>d__0>
	// RD.Core.StateMachineWrap<RD.Server.ObjectUnLockRequestHandler.<Run>d__0>
	// RD.Core.StateMachineWrap<RD.Server.ProcessOuterSenderSystem.<>c__DisplayClass13_0.<<Call>g__Timeout|0>d>
	// RD.Core.StateMachineWrap<RD.Server.ProcessOuterSenderSystem.<>c__DisplayClass3_0.<<OnRead>g__CallInner|0>d>
	// RD.Core.StateMachineWrap<RD.Server.ProcessOuterSenderSystem.<Call>d__13>
	// RD.Core.StateMachineWrap<RD.Server.ProfilerConsoleHandler.<Run>d__0>
	// RD.Core.StateMachineWrap<RD.Server.R2G_GetLoginKeyHandler.<Run>d__0>
	// RD.Core.StateMachineWrap<RD.Server.RealmFiberInit.<Handle>d__0>
	// RD.Core.StateMachineWrap<RD.Server.ReloadConfigConsoleHandler.<Run>d__0>
	// RD.Core.StateMachineWrap<RD.Server.RobotFiberInit.<Handle>d__0>
	// RD.Core.StateMachineWrap<RD.Server.RobotManagerComponentSystem.<<Destroy>g__Remove|1_0>d>
	// RD.Core.StateMachineWrap<RD.Server.RobotManagerComponentSystem.<NewRobot>d__2>
	// RD.Core.StateMachineWrap<RD.Server.RouterFiberInit.<Handle>d__0>
	// RD.Core.StateMachineWrap<RD.Server.RouterManagerFiberInit.<Handle>d__0>
	// RD.Core.StateMachineWrap<RD.Server.ServerInitEvent.<Run>d__0>
	// RD.Core.StateMachineWrap<RD.Server.TransferHelper.<Transfer>d__1>
	// RD.Core.StateMachineWrap<RD.Server.TransferHelper.<TransferAtFrameFinish>d__0>
	// RD.Core.StateMachineWrap<RD.Server.UnitEnterSightRange_NotifyClient.<Run>d__0>
	// RD.Core.StateMachineWrap<RD.Server.UnitLeaveSightRange_NotifyClient.<Run>d__0>
	// RD.Core.StateMachineWrap<RD.SessionSystem.<Call>d__3>
	// RD.Core.StateMachineWrap<RD.SessionSystem.<Call>d__4>
	// RD.Core.StateMachineWrap<RD.ShareInitEvent.<Run>d__0>
	// RD.Core.StateMachineWrap<RD.TimerComponentSystem.<WaitAsync>d__10>
	// RD.Core.StateMachineWrap<RD.TimerComponentSystem.<WaitFrameAsync>d__9>
	// RD.Core.StateMachineWrap<RD.TimerComponentSystem.<WaitTillAsync>d__8>
	// RD.Core.StructBsonSerialize<Unity.Mathematics.float2>
	// RD.Core.StructBsonSerialize<Unity.Mathematics.float3>
	// RD.Core.StructBsonSerialize<Unity.Mathematics.float4>
	// RD.Core.StructBsonSerialize<Unity.Mathematics.quaternion>
	// RD.Core.TreeSet<System.Collections.Generic.KeyValuePair<long,object>>
	// RD.Core.TreeWalkPredicate<System.Collections.Generic.KeyValuePair<long,object>>
	// RD.Core.UpdateSystem<object>
	// RD.ResultCallback<RD.Client.Wait_CreateMyUnit>
	// RD.ResultCallback<RD.Client.Wait_SceneChangeFinish>
	// RD.ResultCallback<RD.Client.Wait_UnitStop>
	// RD.ResultCallback<object>
	// System.Action<RD.Core.EntityRef<object>>
	// System.Action<RD.MessageInfo>
	// System.Action<System.Collections.Generic.KeyValuePair<long,object>>
	// System.Action<Unity.Mathematics.float3>
	// System.Action<byte>
	// System.Action<float>
	// System.Action<int>
	// System.Action<long,int>
	// System.Action<long,object>
	// System.Action<long>
	// System.Action<object,object>
	// System.Action<object>
	// System.ArraySegment.Enumerator<DotRecast.Detour.DtStraightPath>
	// System.ArraySegment.Enumerator<int>
	// System.ArraySegment.Enumerator<long>
	// System.ArraySegment<DotRecast.Detour.DtStraightPath>
	// System.ArraySegment<int>
	// System.ArraySegment<long>
	// System.ByReference<DotRecast.Detour.DtStraightPath>
	// System.ByReference<int>
	// System.ByReference<long>
	// System.Collections.Concurrent.ConcurrentQueue.<Enumerate>d__28<object>
	// System.Collections.Concurrent.ConcurrentQueue.Segment<object>
	// System.Collections.Concurrent.ConcurrentQueue<object>
	// System.Collections.Generic.ArraySortHelper<RD.Core.EntityRef<object>>
	// System.Collections.Generic.ArraySortHelper<RD.MessageInfo>
	// System.Collections.Generic.ArraySortHelper<System.Collections.Generic.KeyValuePair<long,object>>
	// System.Collections.Generic.ArraySortHelper<Unity.Mathematics.float3>
	// System.Collections.Generic.ArraySortHelper<long>
	// System.Collections.Generic.ArraySortHelper<object>
	// System.Collections.Generic.Comparer<RD.Core.ActorId>
	// System.Collections.Generic.Comparer<RD.Core.EntityRef<object>>
	// System.Collections.Generic.Comparer<RD.MessageInfo>
	// System.Collections.Generic.Comparer<System.Collections.Generic.KeyValuePair<long,object>>
	// System.Collections.Generic.Comparer<Unity.Mathematics.float3>
	// System.Collections.Generic.Comparer<byte>
	// System.Collections.Generic.Comparer<int>
	// System.Collections.Generic.Comparer<long>
	// System.Collections.Generic.Comparer<object>
	// System.Collections.Generic.Comparer<uint>
	// System.Collections.Generic.Comparer<ushort>
	// System.Collections.Generic.Dictionary.Enumerator<int,RD.MessageSenderStruct>
	// System.Collections.Generic.Dictionary.Enumerator<int,RD.RpcInfo>
	// System.Collections.Generic.Dictionary.Enumerator<int,long>
	// System.Collections.Generic.Dictionary.Enumerator<int,object>
	// System.Collections.Generic.Dictionary.Enumerator<long,RD.Core.ActorId>
	// System.Collections.Generic.Dictionary.Enumerator<long,RD.Core.EntityRef<object>>
	// System.Collections.Generic.Dictionary.Enumerator<long,RD.TimerAction>
	// System.Collections.Generic.Dictionary.Enumerator<long,object>
	// System.Collections.Generic.Dictionary.Enumerator<object,RD.Client.UIBindVo>
	// System.Collections.Generic.Dictionary.Enumerator<object,RD.Core.EntityRef<object>>
	// System.Collections.Generic.Dictionary.Enumerator<object,long>
	// System.Collections.Generic.Dictionary.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<int,RD.MessageSenderStruct>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<int,RD.RpcInfo>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<int,long>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<int,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<long,RD.Core.ActorId>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<long,RD.Core.EntityRef<object>>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<long,RD.TimerAction>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<long,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,RD.Client.UIBindVo>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,RD.Core.EntityRef<object>>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,long>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.KeyCollection<int,RD.MessageSenderStruct>
	// System.Collections.Generic.Dictionary.KeyCollection<int,RD.RpcInfo>
	// System.Collections.Generic.Dictionary.KeyCollection<int,long>
	// System.Collections.Generic.Dictionary.KeyCollection<int,object>
	// System.Collections.Generic.Dictionary.KeyCollection<long,RD.Core.ActorId>
	// System.Collections.Generic.Dictionary.KeyCollection<long,RD.Core.EntityRef<object>>
	// System.Collections.Generic.Dictionary.KeyCollection<long,RD.TimerAction>
	// System.Collections.Generic.Dictionary.KeyCollection<long,object>
	// System.Collections.Generic.Dictionary.KeyCollection<object,RD.Client.UIBindVo>
	// System.Collections.Generic.Dictionary.KeyCollection<object,RD.Core.EntityRef<object>>
	// System.Collections.Generic.Dictionary.KeyCollection<object,long>
	// System.Collections.Generic.Dictionary.KeyCollection<object,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<int,RD.MessageSenderStruct>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<int,RD.RpcInfo>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<int,long>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<int,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<long,RD.Core.ActorId>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<long,RD.Core.EntityRef<object>>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<long,RD.TimerAction>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<long,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,RD.Client.UIBindVo>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,RD.Core.EntityRef<object>>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,long>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.ValueCollection<int,RD.MessageSenderStruct>
	// System.Collections.Generic.Dictionary.ValueCollection<int,RD.RpcInfo>
	// System.Collections.Generic.Dictionary.ValueCollection<int,long>
	// System.Collections.Generic.Dictionary.ValueCollection<int,object>
	// System.Collections.Generic.Dictionary.ValueCollection<long,RD.Core.ActorId>
	// System.Collections.Generic.Dictionary.ValueCollection<long,RD.Core.EntityRef<object>>
	// System.Collections.Generic.Dictionary.ValueCollection<long,RD.TimerAction>
	// System.Collections.Generic.Dictionary.ValueCollection<long,object>
	// System.Collections.Generic.Dictionary.ValueCollection<object,RD.Client.UIBindVo>
	// System.Collections.Generic.Dictionary.ValueCollection<object,RD.Core.EntityRef<object>>
	// System.Collections.Generic.Dictionary.ValueCollection<object,long>
	// System.Collections.Generic.Dictionary.ValueCollection<object,object>
	// System.Collections.Generic.Dictionary<int,RD.MessageSenderStruct>
	// System.Collections.Generic.Dictionary<int,RD.RpcInfo>
	// System.Collections.Generic.Dictionary<int,long>
	// System.Collections.Generic.Dictionary<int,object>
	// System.Collections.Generic.Dictionary<long,RD.Core.ActorId>
	// System.Collections.Generic.Dictionary<long,RD.Core.EntityRef<object>>
	// System.Collections.Generic.Dictionary<long,RD.TimerAction>
	// System.Collections.Generic.Dictionary<long,object>
	// System.Collections.Generic.Dictionary<object,RD.Client.UIBindVo>
	// System.Collections.Generic.Dictionary<object,RD.Core.EntityRef<object>>
	// System.Collections.Generic.Dictionary<object,long>
	// System.Collections.Generic.Dictionary<object,object>
	// System.Collections.Generic.EqualityComparer<RD.Client.UIBindVo>
	// System.Collections.Generic.EqualityComparer<RD.Core.ActorId>
	// System.Collections.Generic.EqualityComparer<RD.Core.EntityRef<object>>
	// System.Collections.Generic.EqualityComparer<RD.MessageSenderStruct>
	// System.Collections.Generic.EqualityComparer<RD.RpcInfo>
	// System.Collections.Generic.EqualityComparer<RD.TimerAction>
	// System.Collections.Generic.EqualityComparer<System.Collections.Generic.KeyValuePair<long,object>>
	// System.Collections.Generic.EqualityComparer<byte>
	// System.Collections.Generic.EqualityComparer<int>
	// System.Collections.Generic.EqualityComparer<long>
	// System.Collections.Generic.EqualityComparer<object>
	// System.Collections.Generic.EqualityComparer<uint>
	// System.Collections.Generic.EqualityComparer<ushort>
	// System.Collections.Generic.HashSet.Enumerator<int>
	// System.Collections.Generic.HashSet.Enumerator<long>
	// System.Collections.Generic.HashSet.Enumerator<object>
	// System.Collections.Generic.HashSet<int>
	// System.Collections.Generic.HashSet<long>
	// System.Collections.Generic.HashSet<object>
	// System.Collections.Generic.HashSetEqualityComparer<int>
	// System.Collections.Generic.HashSetEqualityComparer<long>
	// System.Collections.Generic.HashSetEqualityComparer<object>
	// System.Collections.Generic.ICollection<RD.Core.EntityRef<object>>
	// System.Collections.Generic.ICollection<RD.MessageInfo>
	// System.Collections.Generic.ICollection<RD.RpcInfo>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<int,RD.MessageSenderStruct>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<int,RD.RpcInfo>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<int,long>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<long,RD.Core.ActorId>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<long,RD.Core.EntityRef<object>>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<long,RD.TimerAction>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<long,object>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,RD.Client.UIBindVo>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,RD.Core.EntityRef<object>>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,long>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.ICollection<Unity.Mathematics.float3>
	// System.Collections.Generic.ICollection<int>
	// System.Collections.Generic.ICollection<long>
	// System.Collections.Generic.ICollection<object>
	// System.Collections.Generic.IComparer<RD.Core.EntityRef<object>>
	// System.Collections.Generic.IComparer<RD.MessageInfo>
	// System.Collections.Generic.IComparer<System.Collections.Generic.KeyValuePair<long,object>>
	// System.Collections.Generic.IComparer<Unity.Mathematics.float3>
	// System.Collections.Generic.IComparer<long>
	// System.Collections.Generic.IComparer<object>
	// System.Collections.Generic.IEnumerable<RD.Core.EntityRef<object>>
	// System.Collections.Generic.IEnumerable<RD.MessageInfo>
	// System.Collections.Generic.IEnumerable<RD.RpcInfo>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,RD.MessageSenderStruct>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,RD.RpcInfo>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,long>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<long,RD.Core.ActorId>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<long,RD.Core.EntityRef<object>>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<long,RD.TimerAction>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<long,object>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,RD.Client.UIBindVo>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,RD.Core.EntityRef<object>>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,long>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.IEnumerable<Unity.Mathematics.float3>
	// System.Collections.Generic.IEnumerable<int>
	// System.Collections.Generic.IEnumerable<long>
	// System.Collections.Generic.IEnumerable<object>
	// System.Collections.Generic.IEnumerator<RD.Core.EntityRef<object>>
	// System.Collections.Generic.IEnumerator<RD.MessageInfo>
	// System.Collections.Generic.IEnumerator<RD.RpcInfo>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int,RD.MessageSenderStruct>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int,RD.RpcInfo>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int,long>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<long,RD.Core.ActorId>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<long,RD.Core.EntityRef<object>>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<long,RD.TimerAction>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<long,object>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,RD.Client.UIBindVo>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,RD.Core.EntityRef<object>>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,long>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.IEnumerator<Unity.Mathematics.float3>
	// System.Collections.Generic.IEnumerator<int>
	// System.Collections.Generic.IEnumerator<long>
	// System.Collections.Generic.IEnumerator<object>
	// System.Collections.Generic.IEqualityComparer<System.Collections.Generic.KeyValuePair<long,object>>
	// System.Collections.Generic.IEqualityComparer<int>
	// System.Collections.Generic.IEqualityComparer<long>
	// System.Collections.Generic.IEqualityComparer<object>
	// System.Collections.Generic.IList<RD.Core.EntityRef<object>>
	// System.Collections.Generic.IList<RD.MessageInfo>
	// System.Collections.Generic.IList<System.Collections.Generic.KeyValuePair<long,object>>
	// System.Collections.Generic.IList<Unity.Mathematics.float3>
	// System.Collections.Generic.IList<long>
	// System.Collections.Generic.IList<object>
	// System.Collections.Generic.IReadOnlyDictionary<object,object>
	// System.Collections.Generic.KeyValuePair<int,RD.MessageSenderStruct>
	// System.Collections.Generic.KeyValuePair<int,RD.RpcInfo>
	// System.Collections.Generic.KeyValuePair<int,long>
	// System.Collections.Generic.KeyValuePair<int,object>
	// System.Collections.Generic.KeyValuePair<long,RD.Core.ActorId>
	// System.Collections.Generic.KeyValuePair<long,RD.Core.EntityRef<object>>
	// System.Collections.Generic.KeyValuePair<long,RD.TimerAction>
	// System.Collections.Generic.KeyValuePair<long,object>
	// System.Collections.Generic.KeyValuePair<object,RD.Client.UIBindVo>
	// System.Collections.Generic.KeyValuePair<object,RD.Core.EntityRef<object>>
	// System.Collections.Generic.KeyValuePair<object,long>
	// System.Collections.Generic.KeyValuePair<object,object>
	// System.Collections.Generic.List.Enumerator<RD.Core.EntityRef<object>>
	// System.Collections.Generic.List.Enumerator<RD.MessageInfo>
	// System.Collections.Generic.List.Enumerator<System.Collections.Generic.KeyValuePair<long,object>>
	// System.Collections.Generic.List.Enumerator<Unity.Mathematics.float3>
	// System.Collections.Generic.List.Enumerator<long>
	// System.Collections.Generic.List.Enumerator<object>
	// System.Collections.Generic.List<RD.Core.EntityRef<object>>
	// System.Collections.Generic.List<RD.MessageInfo>
	// System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<long,object>>
	// System.Collections.Generic.List<Unity.Mathematics.float3>
	// System.Collections.Generic.List<long>
	// System.Collections.Generic.List<object>
	// System.Collections.Generic.ObjectComparer<RD.Core.ActorId>
	// System.Collections.Generic.ObjectComparer<RD.Core.EntityRef<object>>
	// System.Collections.Generic.ObjectComparer<RD.MessageInfo>
	// System.Collections.Generic.ObjectComparer<System.Collections.Generic.KeyValuePair<long,object>>
	// System.Collections.Generic.ObjectComparer<Unity.Mathematics.float3>
	// System.Collections.Generic.ObjectComparer<byte>
	// System.Collections.Generic.ObjectComparer<int>
	// System.Collections.Generic.ObjectComparer<long>
	// System.Collections.Generic.ObjectComparer<object>
	// System.Collections.Generic.ObjectComparer<uint>
	// System.Collections.Generic.ObjectComparer<ushort>
	// System.Collections.Generic.ObjectEqualityComparer<RD.Client.UIBindVo>
	// System.Collections.Generic.ObjectEqualityComparer<RD.Core.ActorId>
	// System.Collections.Generic.ObjectEqualityComparer<RD.Core.EntityRef<object>>
	// System.Collections.Generic.ObjectEqualityComparer<RD.MessageSenderStruct>
	// System.Collections.Generic.ObjectEqualityComparer<RD.RpcInfo>
	// System.Collections.Generic.ObjectEqualityComparer<RD.TimerAction>
	// System.Collections.Generic.ObjectEqualityComparer<System.Collections.Generic.KeyValuePair<long,object>>
	// System.Collections.Generic.ObjectEqualityComparer<byte>
	// System.Collections.Generic.ObjectEqualityComparer<int>
	// System.Collections.Generic.ObjectEqualityComparer<long>
	// System.Collections.Generic.ObjectEqualityComparer<object>
	// System.Collections.Generic.ObjectEqualityComparer<uint>
	// System.Collections.Generic.ObjectEqualityComparer<ushort>
	// System.Collections.Generic.Queue.Enumerator<System.ValueTuple<long,long,int>>
	// System.Collections.Generic.Queue.Enumerator<long>
	// System.Collections.Generic.Queue.Enumerator<object>
	// System.Collections.Generic.Queue.Enumerator<uint>
	// System.Collections.Generic.Queue<System.ValueTuple<long,long,int>>
	// System.Collections.Generic.Queue<long>
	// System.Collections.Generic.Queue<object>
	// System.Collections.Generic.Queue<uint>
	// System.Collections.Generic.Stack.Enumerator<System.ValueTuple<object,object>>
	// System.Collections.Generic.Stack.Enumerator<object>
	// System.Collections.Generic.Stack<System.ValueTuple<object,object>>
	// System.Collections.Generic.Stack<object>
	// System.Collections.ObjectModel.ReadOnlyCollection<RD.Core.EntityRef<object>>
	// System.Collections.ObjectModel.ReadOnlyCollection<RD.MessageInfo>
	// System.Collections.ObjectModel.ReadOnlyCollection<System.Collections.Generic.KeyValuePair<long,object>>
	// System.Collections.ObjectModel.ReadOnlyCollection<Unity.Mathematics.float3>
	// System.Collections.ObjectModel.ReadOnlyCollection<long>
	// System.Collections.ObjectModel.ReadOnlyCollection<object>
	// System.Comparison<RD.Core.EntityRef<object>>
	// System.Comparison<RD.MessageInfo>
	// System.Comparison<System.Collections.Generic.KeyValuePair<long,object>>
	// System.Comparison<Unity.Mathematics.float3>
	// System.Comparison<long>
	// System.Comparison<object>
	// System.Func<byte>
	// System.Func<object,byte>
	// System.Func<object,int>
	// System.Func<object,object,byte,byte,object>
	// System.Func<object,object,byte,object>
	// System.Func<object,object,byte>
	// System.Func<object,object,object,System.ValueTuple<object,int>>
	// System.Func<object,object,object,object>
	// System.Func<object,object,object>
	// System.Func<object,object>
	// System.Func<object>
	// System.Linq.Buffer<RD.RpcInfo>
	// System.Linq.Buffer<long>
	// System.Linq.Buffer<object>
	// System.Linq.Enumerable.<OfTypeIterator>d__97<object>
	// System.Linq.Enumerable.<SelectManyIterator>d__17<object,object>
	// System.Linq.Enumerable.Iterator<object>
	// System.Linq.Enumerable.WhereArrayIterator<object>
	// System.Linq.Enumerable.WhereEnumerableIterator<object>
	// System.Linq.Enumerable.WhereListIterator<object>
	// System.Linq.Enumerable.WhereSelectArrayIterator<object,object>
	// System.Linq.Enumerable.WhereSelectEnumerableIterator<object,object>
	// System.Linq.Enumerable.WhereSelectListIterator<object,object>
	// System.Linq.GroupedEnumerable<object,object,object>
	// System.Linq.Lookup.<GetEnumerator>d__12<object,object>
	// System.Linq.Lookup.Grouping.<GetEnumerator>d__7<object,object>
	// System.Linq.Lookup.Grouping<object,object>
	// System.Linq.Lookup<object,object>
	// System.Nullable<RD.Client.UIBindVo>
	// System.Nullable<byte>
	// System.Predicate<RD.Core.EntityRef<object>>
	// System.Predicate<RD.MessageInfo>
	// System.Predicate<System.Collections.Generic.KeyValuePair<long,object>>
	// System.Predicate<Unity.Mathematics.float3>
	// System.Predicate<int>
	// System.Predicate<long>
	// System.Predicate<object>
	// System.ReadOnlySpan<DotRecast.Detour.DtStraightPath>
	// System.ReadOnlySpan<int>
	// System.ReadOnlySpan<long>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<object>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<object>
	// System.Runtime.CompilerServices.TaskAwaiter<object>
	// System.Span.Enumerator<DotRecast.Detour.DtStraightPath>
	// System.Span.Enumerator<int>
	// System.Span.Enumerator<long>
	// System.Span<DotRecast.Detour.DtStraightPath>
	// System.Span<int>
	// System.Span<long>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<object>
	// System.Threading.Tasks.Task<object>
	// System.Threading.Tasks.TaskFactory.<>c<object>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass32_0<object>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<object>
	// System.Threading.Tasks.TaskFactory<object>
	// System.Tuple<object,object,object>
	// System.Tuple<object,object>
	// System.ValueTuple<RD.Core.ActorId,object>
	// System.ValueTuple<byte,object>
	// System.ValueTuple<long,long,int>
	// System.ValueTuple<object,int>
	// System.ValueTuple<object,object>
	// System.ValueTuple<uint,object>
	// System.ValueTuple<uint,uint>
	// System.ValueTuple<ushort,object>
	// }}

	public void RefMethods()
	{
		// System.Collections.Generic.IEnumerable<object> CSharpx.EnumerableExtensions.Memoize<object>(System.Collections.Generic.IEnumerable<object>)
		// CSharpx.Just<object> CSharpx.Maybe.Just<object>(object)
		// CSharpx.Maybe<object> CSharpx.Maybe.Nothing<object>()
		// object CSharpx.MaybeExtensions.MapValueOrDefault<object,object>(CSharpx.Maybe<object>,System.Func<object,object>,object)
		// CommandLine.ParserResult<object> CommandLine.Core.InstanceBuilder.Build<object>(CSharpx.Maybe<System.Func<object>>,System.Func<System.Collections.Generic.IEnumerable<string>,System.Collections.Generic.IEnumerable<CommandLine.Core.OptionSpecification>,RailwaySharp.ErrorHandling.Result<System.Collections.Generic.IEnumerable<CommandLine.Core.Token>,CommandLine.Error>>,System.Collections.Generic.IEnumerable<string>,System.StringComparer,bool,System.Globalization.CultureInfo,bool,bool,bool,System.Collections.Generic.IEnumerable<CommandLine.ErrorType>)
		// System.Collections.Generic.IEnumerable<object> CommandLine.Core.ReflectionExtensions.GetSpecifications<object>(System.Type,System.Func<System.Reflection.PropertyInfo,object>)
		// RailwaySharp.ErrorHandling.Result<System.Collections.Generic.IEnumerable<CommandLine.Core.Token>,CommandLine.Error> CommandLine.Parser.<ParseArguments>b__11_0<object>(System.Collections.Generic.IEnumerable<string>,System.Collections.Generic.IEnumerable<CommandLine.Core.OptionSpecification>)
		// CommandLine.ParserResult<object> CommandLine.Parser.DisplayHelp<object>(CommandLine.ParserResult<object>,System.IO.TextWriter,int)
		// CommandLine.ParserResult<object> CommandLine.Parser.MakeParserResult<object>(CommandLine.ParserResult<object>,CommandLine.ParserSettings)
		// CommandLine.ParserResult<object> CommandLine.Parser.ParseArguments<object>(System.Collections.Generic.IEnumerable<string>)
		// CommandLine.ParserResult<object> CommandLine.ParserResultExtensions.WithNotParsed<object>(CommandLine.ParserResult<object>,System.Action<System.Collections.Generic.IEnumerable<CommandLine.Error>>)
		// CommandLine.ParserResult<object> CommandLine.ParserResultExtensions.WithParsed<object>(CommandLine.ParserResult<object>,System.Action<object>)
		// object MongoDB.Bson.Serialization.BsonSerializer.Deserialize<object>(MongoDB.Bson.IO.IBsonReader,System.Action<MongoDB.Bson.Serialization.BsonDeserializationContext.Builder>)
		// object MongoDB.Bson.Serialization.BsonSerializer.Deserialize<object>(string,System.Action<MongoDB.Bson.Serialization.BsonDeserializationContext.Builder>)
		// MongoDB.Bson.Serialization.IBsonSerializer<object> MongoDB.Bson.Serialization.BsonSerializer.LookupSerializer<object>()
		// object MongoDB.Bson.Serialization.IBsonSerializerExtensions.Deserialize<object>(MongoDB.Bson.Serialization.IBsonSerializer<object>,MongoDB.Bson.Serialization.BsonDeserializationContext)
		// object RD.Client.UIBindComponentTable.FindComponent<object>(string)
		// object RD.Client.UIBindEventTable.FindEvent<object>(string)
		// RD.Core.Promise RD.Client.UIEventComponent.Run<RD.Client.UIEventPanelCloseAfter>(RD.Core.Scene,string,RD.Client.UIEventPanelCloseAfter)
		// RD.Core.Promise RD.Client.UIEventComponent.Run<RD.Client.UIEventPanelCloseBefore>(RD.Core.Scene,string,RD.Client.UIEventPanelCloseBefore)
		// RD.Core.Promise RD.Client.UIEventComponent.Run<RD.Client.UIEventPanelDestroy>(RD.Core.Scene,string,RD.Client.UIEventPanelDestroy)
		// RD.Core.Promise RD.Client.UIEventComponent.Run<RD.Client.UIEventPanelOpenAfter>(RD.Core.Scene,string,RD.Client.UIEventPanelOpenAfter)
		// RD.Core.Promise RD.Client.UIEventComponent.Run<RD.Client.UIEventPanelOpenBefore>(RD.Core.Scene,string,RD.Client.UIEventPanelOpenBefore)
		// RD.Core.Promise<bool> RD.Client.UIEventSystem.Open<object,object,object,object,object>(RD.Core.Entity,object,object,object,object,object)
		// RD.Core.Promise<bool> RD.Client.UIEventSystem.Open<object,object,object,object>(RD.Core.Entity,object,object,object,object)
		// RD.Core.Promise<bool> RD.Client.UIEventSystem.Open<object,object,object>(RD.Core.Entity,object,object,object)
		// RD.Core.Promise<bool> RD.Client.UIEventSystem.Open<object,object>(RD.Core.Entity,object,object)
		// RD.Core.Promise<bool> RD.Client.UIEventSystem.Open<object>(RD.Core.Entity,object)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,RD.Server.HttpGetRouterHandler.<Handle>d__0>(System.Runtime.CompilerServices.TaskAwaiter&,RD.Server.HttpGetRouterHandler.<Handle>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,RD.Server.ConsoleComponentSystem.<Start>d__1>(System.Runtime.CompilerServices.TaskAwaiter<object>&,RD.Server.ConsoleComponentSystem.<Start>d__1&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,RD.Server.HttpComponentSystem.<Accept>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,RD.Server.HttpComponentSystem.<Accept>d__2&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<YooAsset.OperationAwaiter,RD.Client.ResourcesLoaderComponentSystem.<LoadSceneAsync>d__5>(YooAsset.OperationAwaiter&,RD.Client.ResourcesLoaderComponentSystem.<LoadSceneAsync>d__5&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.A2NetClient_MessageHandler.<Run>d__0>(object&,RD.Client.A2NetClient_MessageHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.A2NetClient_RequestHandler.<Run>d__0>(object&,RD.Client.A2NetClient_RequestHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.AI_Attack.<Execute>d__1>(object&,RD.Client.AI_Attack.<Execute>d__1&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.AI_Patrol.<Execute>d__1>(object&,RD.Client.AI_Patrol.<Execute>d__1&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.AfterCreateClientScene_AddComponent.<Run>d__0>(object&,RD.Client.AfterCreateClientScene_AddComponent.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.AfterCreateCurrentScene_AddComponent.<Run>d__0>(object&,RD.Client.AfterCreateCurrentScene_AddComponent.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.AfterUnitCreate_CreateUnitView.<Run>d__0>(object&,RD.Client.AfterUnitCreate_CreateUnitView.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.AppStartInitFinish_CreateLoginUI.<Run>d__0>(object&,RD.Client.AppStartInitFinish_CreateLoginUI.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.ChangePosition_SyncGameObjectPos.<Run>d__0>(object&,RD.Client.ChangePosition_SyncGameObjectPos.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.ChangeRotation_SyncGameObjectRotation.<Run>d__0>(object&,RD.Client.ChangeRotation_SyncGameObjectRotation.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.ClientInitEvent.<Run>d__0>(object&,RD.Client.ClientInitEvent.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.ClientSenderComponentSystem.<DisposeAsync>d__3>(object&,RD.Client.ClientSenderComponentSystem.<DisposeAsync>d__3&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.ClientSenderComponentSystem.<RemoveFiberAsync>d__2>(object&,RD.Client.ClientSenderComponentSystem.<RemoveFiberAsync>d__2&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.EnterMapHelper.<EnterMapAsync>d__0>(object&,RD.Client.EnterMapHelper.<EnterMapAsync>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.LobbyPanelComponentSystem.<OnEventEnterMapInvoke>d__6>(object&,RD.Client.LobbyPanelComponentSystem.<OnEventEnterMapInvoke>d__6&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.LoginFinish_CreateLobbyUI.<Run>d__0>(object&,RD.Client.LoginFinish_CreateLobbyUI.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.LoginFinish_RemoveLoginUI.<Run>d__0>(object&,RD.Client.LoginFinish_RemoveLoginUI.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.LoginHelper.<Login>d__0>(object&,RD.Client.LoginHelper.<Login>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.LoginPanelComponentSystem.<DoLogin>d__9>(object&,RD.Client.LoginPanelComponentSystem.<DoLogin>d__9&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.LoginPanelComponentSystem.<OnEventLoginInvoke>d__6>(object&,RD.Client.LoginPanelComponentSystem.<OnEventLoginInvoke>d__6&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.LoginPanelComponentSystem.<OnEventQQInvoke>d__8>(object&,RD.Client.LoginPanelComponentSystem.<OnEventQQInvoke>d__8&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.LoginPanelComponentSystem.<OnEventWeChatInvoke>d__7>(object&,RD.Client.LoginPanelComponentSystem.<OnEventWeChatInvoke>d__7&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.M2C_CreateMyUnitHandler.<Run>d__0>(object&,RD.Client.M2C_CreateMyUnitHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.M2C_CreateUnitsHandler.<Run>d__0>(object&,RD.Client.M2C_CreateUnitsHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.M2C_PathfindingResultHandler.<Run>d__0>(object&,RD.Client.M2C_PathfindingResultHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.M2C_RemoveUnitsHandler.<Run>d__0>(object&,RD.Client.M2C_RemoveUnitsHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.M2C_StartSceneChangeHandler.<Run>d__0>(object&,RD.Client.M2C_StartSceneChangeHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.M2C_StopHandler.<Run>d__0>(object&,RD.Client.M2C_StopHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.Main2NetClient_LoginHandler.<Run>d__0>(object&,RD.Client.Main2NetClient_LoginHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.MoveHelper.<MoveToAsync>d__1>(object&,RD.Client.MoveHelper.<MoveToAsync>d__1&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.NetClient2Main_SessionDisposeHandler.<Run>d__0>(object&,RD.Client.NetClient2Main_SessionDisposeHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.NetClientFiberInit.<Handle>d__0>(object&,RD.Client.NetClientFiberInit.<Handle>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.On_UIEventInitializeBefore_CountDownHandler.<Run>d__0>(object&,RD.Client.On_UIEventInitializeBefore_CountDownHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.On_UIEvent_YooAssetsLoad_Completed_SpriteHandler.<Run>d__0>(object&,RD.Client.On_UIEvent_YooAssetsLoad_Completed_SpriteHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.OperaComponentSystem.<Test1>d__3>(object&,RD.Client.OperaComponentSystem.<Test1>d__3&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.OperaComponentSystem.<Test2>d__4>(object&,RD.Client.OperaComponentSystem.<Test2>d__4&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.OperaComponentSystem.<TestCancelAfter>d__5>(object&,RD.Client.OperaComponentSystem.<TestCancelAfter>d__5&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.PingComponentSystem.<PingAsync>d__2>(object&,RD.Client.PingComponentSystem.<PingAsync>d__2&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.ResourcesLoaderComponentSystem.<LoadSceneAsync>d__5>(object&,RD.Client.ResourcesLoaderComponentSystem.<LoadSceneAsync>d__5&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.RouterAddressComponentSystem.<GetAllRouter>d__2>(object&,RD.Client.RouterAddressComponentSystem.<GetAllRouter>d__2&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.RouterAddressComponentSystem.<Init>d__1>(object&,RD.Client.RouterAddressComponentSystem.<Init>d__1&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.RouterAddressComponentSystem.<WaitTenMinGetAllRouter>d__3>(object&,RD.Client.RouterAddressComponentSystem.<WaitTenMinGetAllRouter>d__3&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.RouterCheckComponentSystem.<CheckAsync>d__1>(object&,RD.Client.RouterCheckComponentSystem.<CheckAsync>d__1&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.SceneChangeFinishEvent_CreateUIHelp.<Run>d__0>(object&,RD.Client.SceneChangeFinishEvent_CreateUIHelp.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.SceneChangeHelper.<SceneChangeTo>d__0>(object&,RD.Client.SceneChangeHelper.<SceneChangeTo>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.SceneChangeStart_AddComponent.<Run>d__0>(object&,RD.Client.SceneChangeStart_AddComponent.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.UIChildSystem.<>c__DisplayClass12_0.<<OpenAllChild>g__Open|0>d>(object&,RD.Client.UIChildSystem.<>c__DisplayClass12_0.<<OpenAllChild>g__Open|0>d&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.UIChildSystem.<>c__DisplayClass14_0.<<CloseAllChild>g__Close|0>d>(object&,RD.Client.UIChildSystem.<>c__DisplayClass14_0.<<CloseAllChild>g__Close|0>d&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.UICloseCommonComponentSystem.<OnEventCloseInvoke>d__6>(object&,RD.Client.UICloseCommonComponentSystem.<OnEventCloseInvoke>d__6&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.UIEventPanelCloseAfterHandler.<Run>d__0>(object&,RD.Client.UIEventPanelCloseAfterHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.UIEventPanelCloseBeforeHandler.<Run>d__0>(object&,RD.Client.UIEventPanelCloseBeforeHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.UIEventPanelDestroyHandler.<Run>d__0>(object&,RD.Client.UIEventPanelDestroyHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.UIEventPanelOpenAfterHandler.<Run>d__0>(object&,RD.Client.UIEventPanelOpenAfterHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.UIEventPanelOpenBeforeHandler.<Run>d__0>(object&,RD.Client.UIEventPanelOpenBeforeHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.UIInvokeTimerComponentWaitAsyncHandler.<Handle>d__0>(object&,RD.Client.UIInvokeTimerComponentWaitAsyncHandler.<Handle>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.UIInvokeTimerComponentWaitFrameAsyncHandler.<Handle>d__0>(object&,RD.Client.UIInvokeTimerComponentWaitFrameAsyncHandler.<Handle>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.UIInvokeTimerComponentWaitSecondAsyncHandler.<Handle>d__0>(object&,RD.Client.UIInvokeTimerComponentWaitSecondAsyncHandler.<Handle>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.UILoadComponentSystem.<LoadAssetAsyncAction>d__17>(object&,RD.Client.UILoadComponentSystem.<LoadAssetAsyncAction>d__17&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.UILoadComponentSystem.<LoadAssetAsyncAction>d__5<object>>(object&,RD.Client.UILoadComponentSystem.<LoadAssetAsyncAction>d__5<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.UILoadComponentSystem.<LoadAssetAsyncInstantiate>d__8>(object&,RD.Client.UILoadComponentSystem.<LoadAssetAsyncInstantiate>d__8&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.UIMgrComponentSystem.<AddUICloseElse>d__12>(object&,RD.Client.UIMgrComponentSystem.<AddUICloseElse>d__12&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.UIMgrComponentSystem.<BanLayerOptionCountDown>d__17>(object&,RD.Client.UIMgrComponentSystem.<BanLayerOptionCountDown>d__17&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.UIMgrComponentSystem.<CloseAll>d__36>(object&,RD.Client.UIMgrComponentSystem.<CloseAll>d__36&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.UIMgrComponentSystem.<OpenPanelAfter>d__49>(object&,RD.Client.UIMgrComponentSystem.<OpenPanelAfter>d__49&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.UIMgrComponentSystem.<OpenPanelBefore>d__48>(object&,RD.Client.UIMgrComponentSystem.<OpenPanelBefore>d__48&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.UIMgrComponentSystem.<RemoveUIAddElse>d__13>(object&,RD.Client.UIMgrComponentSystem.<RemoveUIAddElse>d__13&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.UIPanelComponentSystem.<CloseAllView>d__49>(object&,RD.Client.UIPanelComponentSystem.<CloseAllView>d__49&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.UIPanelComponentSystem.<CloseAllViewTween>d__50>(object&,RD.Client.UIPanelComponentSystem.<CloseAllViewTween>d__50&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.UIPanelComponentSystem.<CloseLastView>d__48>(object&,RD.Client.UIPanelComponentSystem.<CloseLastView>d__48&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.UIPanelComponentSystem.<DoCountDownDestroyPanel>d__36>(object&,RD.Client.UIPanelComponentSystem.<DoCountDownDestroyPanel>d__36&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.UIPanelComponentSystem.<OpenAllViewTween>d__51>(object&,RD.Client.UIPanelComponentSystem.<OpenAllViewTween>d__51&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.UIPanelComponentSystem.<OpenViewAfter>d__47>(object&,RD.Client.UIPanelComponentSystem.<OpenViewAfter>d__47&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.UIPanelComponentSystem.<OpenViewBefore>d__46>(object&,RD.Client.UIPanelComponentSystem.<OpenViewBefore>d__46&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.UIWaitComponentSystem.<UIWindowClose>d__2>(object&,RD.Client.UIWaitComponentSystem.<UIWindowClose>d__2&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.UIWindowComponentSystem.<InternalOnWindowCloseTween>d__3>(object&,RD.Client.UIWindowComponentSystem.<InternalOnWindowCloseTween>d__3&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.UIWindowComponentSystem.<InternalOnWindowOpenTween>d__2>(object&,RD.Client.UIWindowComponentSystem.<InternalOnWindowOpenTween>d__2&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.UIWindowComponentSystem.<OnCloseTween>d__9>(object&,RD.Client.UIWindowComponentSystem.<OnCloseTween>d__9&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.UIWindowComponentSystem.<OnOpenTween>d__8>(object&,RD.Client.UIWindowComponentSystem.<OnOpenTween>d__8&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.UIWindowComponentSystem.<SealedOnWindowCloseTween>d__7>(object&,RD.Client.UIWindowComponentSystem.<SealedOnWindowCloseTween>d__7&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.UIWindowComponentSystem.<SealedOnWindowOpenTween>d__6>(object&,RD.Client.UIWindowComponentSystem.<SealedOnWindowOpenTween>d__6&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.WindowFadeAnim.<GetAwaiter>d__0>(object&,RD.Client.WindowFadeAnim.<GetAwaiter>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.WindowFadeAnim.<In>d__1>(object&,RD.Client.WindowFadeAnim.<In>d__1&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Client.WindowFadeAnim.<Out>d__2>(object&,RD.Client.WindowFadeAnim.<Out>d__2&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.HashWaitSystem.<Notify>d__4>(object&,RD.HashWaitSystem.<Notify>d__4&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.MailBoxType_UnOrderedMessageHandler.<HandleAsync>d__1>(object&,RD.MailBoxType_UnOrderedMessageHandler.<HandleAsync>d__1&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.MessageHandler.<Handle>d__1<object,object,object>>(object&,RD.MessageHandler.<Handle>d__1<object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.MessageHandler.<Handle>d__1<object,object>>(object&,RD.MessageHandler.<Handle>d__1<object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.MessageSessionHandler.<HandleAsync>d__2<object,object>>(object&,RD.MessageSessionHandler.<HandleAsync>d__2<object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.MessageSessionHandler.<HandleAsync>d__2<object>>(object&,RD.MessageSessionHandler.<HandleAsync>d__2<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.NumericChangeEvent_NotifyWatcher.<Run>d__0>(object&,RD.NumericChangeEvent_NotifyWatcher.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.ProcessInnerSenderSystem.<>c__DisplayClass10_0.<<Call>g__Timeout|0>d>(object&,RD.ProcessInnerSenderSystem.<>c__DisplayClass10_0.<<Call>g__Timeout|0>d&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.PromiseCancellationTokenHelper.<AddCancel>d__1>(object&,RD.PromiseCancellationTokenHelper.<AddCancel>d__1&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.PromiseCancellationTokenHelper.<TimeoutAsync>d__0>(object&,RD.PromiseCancellationTokenHelper.<TimeoutAsync>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.PromiseCancellationTokenHelper.<TimeoutAsync>d__3>(object&,RD.PromiseCancellationTokenHelper.<TimeoutAsync>d__3&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.PromiseCancellationTokenHelper.<TimeoutAsync>d__5>(object&,RD.PromiseCancellationTokenHelper.<TimeoutAsync>d__5&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.PromiseHelperExtend.<WaitUntil>d__0>(object&,RD.PromiseHelperExtend.<WaitUntil>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.PromiseHelperExtend.<WaitUntil>d__1>(object&,RD.PromiseHelperExtend.<WaitUntil>d__1&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Server.A2NetInner_MessageHandler.<Run>d__0>(object&,RD.Server.A2NetInner_MessageHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Server.A2NetInner_RequestHandler.<Run>d__0>(object&,RD.Server.A2NetInner_RequestHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Server.C2G_EnterMapHandler.<Run>d__0>(object&,RD.Server.C2G_EnterMapHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Server.C2G_LoginGateHandler.<Run>d__0>(object&,RD.Server.C2G_LoginGateHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Server.C2G_PingHandler.<Run>d__0>(object&,RD.Server.C2G_PingHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Server.C2M_PathfindingResultHandler.<Run>d__0>(object&,RD.Server.C2M_PathfindingResultHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Server.C2M_StopHandler.<Run>d__0>(object&,RD.Server.C2M_StopHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Server.C2M_TestRobotCaseHandler.<Run>d__0>(object&,RD.Server.C2M_TestRobotCaseHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Server.C2M_TransferMapHandler.<Run>d__0>(object&,RD.Server.C2M_TransferMapHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Server.C2R_LoginHandler.<CloseSession>d__1>(object&,RD.Server.C2R_LoginHandler.<CloseSession>d__1&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Server.C2R_LoginHandler.<Run>d__0>(object&,RD.Server.C2R_LoginHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Server.ChangePosition_NotifyAOI.<Run>d__0>(object&,RD.Server.ChangePosition_NotifyAOI.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Server.ConsoleComponentSystem.<Start>d__1>(object&,RD.Server.ConsoleComponentSystem.<Start>d__1&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Server.CreateRobotConsoleHandler.<Run>d__0>(object&,RD.Server.CreateRobotConsoleHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Server.G2M_SessionDisconnectHandler.<Run>d__0>(object&,RD.Server.G2M_SessionDisconnectHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Server.GateFiberInit.<Handle>d__0>(object&,RD.Server.GateFiberInit.<Handle>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Server.GateSessionKeyComponentSystem.<TimeoutRemoveKey>d__3>(object&,RD.Server.GateSessionKeyComponentSystem.<TimeoutRemoveKey>d__3&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Server.HttpComponentSystem.<Handle>d__3>(object&,RD.Server.HttpComponentSystem.<Handle>d__3&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Server.HttpGetRouterHandler.<Handle>d__0>(object&,RD.Server.HttpGetRouterHandler.<Handle>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Server.LocationFiberInit.<Handle>d__0>(object&,RD.Server.LocationFiberInit.<Handle>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Server.LocationOneTypeSystem.<>c__DisplayClass3_0.<<Lock>g__TimeWaitAsync|0>d>(object&,RD.Server.LocationOneTypeSystem.<>c__DisplayClass3_0.<<Lock>g__TimeWaitAsync|0>d&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Server.LocationOneTypeSystem.<Add>d__1>(object&,RD.Server.LocationOneTypeSystem.<Add>d__1&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Server.LocationOneTypeSystem.<Lock>d__3>(object&,RD.Server.LocationOneTypeSystem.<Lock>d__3&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Server.LocationOneTypeSystem.<Remove>d__2>(object&,RD.Server.LocationOneTypeSystem.<Remove>d__2&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Server.LocationProxyComponentSystem.<Add>d__1>(object&,RD.Server.LocationProxyComponentSystem.<Add>d__1&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Server.LocationProxyComponentSystem.<AddLocation>d__6>(object&,RD.Server.LocationProxyComponentSystem.<AddLocation>d__6&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Server.LocationProxyComponentSystem.<Lock>d__2>(object&,RD.Server.LocationProxyComponentSystem.<Lock>d__2&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Server.LocationProxyComponentSystem.<Remove>d__4>(object&,RD.Server.LocationProxyComponentSystem.<Remove>d__4&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Server.LocationProxyComponentSystem.<RemoveLocation>d__7>(object&,RD.Server.LocationProxyComponentSystem.<RemoveLocation>d__7&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Server.LocationProxyComponentSystem.<UnLock>d__3>(object&,RD.Server.LocationProxyComponentSystem.<UnLock>d__3&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Server.M2M_UnitTransferRequestHandler.<Run>d__0>(object&,RD.Server.M2M_UnitTransferRequestHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Server.MailBoxType_OrderedMessageHandler.<HandleInner>d__1>(object&,RD.Server.MailBoxType_OrderedMessageHandler.<HandleInner>d__1&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Server.MapFiberInit.<Handle>d__0>(object&,RD.Server.MapFiberInit.<Handle>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Server.MessageLocationHandler.<Handle>d__1<object,object,object>>(object&,RD.Server.MessageLocationHandler.<Handle>d__1<object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Server.MessageLocationHandler.<Handle>d__1<object,object>>(object&,RD.Server.MessageLocationHandler.<Handle>d__1<object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Server.MessageLocationSenderComponentSystem.<SendInner>d__7>(object&,RD.Server.MessageLocationSenderComponentSystem.<SendInner>d__7&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Server.MoveHelper.<FindPathMoveToAsync>d__0>(object&,RD.Server.MoveHelper.<FindPathMoveToAsync>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Server.NetComponentOnReadInvoker_Gate.<HandleAsync>d__1>(object&,RD.Server.NetComponentOnReadInvoker_Gate.<HandleAsync>d__1&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Server.NetInnerFiberInit.<Handle>d__0>(object&,RD.Server.NetInnerFiberInit.<Handle>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Server.ObjectAddRequestHandler.<Run>d__0>(object&,RD.Server.ObjectAddRequestHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Server.ObjectGetRequestHandler.<Run>d__0>(object&,RD.Server.ObjectGetRequestHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Server.ObjectLockRequestHandler.<Run>d__0>(object&,RD.Server.ObjectLockRequestHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Server.ObjectRemoveRequestHandler.<Run>d__0>(object&,RD.Server.ObjectRemoveRequestHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Server.ObjectUnLockRequestHandler.<Run>d__0>(object&,RD.Server.ObjectUnLockRequestHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Server.ProcessOuterSenderSystem.<>c__DisplayClass13_0.<<Call>g__Timeout|0>d>(object&,RD.Server.ProcessOuterSenderSystem.<>c__DisplayClass13_0.<<Call>g__Timeout|0>d&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Server.ProcessOuterSenderSystem.<>c__DisplayClass3_0.<<OnRead>g__CallInner|0>d>(object&,RD.Server.ProcessOuterSenderSystem.<>c__DisplayClass3_0.<<OnRead>g__CallInner|0>d&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Server.ProfilerConsoleHandler.<Run>d__0>(object&,RD.Server.ProfilerConsoleHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Server.R2G_GetLoginKeyHandler.<Run>d__0>(object&,RD.Server.R2G_GetLoginKeyHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Server.RealmFiberInit.<Handle>d__0>(object&,RD.Server.RealmFiberInit.<Handle>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Server.ReloadConfigConsoleHandler.<Run>d__0>(object&,RD.Server.ReloadConfigConsoleHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Server.RobotFiberInit.<Handle>d__0>(object&,RD.Server.RobotFiberInit.<Handle>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Server.RobotManagerComponentSystem.<<Destroy>g__Remove|1_0>d>(object&,RD.Server.RobotManagerComponentSystem.<<Destroy>g__Remove|1_0>d&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Server.RobotManagerComponentSystem.<NewRobot>d__2>(object&,RD.Server.RobotManagerComponentSystem.<NewRobot>d__2&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Server.RouterFiberInit.<Handle>d__0>(object&,RD.Server.RouterFiberInit.<Handle>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Server.RouterManagerFiberInit.<Handle>d__0>(object&,RD.Server.RouterManagerFiberInit.<Handle>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Server.ServerInitEvent.<Run>d__0>(object&,RD.Server.ServerInitEvent.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Server.TransferHelper.<Transfer>d__1>(object&,RD.Server.TransferHelper.<Transfer>d__1&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Server.TransferHelper.<TransferAtFrameFinish>d__0>(object&,RD.Server.TransferHelper.<TransferAtFrameFinish>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Server.UnitEnterSightRange_NotifyClient.<Run>d__0>(object&,RD.Server.UnitEnterSightRange_NotifyClient.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.Server.UnitLeaveSightRange_NotifyClient.<Run>d__0>(object&,RD.Server.UnitLeaveSightRange_NotifyClient.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.ShareInitEvent.<Run>d__0>(object&,RD.ShareInitEvent.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.TimerComponentSystem.<WaitAsync>d__10>(object&,RD.TimerComponentSystem.<WaitAsync>d__10&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.TimerComponentSystem.<WaitFrameAsync>d__9>(object&,RD.TimerComponentSystem.<WaitFrameAsync>d__9&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.AwaitUnsafeOnCompleted<object,RD.TimerComponentSystem.<WaitTillAsync>d__8>(object&,RD.TimerComponentSystem.<WaitTillAsync>d__8&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<RD.Core.ActorId>.AwaitUnsafeOnCompleted<object,RD.Server.LocationOneTypeSystem.<Get>d__5>(object&,RD.Server.LocationOneTypeSystem.<Get>d__5&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<RD.Core.ActorId>.AwaitUnsafeOnCompleted<object,RD.Server.LocationProxyComponentSystem.<Get>d__5>(object&,RD.Server.LocationProxyComponentSystem.<Get>d__5&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<System.ValueTuple<object,int>>.AwaitUnsafeOnCompleted<YooAsset.OperationAwaiter,RD.Client.UIYooAssetsLoadComponentSystem.<LoadAssetAsyncFunc>d__4>(YooAsset.OperationAwaiter&,RD.Client.UIYooAssetsLoadComponentSystem.<LoadAssetAsyncFunc>d__4&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<System.ValueTuple<uint,object>>.AwaitUnsafeOnCompleted<object,RD.Client.RouterHelper.<GetRouterAddress>d__1>(object&,RD.Client.RouterHelper.<GetRouterAddress>d__1&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.AwaitUnsafeOnCompleted<object,RD.Client.LobbyPanelComponentSystem.<UIOpen>d__5>(object&,RD.Client.LobbyPanelComponentSystem.<UIOpen>d__5&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.AwaitUnsafeOnCompleted<object,RD.Client.LoginPanelComponentSystem.<UIOpen>d__5>(object&,RD.Client.LoginPanelComponentSystem.<UIOpen>d__5&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.AwaitUnsafeOnCompleted<object,RD.Client.MainPanelComponentSystem.<UIOpen>d__5>(object&,RD.Client.MainPanelComponentSystem.<UIOpen>d__5&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.AwaitUnsafeOnCompleted<object,RD.Client.UIChildSystem.<CloseAllChild>d__14>(object&,RD.Client.UIChildSystem.<CloseAllChild>d__14&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.AwaitUnsafeOnCompleted<object,RD.Client.UIChildSystem.<OpenAllChild>d__12>(object&,RD.Client.UIChildSystem.<OpenAllChild>d__12&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.AwaitUnsafeOnCompleted<object,RD.Client.UICloseCommonComponentSystem.<CloseUI>d__5>(object&,RD.Client.UICloseCommonComponentSystem.<CloseUI>d__5&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.AwaitUnsafeOnCompleted<object,RD.Client.UIInvokeClosePanelAsyncHandler.<Handle>d__0>(object&,RD.Client.UIInvokeClosePanelAsyncHandler.<Handle>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.AwaitUnsafeOnCompleted<object,RD.Client.UIInvokeHomePanelWaitHandler.<Handle>d__0>(object&,RD.Client.UIInvokeHomePanelWaitHandler.<Handle>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.AwaitUnsafeOnCompleted<object,RD.Client.UIInvokeRootOpenPanelAsyncHandler.<Handle>d__0>(object&,RD.Client.UIInvokeRootOpenPanelAsyncHandler.<Handle>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.AwaitUnsafeOnCompleted<object,RD.Client.UIInvokeViewClosePanelAsyncHandler.<Handle>d__0>(object&,RD.Client.UIInvokeViewClosePanelAsyncHandler.<Handle>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.AwaitUnsafeOnCompleted<object,RD.Client.UIInvokeYooAssetsHandler.<Handle>d__0>(object&,RD.Client.UIInvokeYooAssetsHandler.<Handle>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.AwaitUnsafeOnCompleted<object,RD.Client.UILoadComponentSystem.<Initialize>d__2>(object&,RD.Client.UILoadComponentSystem.<Initialize>d__2&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.AwaitUnsafeOnCompleted<object,RD.Client.UIMgrComponentSystem.<CloseLayerTopPanelAsync>d__33>(object&,RD.Client.UIMgrComponentSystem.<CloseLayerTopPanelAsync>d__33&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.AwaitUnsafeOnCompleted<object,RD.Client.UIMgrComponentSystem.<ClosePanelAsync>d__25>(object&,RD.Client.UIMgrComponentSystem.<ClosePanelAsync>d__25&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.AwaitUnsafeOnCompleted<object,RD.Client.UIMgrComponentSystem.<ClosePanelAsync>d__28<object>>(object&,RD.Client.UIMgrComponentSystem.<ClosePanelAsync>d__28<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.AwaitUnsafeOnCompleted<object,RD.Client.UIMgrComponentSystem.<CloseTopPanelAsync>d__35>(object&,RD.Client.UIMgrComponentSystem.<CloseTopPanelAsync>d__35&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.AwaitUnsafeOnCompleted<object,RD.Client.UIMgrComponentSystem.<HomePanel>d__30>(object&,RD.Client.UIMgrComponentSystem.<HomePanel>d__30&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.AwaitUnsafeOnCompleted<object,RD.Client.UIMgrComponentSystem.<HomePanel>d__31<object>>(object&,RD.Client.UIMgrComponentSystem.<HomePanel>d__31<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.AwaitUnsafeOnCompleted<object,RD.Client.UIMgrComponentSystem.<InitRoot>d__84>(object&,RD.Client.UIMgrComponentSystem.<InitRoot>d__84&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.AwaitUnsafeOnCompleted<object,RD.Client.UIMgrComponentSystem.<Initialize>d__43>(object&,RD.Client.UIMgrComponentSystem.<Initialize>d__43&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.AwaitUnsafeOnCompleted<object,RD.Client.UIMgrComponentSystem.<PreLoadPanelAsync>d__79<object>>(object&,RD.Client.UIMgrComponentSystem.<PreLoadPanelAsync>d__79<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.AwaitUnsafeOnCompleted<object,RD.Client.UIMgrComponentSystem.<PreLoadPanelAsync>d__80>(object&,RD.Client.UIMgrComponentSystem.<PreLoadPanelAsync>d__80&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.AwaitUnsafeOnCompleted<object,RD.Client.UIMgrComponentSystem.<RemoveUIToHome>d__14>(object&,RD.Client.UIMgrComponentSystem.<RemoveUIToHome>d__14&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.AwaitUnsafeOnCompleted<object,RD.Client.UIPanelComponentSystem.<CloseAsync>d__4>(object&,RD.Client.UIPanelComponentSystem.<CloseAsync>d__4&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.AwaitUnsafeOnCompleted<object,RD.Client.UIPanelComponentSystem.<CloseViewAsync>d__10>(object&,RD.Client.UIPanelComponentSystem.<CloseViewAsync>d__10&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.AwaitUnsafeOnCompleted<object,RD.Client.UIPanelComponentSystem.<CloseViewAsync>d__11>(object&,RD.Client.UIPanelComponentSystem.<CloseViewAsync>d__11&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.AwaitUnsafeOnCompleted<object,RD.Client.UIPanelComponentSystem.<CloseViewAsync>d__9<object>>(object&,RD.Client.UIPanelComponentSystem.<CloseViewAsync>d__9<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.AwaitUnsafeOnCompleted<object,RD.Client.UIPanelComponentSystem.<Home>d__5<object>>(object&,RD.Client.UIPanelComponentSystem.<Home>d__5<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.AwaitUnsafeOnCompleted<object,RD.Client.UIPanelComponentSystem.<Home>d__6>(object&,RD.Client.UIPanelComponentSystem.<Home>d__6&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.AwaitUnsafeOnCompleted<object,RD.Client.UIPanelComponentSystem.<Open>d__27>(object&,RD.Client.UIPanelComponentSystem.<Open>d__27&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.AwaitUnsafeOnCompleted<object,RD.Client.UIPanelComponentSystem.<Open>d__28>(object&,RD.Client.UIPanelComponentSystem.<Open>d__28&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.AwaitUnsafeOnCompleted<object,RD.Client.UIPanelComponentSystem.<Open>d__29<object>>(object&,RD.Client.UIPanelComponentSystem.<Open>d__29<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.AwaitUnsafeOnCompleted<object,RD.Client.UIPanelComponentSystem.<Open>d__30<object,object>>(object&,RD.Client.UIPanelComponentSystem.<Open>d__30<object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.AwaitUnsafeOnCompleted<object,RD.Client.UIPanelComponentSystem.<Open>d__31<object,object,object>>(object&,RD.Client.UIPanelComponentSystem.<Open>d__31<object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.AwaitUnsafeOnCompleted<object,RD.Client.UIPanelComponentSystem.<Open>d__32<object,object,object,object>>(object&,RD.Client.UIPanelComponentSystem.<Open>d__32<object,object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.AwaitUnsafeOnCompleted<object,RD.Client.UIPanelComponentSystem.<Open>d__33<object,object,object,object,object>>(object&,RD.Client.UIPanelComponentSystem.<Open>d__33<object,object,object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.AwaitUnsafeOnCompleted<object,RD.Client.UIPanelComponentSystem.<UseBaseOpen>d__26>(object&,RD.Client.UIPanelComponentSystem.<UseBaseOpen>d__26&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.AwaitUnsafeOnCompleted<object,RD.Client.UIRootComponentSystem.<PreLoadPanelAsync>d__29<object>>(object&,RD.Client.UIRootComponentSystem.<PreLoadPanelAsync>d__29<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.AwaitUnsafeOnCompleted<object,RD.Client.UIRootComponentSystem.<PreLoadPanelAsync>d__30>(object&,RD.Client.UIRootComponentSystem.<PreLoadPanelAsync>d__30&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.AwaitUnsafeOnCompleted<object,RD.Client.UIViewComponentSystem.<CloseAsync>d__4>(object&,RD.Client.UIViewComponentSystem.<CloseAsync>d__4&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.AwaitUnsafeOnCompleted<object,RD.Client.UIViewComponentSystem.<ClosePanelAsync>d__6>(object&,RD.Client.UIViewComponentSystem.<ClosePanelAsync>d__6&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.AwaitUnsafeOnCompleted<object,RD.Client.UIViewComponentSystem.<Open>d__11>(object&,RD.Client.UIViewComponentSystem.<Open>d__11&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.AwaitUnsafeOnCompleted<object,RD.Client.UIViewComponentSystem.<Open>d__12>(object&,RD.Client.UIViewComponentSystem.<Open>d__12&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.AwaitUnsafeOnCompleted<object,RD.Client.UIViewComponentSystem.<Open>d__13<object>>(object&,RD.Client.UIViewComponentSystem.<Open>d__13<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.AwaitUnsafeOnCompleted<object,RD.Client.UIViewComponentSystem.<Open>d__14<object,object>>(object&,RD.Client.UIViewComponentSystem.<Open>d__14<object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.AwaitUnsafeOnCompleted<object,RD.Client.UIViewComponentSystem.<Open>d__15<object,object,object>>(object&,RD.Client.UIViewComponentSystem.<Open>d__15<object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.AwaitUnsafeOnCompleted<object,RD.Client.UIViewComponentSystem.<Open>d__16<object,object,object,object>>(object&,RD.Client.UIViewComponentSystem.<Open>d__16<object,object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.AwaitUnsafeOnCompleted<object,RD.Client.UIViewComponentSystem.<Open>d__17<object,object,object,object,object>>(object&,RD.Client.UIViewComponentSystem.<Open>d__17<object,object,object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.AwaitUnsafeOnCompleted<object,RD.Client.UIViewComponentSystem.<UseBaseOpen>d__10>(object&,RD.Client.UIViewComponentSystem.<UseBaseOpen>d__10&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.AwaitUnsafeOnCompleted<object,RD.Client.UIYooAssetsLoadComponentSystem.<Initialize>d__2>(object&,RD.Client.UIYooAssetsLoadComponentSystem.<Initialize>d__2&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.AwaitUnsafeOnCompleted<object,RD.MoveComponentSystem.<MoveToAsync>d__5>(object&,RD.MoveComponentSystem.<MoveToAsync>d__5&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<int>.AwaitUnsafeOnCompleted<object,RD.Client.MoveHelper.<MoveToAsync>d__0>(object&,RD.Client.MoveHelper.<MoveToAsync>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<int>.AwaitUnsafeOnCompleted<object,RD.Client.UIMgrComponentSystem.<OpenPanelWaitAsync>d__58<object>>(object&,RD.Client.UIMgrComponentSystem.<OpenPanelWaitAsync>d__58<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<int>.AwaitUnsafeOnCompleted<object,RD.Client.UIMgrComponentSystem.<OpenPanelWaitAsync>d__60<object,object>>(object&,RD.Client.UIMgrComponentSystem.<OpenPanelWaitAsync>d__60<object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<int>.AwaitUnsafeOnCompleted<object,RD.Client.UIMgrComponentSystem.<OpenPanelWaitAsync>d__61<object,object,object>>(object&,RD.Client.UIMgrComponentSystem.<OpenPanelWaitAsync>d__61<object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<int>.AwaitUnsafeOnCompleted<object,RD.Client.UIMgrComponentSystem.<OpenPanelWaitAsync>d__62<object,object,object,object>>(object&,RD.Client.UIMgrComponentSystem.<OpenPanelWaitAsync>d__62<object,object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<int>.AwaitUnsafeOnCompleted<object,RD.Client.UIMgrComponentSystem.<OpenPanelWaitAsync>d__63<object,object,object,object,object>>(object&,RD.Client.UIMgrComponentSystem.<OpenPanelWaitAsync>d__63<object,object,object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<int>.AwaitUnsafeOnCompleted<object,RD.Client.UIMgrComponentSystem.<OpenPanelWaitAsync>d__64<object,object,object,object,object,object>>(object&,RD.Client.UIMgrComponentSystem.<OpenPanelWaitAsync>d__64<object,object,object,object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<int>.AwaitUnsafeOnCompleted<object,RD.Client.UIMgrComponentSystem.<OpenPanelWaitAsync>d__72>(object&,RD.Client.UIMgrComponentSystem.<OpenPanelWaitAsync>d__72&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<int>.AwaitUnsafeOnCompleted<object,RD.Client.UIMgrComponentSystem.<OpenPanelWaitAsync>d__74<object>>(object&,RD.Client.UIMgrComponentSystem.<OpenPanelWaitAsync>d__74<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<int>.AwaitUnsafeOnCompleted<object,RD.Client.UIMgrComponentSystem.<OpenPanelWaitAsync>d__75<object,object>>(object&,RD.Client.UIMgrComponentSystem.<OpenPanelWaitAsync>d__75<object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<int>.AwaitUnsafeOnCompleted<object,RD.Client.UIMgrComponentSystem.<OpenPanelWaitAsync>d__76<object,object,object>>(object&,RD.Client.UIMgrComponentSystem.<OpenPanelWaitAsync>d__76<object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<int>.AwaitUnsafeOnCompleted<object,RD.Client.UIMgrComponentSystem.<OpenPanelWaitAsync>d__77<object,object,object,object>>(object&,RD.Client.UIMgrComponentSystem.<OpenPanelWaitAsync>d__77<object,object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<int>.AwaitUnsafeOnCompleted<object,RD.Client.UIMgrComponentSystem.<OpenPanelWaitAsync>d__78<object,object,object,object,object>>(object&,RD.Client.UIMgrComponentSystem.<OpenPanelWaitAsync>d__78<object,object,object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<int>.AwaitUnsafeOnCompleted<object,RD.Client.UIMgrComponentSystem.<OpenPanelWaitParamAsync>d__59<object>>(object&,RD.Client.UIMgrComponentSystem.<OpenPanelWaitParamAsync>d__59<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<int>.AwaitUnsafeOnCompleted<object,RD.Client.UIMgrComponentSystem.<OpenPanelWaitParamAsync>d__73>(object&,RD.Client.UIMgrComponentSystem.<OpenPanelWaitParamAsync>d__73&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<int>.AwaitUnsafeOnCompleted<object,RD.Client.UIMgrComponentSystem.<PanelWait>d__57>(object&,RD.Client.UIMgrComponentSystem.<PanelWait>d__57&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<int>.AwaitUnsafeOnCompleted<object,RD.Client.UIRootComponentSystem.<OpenPanelWaitAsync>d__10<object,object>>(object&,RD.Client.UIRootComponentSystem.<OpenPanelWaitAsync>d__10<object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<int>.AwaitUnsafeOnCompleted<object,RD.Client.UIRootComponentSystem.<OpenPanelWaitAsync>d__11<object,object,object>>(object&,RD.Client.UIRootComponentSystem.<OpenPanelWaitAsync>d__11<object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<int>.AwaitUnsafeOnCompleted<object,RD.Client.UIRootComponentSystem.<OpenPanelWaitAsync>d__12<object,object,object,object>>(object&,RD.Client.UIRootComponentSystem.<OpenPanelWaitAsync>d__12<object,object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<int>.AwaitUnsafeOnCompleted<object,RD.Client.UIRootComponentSystem.<OpenPanelWaitAsync>d__13<object,object,object,object,object>>(object&,RD.Client.UIRootComponentSystem.<OpenPanelWaitAsync>d__13<object,object,object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<int>.AwaitUnsafeOnCompleted<object,RD.Client.UIRootComponentSystem.<OpenPanelWaitAsync>d__14<object,object,object,object,object,object>>(object&,RD.Client.UIRootComponentSystem.<OpenPanelWaitAsync>d__14<object,object,object,object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<int>.AwaitUnsafeOnCompleted<object,RD.Client.UIRootComponentSystem.<OpenPanelWaitAsync>d__22>(object&,RD.Client.UIRootComponentSystem.<OpenPanelWaitAsync>d__22&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<int>.AwaitUnsafeOnCompleted<object,RD.Client.UIRootComponentSystem.<OpenPanelWaitAsync>d__24<object>>(object&,RD.Client.UIRootComponentSystem.<OpenPanelWaitAsync>d__24<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<int>.AwaitUnsafeOnCompleted<object,RD.Client.UIRootComponentSystem.<OpenPanelWaitAsync>d__25<object,object>>(object&,RD.Client.UIRootComponentSystem.<OpenPanelWaitAsync>d__25<object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<int>.AwaitUnsafeOnCompleted<object,RD.Client.UIRootComponentSystem.<OpenPanelWaitAsync>d__26<object,object,object>>(object&,RD.Client.UIRootComponentSystem.<OpenPanelWaitAsync>d__26<object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<int>.AwaitUnsafeOnCompleted<object,RD.Client.UIRootComponentSystem.<OpenPanelWaitAsync>d__27<object,object,object,object>>(object&,RD.Client.UIRootComponentSystem.<OpenPanelWaitAsync>d__27<object,object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<int>.AwaitUnsafeOnCompleted<object,RD.Client.UIRootComponentSystem.<OpenPanelWaitAsync>d__28<object,object,object,object,object>>(object&,RD.Client.UIRootComponentSystem.<OpenPanelWaitAsync>d__28<object,object,object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<int>.AwaitUnsafeOnCompleted<object,RD.Client.UIRootComponentSystem.<OpenPanelWaitAsync>d__8<object>>(object&,RD.Client.UIRootComponentSystem.<OpenPanelWaitAsync>d__8<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<int>.AwaitUnsafeOnCompleted<object,RD.Client.UIRootComponentSystem.<OpenPanelWaitParamAsync>d__23>(object&,RD.Client.UIRootComponentSystem.<OpenPanelWaitParamAsync>d__23&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<int>.AwaitUnsafeOnCompleted<object,RD.Client.UIRootComponentSystem.<OpenPanelWaitParamAsync>d__9<object>>(object&,RD.Client.UIRootComponentSystem.<OpenPanelWaitParamAsync>d__9<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<int>.AwaitUnsafeOnCompleted<object,RD.HashWaitSystem.<Wait>d__2>(object&,RD.HashWaitSystem.<Wait>d__2&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<int>.AwaitUnsafeOnCompleted<object,RD.HashWaitSystem.<WaitObject>d__5>(object&,RD.HashWaitSystem.<WaitObject>d__5&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<long>.AwaitUnsafeOnCompleted<object,RD.Client.ClientSenderComponentSystem.<LoginAsync>d__4>(object&,RD.Client.ClientSenderComponentSystem.<LoginAsync>d__4&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<YooAsset.OperationAwaiter,RD.Client.ResourcesLoaderComponentSystem.<LoadAllAssetsAsync>d__4<object>>(YooAsset.OperationAwaiter&,RD.Client.ResourcesLoaderComponentSystem.<LoadAllAssetsAsync>d__4<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<YooAsset.OperationAwaiter,RD.Client.ResourcesLoaderComponentSystem.<LoadAssetAsync>d__3<object>>(YooAsset.OperationAwaiter&,RD.Client.ResourcesLoaderComponentSystem.<LoadAssetAsync>d__3<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.ClientSenderComponentSystem.<Call>d__6>(object&,RD.Client.ClientSenderComponentSystem.<Call>d__6&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.GetAllConfigBytes.<Handle>d__0>(object&,RD.Client.GetAllConfigBytes.<Handle>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.GetOneConfigBytes.<Handle>d__0>(object&,RD.Client.GetOneConfigBytes.<Handle>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.ResourcesLoaderComponentSystem.<LoadAllAssetsAsync>d__4<object>>(object&,RD.Client.ResourcesLoaderComponentSystem.<LoadAllAssetsAsync>d__4<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.ResourcesLoaderComponentSystem.<LoadAssetAsync>d__3<object>>(object&,RD.Client.ResourcesLoaderComponentSystem.<LoadAssetAsync>d__3<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.RouterHelper.<CreateRouterSession>d__0>(object&,RD.Client.RouterHelper.<CreateRouterSession>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UIFactory.<CreateAsync>d__13>(object&,RD.Client.UIFactory.<CreateAsync>d__13&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UIFactory.<CreateGameObjectAsync>d__15>(object&,RD.Client.UIFactory.<CreateGameObjectAsync>d__15&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UIFactory.<CreatePanelAsync>d__12>(object&,RD.Client.UIFactory.<CreatePanelAsync>d__12&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UIFactory.<CreatePanelGameObjectAsync>d__14>(object&,RD.Client.UIFactory.<CreatePanelGameObjectAsync>d__14&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UIFactory.<InstantiateAsync>d__10>(object&,RD.Client.UIFactory.<InstantiateAsync>d__10&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UIFactory.<InstantiateAsync>d__11>(object&,RD.Client.UIFactory.<InstantiateAsync>d__11&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UIFactory.<InstantiateAsync>d__6<object>>(object&,RD.Client.UIFactory.<InstantiateAsync>d__6<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UIFactory.<InstantiateAsync>d__7<object>>(object&,RD.Client.UIFactory.<InstantiateAsync>d__7<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UIFactory.<InstantiateAsync>d__8>(object&,RD.Client.UIFactory.<InstantiateAsync>d__8&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UIFactory.<InstantiateAsync>d__9>(object&,RD.Client.UIFactory.<InstantiateAsync>d__9&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UIFactory.<InstantiateGameObjectAsync>d__1>(object&,RD.Client.UIFactory.<InstantiateGameObjectAsync>d__1&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UIFactory.<InstantiateGameObjectAsync>d__27>(object&,RD.Client.UIFactory.<InstantiateGameObjectAsync>d__27&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UIInvokeCoroutineLockHandler.<Handle>d__0>(object&,RD.Client.UIInvokeCoroutineLockHandler.<Handle>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UIInvokeInstantiateGameObjectAsyncHandler.<Handle>d__0>(object&,RD.Client.UIInvokeInstantiateGameObjectAsyncHandler.<Handle>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UIInvokeLoadAsyncHandler.<Handle>d__0>(object&,RD.Client.UIInvokeLoadAsyncHandler.<Handle>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UIInvokeLoadInstantiateAsyncHandler.<Handle>d__0>(object&,RD.Client.UIInvokeLoadInstantiateAsyncHandler.<Handle>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UIInvokeLoadInstantiateByVoAsyncHandler.<Handle>d__0>(object&,RD.Client.UIInvokeLoadInstantiateByVoAsyncHandler.<Handle>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UIInvokeLoadSpriteAsyncHandler.<Handle>d__0>(object&,RD.Client.UIInvokeLoadSpriteAsyncHandler.<Handle>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UIInvokeLoadTexture2DAsyncHandler.<Handle>d__0>(object&,RD.Client.UIInvokeLoadTexture2DAsyncHandler.<Handle>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UILoadComponentSystem.<LoadAssetAsync>d__15>(object&,RD.Client.UILoadComponentSystem.<LoadAssetAsync>d__15&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UILoadComponentSystem.<LoadAssetAsync>d__19<object>>(object&,RD.Client.UILoadComponentSystem.<LoadAssetAsync>d__19<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UILoadComponentSystem.<LoadAssetAsync>d__23>(object&,RD.Client.UILoadComponentSystem.<LoadAssetAsync>d__23&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UILoadComponentSystem.<LoadAssetAsync>d__3<object>>(object&,RD.Client.UILoadComponentSystem.<LoadAssetAsync>d__3<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UILoadComponentSystem.<LoadAssetAsyncInstantiate>d__7>(object&,RD.Client.UILoadComponentSystem.<LoadAssetAsyncInstantiate>d__7&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UIMgrComponentSystem.<OpenPanelAsync>d__50<object>>(object&,RD.Client.UIMgrComponentSystem.<OpenPanelAsync>d__50<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UIMgrComponentSystem.<OpenPanelAsync>d__52<object,object>>(object&,RD.Client.UIMgrComponentSystem.<OpenPanelAsync>d__52<object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UIMgrComponentSystem.<OpenPanelAsync>d__53<object,object,object>>(object&,RD.Client.UIMgrComponentSystem.<OpenPanelAsync>d__53<object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UIMgrComponentSystem.<OpenPanelAsync>d__54<object,object,object,object>>(object&,RD.Client.UIMgrComponentSystem.<OpenPanelAsync>d__54<object,object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UIMgrComponentSystem.<OpenPanelAsync>d__55<object,object,object,object,object>>(object&,RD.Client.UIMgrComponentSystem.<OpenPanelAsync>d__55<object,object,object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UIMgrComponentSystem.<OpenPanelAsync>d__56<object,object,object,object,object,object>>(object&,RD.Client.UIMgrComponentSystem.<OpenPanelAsync>d__56<object,object,object,object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UIMgrComponentSystem.<OpenPanelAsync>d__65>(object&,RD.Client.UIMgrComponentSystem.<OpenPanelAsync>d__65&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UIMgrComponentSystem.<OpenPanelAsync>d__67<object>>(object&,RD.Client.UIMgrComponentSystem.<OpenPanelAsync>d__67<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UIMgrComponentSystem.<OpenPanelAsync>d__68<object,object>>(object&,RD.Client.UIMgrComponentSystem.<OpenPanelAsync>d__68<object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UIMgrComponentSystem.<OpenPanelAsync>d__69<object,object,object>>(object&,RD.Client.UIMgrComponentSystem.<OpenPanelAsync>d__69<object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UIMgrComponentSystem.<OpenPanelAsync>d__70<object,object,object,object>>(object&,RD.Client.UIMgrComponentSystem.<OpenPanelAsync>d__70<object,object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UIMgrComponentSystem.<OpenPanelAsync>d__71<object,object,object,object,object>>(object&,RD.Client.UIMgrComponentSystem.<OpenPanelAsync>d__71<object,object,object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UIMgrComponentSystem.<OpenPanelParamAsync>d__51<object>>(object&,RD.Client.UIMgrComponentSystem.<OpenPanelParamAsync>d__51<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UIMgrComponentSystem.<OpenPanelParamAsync>d__66>(object&,RD.Client.UIMgrComponentSystem.<OpenPanelParamAsync>d__66&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UIMgrComponentSystem.<OpenPanelStartAsync>d__47>(object&,RD.Client.UIMgrComponentSystem.<OpenPanelStartAsync>d__47&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UIPanelComponentSystem.<GetView>d__42>(object&,RD.Client.UIPanelComponentSystem.<GetView>d__42&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UIPanelComponentSystem.<GetView>d__43<object>>(object&,RD.Client.UIPanelComponentSystem.<GetView>d__43<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UIPanelComponentSystem.<OpenViewAsync>d__12<object>>(object&,RD.Client.UIPanelComponentSystem.<OpenViewAsync>d__12<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UIPanelComponentSystem.<OpenViewAsync>d__14<object,object>>(object&,RD.Client.UIPanelComponentSystem.<OpenViewAsync>d__14<object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UIPanelComponentSystem.<OpenViewAsync>d__15<object,object,object>>(object&,RD.Client.UIPanelComponentSystem.<OpenViewAsync>d__15<object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UIPanelComponentSystem.<OpenViewAsync>d__16<object,object,object,object>>(object&,RD.Client.UIPanelComponentSystem.<OpenViewAsync>d__16<object,object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UIPanelComponentSystem.<OpenViewAsync>d__17<object,object,object,object,object>>(object&,RD.Client.UIPanelComponentSystem.<OpenViewAsync>d__17<object,object,object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UIPanelComponentSystem.<OpenViewAsync>d__18<object,object,object,object,object,object>>(object&,RD.Client.UIPanelComponentSystem.<OpenViewAsync>d__18<object,object,object,object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UIPanelComponentSystem.<OpenViewAsync>d__19>(object&,RD.Client.UIPanelComponentSystem.<OpenViewAsync>d__19&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UIPanelComponentSystem.<OpenViewAsync>d__21<object>>(object&,RD.Client.UIPanelComponentSystem.<OpenViewAsync>d__21<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UIPanelComponentSystem.<OpenViewAsync>d__22<object,object>>(object&,RD.Client.UIPanelComponentSystem.<OpenViewAsync>d__22<object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UIPanelComponentSystem.<OpenViewAsync>d__23<object,object,object>>(object&,RD.Client.UIPanelComponentSystem.<OpenViewAsync>d__23<object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UIPanelComponentSystem.<OpenViewAsync>d__24<object,object,object,object>>(object&,RD.Client.UIPanelComponentSystem.<OpenViewAsync>d__24<object,object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UIPanelComponentSystem.<OpenViewAsync>d__25<object,object,object,object,object>>(object&,RD.Client.UIPanelComponentSystem.<OpenViewAsync>d__25<object,object,object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UIPanelComponentSystem.<OpenViewParamAsync>d__13<object>>(object&,RD.Client.UIPanelComponentSystem.<OpenViewParamAsync>d__13<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UIPanelComponentSystem.<OpenViewParamAsync>d__20>(object&,RD.Client.UIPanelComponentSystem.<OpenViewParamAsync>d__20&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UIRootComponentSystem.<OpenPanelAsync>d__15>(object&,RD.Client.UIRootComponentSystem.<OpenPanelAsync>d__15&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UIRootComponentSystem.<OpenPanelAsync>d__17<object>>(object&,RD.Client.UIRootComponentSystem.<OpenPanelAsync>d__17<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UIRootComponentSystem.<OpenPanelAsync>d__18<object,object>>(object&,RD.Client.UIRootComponentSystem.<OpenPanelAsync>d__18<object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UIRootComponentSystem.<OpenPanelAsync>d__19<object,object,object>>(object&,RD.Client.UIRootComponentSystem.<OpenPanelAsync>d__19<object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UIRootComponentSystem.<OpenPanelAsync>d__1<object>>(object&,RD.Client.UIRootComponentSystem.<OpenPanelAsync>d__1<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UIRootComponentSystem.<OpenPanelAsync>d__20<object,object,object,object>>(object&,RD.Client.UIRootComponentSystem.<OpenPanelAsync>d__20<object,object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UIRootComponentSystem.<OpenPanelAsync>d__21<object,object,object,object,object>>(object&,RD.Client.UIRootComponentSystem.<OpenPanelAsync>d__21<object,object,object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UIRootComponentSystem.<OpenPanelAsync>d__3<object,object>>(object&,RD.Client.UIRootComponentSystem.<OpenPanelAsync>d__3<object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UIRootComponentSystem.<OpenPanelAsync>d__4<object,object,object>>(object&,RD.Client.UIRootComponentSystem.<OpenPanelAsync>d__4<object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UIRootComponentSystem.<OpenPanelAsync>d__5<object,object,object,object>>(object&,RD.Client.UIRootComponentSystem.<OpenPanelAsync>d__5<object,object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UIRootComponentSystem.<OpenPanelAsync>d__6<object,object,object,object,object>>(object&,RD.Client.UIRootComponentSystem.<OpenPanelAsync>d__6<object,object,object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UIRootComponentSystem.<OpenPanelAsync>d__7<object,object,object,object,object,object>>(object&,RD.Client.UIRootComponentSystem.<OpenPanelAsync>d__7<object,object,object,object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UIRootComponentSystem.<OpenPanelParamAsync>d__16>(object&,RD.Client.UIRootComponentSystem.<OpenPanelParamAsync>d__16&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UIRootComponentSystem.<OpenPanelParamAsync>d__2<object>>(object&,RD.Client.UIRootComponentSystem.<OpenPanelParamAsync>d__2<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Client.UIYooAssetsSpriteComponentSystem.<GetSpriteAsync>d__3>(object&,RD.Client.UIYooAssetsSpriteComponentSystem.<GetSpriteAsync>d__3&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.CoroutineLockComponentSystem.<Wait>d__3>(object&,RD.CoroutineLockComponentSystem.<Wait>d__3&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.CoroutineLockQueueSystem.<Wait>d__2>(object&,RD.CoroutineLockQueueSystem.<Wait>d__2&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.CoroutineLockQueueTypeSystem.<Wait>d__4>(object&,RD.CoroutineLockQueueTypeSystem.<Wait>d__4&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.ObjectWaitSystem.<Wait>d__2<object>>(object&,RD.ObjectWaitSystem.<Wait>d__2<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.ProcessInnerSenderSystem.<Call>d__10>(object&,RD.ProcessInnerSenderSystem.<Call>d__10&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.PromiseCancellationTokenHelper.<AddCancel>d__2<object>>(object&,RD.PromiseCancellationTokenHelper.<AddCancel>d__2<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.PromiseCancellationTokenHelper.<TimeoutAsync>d__4<object>>(object&,RD.PromiseCancellationTokenHelper.<TimeoutAsync>d__4<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.PromiseCancellationTokenHelper.<TimeoutAsync>d__6<object>>(object&,RD.PromiseCancellationTokenHelper.<TimeoutAsync>d__6<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Server.GateMapFactory.<Create>d__0>(object&,RD.Server.GateMapFactory.<Create>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Server.MessageLocationSenderComponentSystem.<Call>d__10>(object&,RD.Server.MessageLocationSenderComponentSystem.<Call>d__10&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Server.MessageLocationSenderComponentSystem.<Call>d__8>(object&,RD.Server.MessageLocationSenderComponentSystem.<Call>d__8&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Server.MessageLocationSenderComponentSystem.<CallInner>d__11>(object&,RD.Server.MessageLocationSenderComponentSystem.<CallInner>d__11&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Server.MessageSenderSystem.<Call>d__2>(object&,RD.Server.MessageSenderSystem.<Call>d__2&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.Server.ProcessOuterSenderSystem.<Call>d__13>(object&,RD.Server.ProcessOuterSenderSystem.<Call>d__13&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.SessionSystem.<Call>d__3>(object&,RD.SessionSystem.<Call>d__3&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.AwaitUnsafeOnCompleted<object,RD.SessionSystem.<Call>d__4>(object&,RD.SessionSystem.<Call>d__4&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<uint>.AwaitUnsafeOnCompleted<object,RD.Client.RouterHelper.<Connect>d__2>(object&,RD.Client.RouterHelper.<Connect>d__2&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.A2NetClient_MessageHandler.<Run>d__0>(RD.Client.A2NetClient_MessageHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.A2NetClient_RequestHandler.<Run>d__0>(RD.Client.A2NetClient_RequestHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.AI_Attack.<Execute>d__1>(RD.Client.AI_Attack.<Execute>d__1&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.AI_Patrol.<Execute>d__1>(RD.Client.AI_Patrol.<Execute>d__1&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.AfterCreateClientScene_AddComponent.<Run>d__0>(RD.Client.AfterCreateClientScene_AddComponent.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.AfterCreateCurrentScene_AddComponent.<Run>d__0>(RD.Client.AfterCreateCurrentScene_AddComponent.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.AfterUnitCreate_CreateUnitView.<Run>d__0>(RD.Client.AfterUnitCreate_CreateUnitView.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.AppStartInitFinish_CreateLoginUI.<Run>d__0>(RD.Client.AppStartInitFinish_CreateLoginUI.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.ChangePosition_SyncGameObjectPos.<Run>d__0>(RD.Client.ChangePosition_SyncGameObjectPos.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.ChangeRotation_SyncGameObjectRotation.<Run>d__0>(RD.Client.ChangeRotation_SyncGameObjectRotation.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.ClientInitEvent.<Run>d__0>(RD.Client.ClientInitEvent.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.ClientSenderComponentSystem.<DisposeAsync>d__3>(RD.Client.ClientSenderComponentSystem.<DisposeAsync>d__3&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.ClientSenderComponentSystem.<RemoveFiberAsync>d__2>(RD.Client.ClientSenderComponentSystem.<RemoveFiberAsync>d__2&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.EnterMapHelper.<EnterMapAsync>d__0>(RD.Client.EnterMapHelper.<EnterMapAsync>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.LobbyPanelComponentSystem.<OnEventEnterMapInvoke>d__6>(RD.Client.LobbyPanelComponentSystem.<OnEventEnterMapInvoke>d__6&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.LoginFinish_CreateLobbyUI.<Run>d__0>(RD.Client.LoginFinish_CreateLobbyUI.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.LoginFinish_RemoveLoginUI.<Run>d__0>(RD.Client.LoginFinish_RemoveLoginUI.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.LoginHelper.<Login>d__0>(RD.Client.LoginHelper.<Login>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.LoginPanelComponentSystem.<DoLogin>d__9>(RD.Client.LoginPanelComponentSystem.<DoLogin>d__9&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.LoginPanelComponentSystem.<OnEventLoginInvoke>d__6>(RD.Client.LoginPanelComponentSystem.<OnEventLoginInvoke>d__6&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.LoginPanelComponentSystem.<OnEventQQInvoke>d__8>(RD.Client.LoginPanelComponentSystem.<OnEventQQInvoke>d__8&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.LoginPanelComponentSystem.<OnEventWeChatInvoke>d__7>(RD.Client.LoginPanelComponentSystem.<OnEventWeChatInvoke>d__7&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.M2C_CreateMyUnitHandler.<Run>d__0>(RD.Client.M2C_CreateMyUnitHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.M2C_CreateUnitsHandler.<Run>d__0>(RD.Client.M2C_CreateUnitsHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.M2C_PathfindingResultHandler.<Run>d__0>(RD.Client.M2C_PathfindingResultHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.M2C_RemoveUnitsHandler.<Run>d__0>(RD.Client.M2C_RemoveUnitsHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.M2C_StartSceneChangeHandler.<Run>d__0>(RD.Client.M2C_StartSceneChangeHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.M2C_StopHandler.<Run>d__0>(RD.Client.M2C_StopHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.Main2NetClient_LoginHandler.<Run>d__0>(RD.Client.Main2NetClient_LoginHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.MoveHelper.<MoveToAsync>d__1>(RD.Client.MoveHelper.<MoveToAsync>d__1&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.NetClient2Main_SessionDisposeHandler.<Run>d__0>(RD.Client.NetClient2Main_SessionDisposeHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.NetClientFiberInit.<Handle>d__0>(RD.Client.NetClientFiberInit.<Handle>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.On_UIEventInitializeBefore_CountDownHandler.<Run>d__0>(RD.Client.On_UIEventInitializeBefore_CountDownHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.On_UIEvent_YooAssetsLoad_Completed_SpriteHandler.<Run>d__0>(RD.Client.On_UIEvent_YooAssetsLoad_Completed_SpriteHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.OperaComponentSystem.<Test1>d__3>(RD.Client.OperaComponentSystem.<Test1>d__3&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.OperaComponentSystem.<Test2>d__4>(RD.Client.OperaComponentSystem.<Test2>d__4&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.OperaComponentSystem.<TestCancelAfter>d__5>(RD.Client.OperaComponentSystem.<TestCancelAfter>d__5&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.PingComponentSystem.<PingAsync>d__2>(RD.Client.PingComponentSystem.<PingAsync>d__2&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.ResourcesLoaderComponentSystem.<LoadSceneAsync>d__5>(RD.Client.ResourcesLoaderComponentSystem.<LoadSceneAsync>d__5&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.RouterAddressComponentSystem.<GetAllRouter>d__2>(RD.Client.RouterAddressComponentSystem.<GetAllRouter>d__2&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.RouterAddressComponentSystem.<Init>d__1>(RD.Client.RouterAddressComponentSystem.<Init>d__1&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.RouterAddressComponentSystem.<WaitTenMinGetAllRouter>d__3>(RD.Client.RouterAddressComponentSystem.<WaitTenMinGetAllRouter>d__3&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.RouterCheckComponentSystem.<CheckAsync>d__1>(RD.Client.RouterCheckComponentSystem.<CheckAsync>d__1&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.SceneChangeFinishEvent_CreateUIHelp.<Run>d__0>(RD.Client.SceneChangeFinishEvent_CreateUIHelp.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.SceneChangeHelper.<SceneChangeTo>d__0>(RD.Client.SceneChangeHelper.<SceneChangeTo>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.SceneChangeStart_AddComponent.<Run>d__0>(RD.Client.SceneChangeStart_AddComponent.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.UIChildSystem.<>c__DisplayClass12_0.<<OpenAllChild>g__Open|0>d>(RD.Client.UIChildSystem.<>c__DisplayClass12_0.<<OpenAllChild>g__Open|0>d&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.UIChildSystem.<>c__DisplayClass14_0.<<CloseAllChild>g__Close|0>d>(RD.Client.UIChildSystem.<>c__DisplayClass14_0.<<CloseAllChild>g__Close|0>d&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.UICloseCommonComponentSystem.<OnEventCloseInvoke>d__6>(RD.Client.UICloseCommonComponentSystem.<OnEventCloseInvoke>d__6&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.UIEventComponent.<Run>d__2<RD.Client.UIEventPanelCloseAfter>>(RD.Client.UIEventComponent.<Run>d__2<RD.Client.UIEventPanelCloseAfter>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.UIEventComponent.<Run>d__2<RD.Client.UIEventPanelCloseBefore>>(RD.Client.UIEventComponent.<Run>d__2<RD.Client.UIEventPanelCloseBefore>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.UIEventComponent.<Run>d__2<RD.Client.UIEventPanelDestroy>>(RD.Client.UIEventComponent.<Run>d__2<RD.Client.UIEventPanelDestroy>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.UIEventComponent.<Run>d__2<RD.Client.UIEventPanelOpenAfter>>(RD.Client.UIEventComponent.<Run>d__2<RD.Client.UIEventPanelOpenAfter>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.UIEventComponent.<Run>d__2<RD.Client.UIEventPanelOpenBefore>>(RD.Client.UIEventComponent.<Run>d__2<RD.Client.UIEventPanelOpenBefore>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.UIEventPanelCloseAfterHandler.<Run>d__0>(RD.Client.UIEventPanelCloseAfterHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.UIEventPanelCloseBeforeHandler.<Run>d__0>(RD.Client.UIEventPanelCloseBeforeHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.UIEventPanelDestroyHandler.<Run>d__0>(RD.Client.UIEventPanelDestroyHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.UIEventPanelOpenAfterHandler.<Run>d__0>(RD.Client.UIEventPanelOpenAfterHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.UIEventPanelOpenBeforeHandler.<Run>d__0>(RD.Client.UIEventPanelOpenBeforeHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.UIInvokeTimerComponentWaitAsyncHandler.<Handle>d__0>(RD.Client.UIInvokeTimerComponentWaitAsyncHandler.<Handle>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.UIInvokeTimerComponentWaitFrameAsyncHandler.<Handle>d__0>(RD.Client.UIInvokeTimerComponentWaitFrameAsyncHandler.<Handle>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.UIInvokeTimerComponentWaitSecondAsyncHandler.<Handle>d__0>(RD.Client.UIInvokeTimerComponentWaitSecondAsyncHandler.<Handle>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.UILoadComponentSystem.<LoadAssetAsyncAction>d__17>(RD.Client.UILoadComponentSystem.<LoadAssetAsyncAction>d__17&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.UILoadComponentSystem.<LoadAssetAsyncAction>d__5<object>>(RD.Client.UILoadComponentSystem.<LoadAssetAsyncAction>d__5<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.UILoadComponentSystem.<LoadAssetAsyncInstantiate>d__8>(RD.Client.UILoadComponentSystem.<LoadAssetAsyncInstantiate>d__8&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.UIMgrComponentSystem.<AddUICloseElse>d__12>(RD.Client.UIMgrComponentSystem.<AddUICloseElse>d__12&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.UIMgrComponentSystem.<BanLayerOptionCountDown>d__17>(RD.Client.UIMgrComponentSystem.<BanLayerOptionCountDown>d__17&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.UIMgrComponentSystem.<CloseAll>d__36>(RD.Client.UIMgrComponentSystem.<CloseAll>d__36&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.UIMgrComponentSystem.<OpenPanelAfter>d__49>(RD.Client.UIMgrComponentSystem.<OpenPanelAfter>d__49&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.UIMgrComponentSystem.<OpenPanelBefore>d__48>(RD.Client.UIMgrComponentSystem.<OpenPanelBefore>d__48&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.UIMgrComponentSystem.<RemoveUIAddElse>d__13>(RD.Client.UIMgrComponentSystem.<RemoveUIAddElse>d__13&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.UIPanelComponentSystem.<CloseAllView>d__49>(RD.Client.UIPanelComponentSystem.<CloseAllView>d__49&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.UIPanelComponentSystem.<CloseAllViewTween>d__50>(RD.Client.UIPanelComponentSystem.<CloseAllViewTween>d__50&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.UIPanelComponentSystem.<CloseLastView>d__48>(RD.Client.UIPanelComponentSystem.<CloseLastView>d__48&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.UIPanelComponentSystem.<DoCountDownDestroyPanel>d__36>(RD.Client.UIPanelComponentSystem.<DoCountDownDestroyPanel>d__36&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.UIPanelComponentSystem.<OpenAllViewTween>d__51>(RD.Client.UIPanelComponentSystem.<OpenAllViewTween>d__51&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.UIPanelComponentSystem.<OpenViewAfter>d__47>(RD.Client.UIPanelComponentSystem.<OpenViewAfter>d__47&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.UIPanelComponentSystem.<OpenViewBefore>d__46>(RD.Client.UIPanelComponentSystem.<OpenViewBefore>d__46&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.UIWaitComponentSystem.<UIWindowClose>d__2>(RD.Client.UIWaitComponentSystem.<UIWindowClose>d__2&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.UIWindowComponentSystem.<InternalOnWindowCloseTween>d__3>(RD.Client.UIWindowComponentSystem.<InternalOnWindowCloseTween>d__3&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.UIWindowComponentSystem.<InternalOnWindowOpenTween>d__2>(RD.Client.UIWindowComponentSystem.<InternalOnWindowOpenTween>d__2&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.UIWindowComponentSystem.<OnCloseTween>d__9>(RD.Client.UIWindowComponentSystem.<OnCloseTween>d__9&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.UIWindowComponentSystem.<OnOpenTween>d__8>(RD.Client.UIWindowComponentSystem.<OnOpenTween>d__8&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.UIWindowComponentSystem.<SealedOnWindowCloseTween>d__7>(RD.Client.UIWindowComponentSystem.<SealedOnWindowCloseTween>d__7&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.UIWindowComponentSystem.<SealedOnWindowOpenTween>d__6>(RD.Client.UIWindowComponentSystem.<SealedOnWindowOpenTween>d__6&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.WindowFadeAnim.<GetAwaiter>d__0>(RD.Client.WindowFadeAnim.<GetAwaiter>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.WindowFadeAnim.<In>d__1>(RD.Client.WindowFadeAnim.<In>d__1&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Client.WindowFadeAnim.<Out>d__2>(RD.Client.WindowFadeAnim.<Out>d__2&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Core.EntitySystemHelper.<DynamicEvent>d__0<RD.Client.UIEventPanelCloseAfter>>(RD.Core.EntitySystemHelper.<DynamicEvent>d__0<RD.Client.UIEventPanelCloseAfter>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Core.EntitySystemHelper.<DynamicEvent>d__0<RD.Client.UIEventPanelCloseBefore>>(RD.Core.EntitySystemHelper.<DynamicEvent>d__0<RD.Client.UIEventPanelCloseBefore>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Core.EntitySystemHelper.<DynamicEvent>d__0<RD.Client.UIEventPanelDestroy>>(RD.Core.EntitySystemHelper.<DynamicEvent>d__0<RD.Client.UIEventPanelDestroy>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Core.EntitySystemHelper.<DynamicEvent>d__0<RD.Client.UIEventPanelOpenAfter>>(RD.Core.EntitySystemHelper.<DynamicEvent>d__0<RD.Client.UIEventPanelOpenAfter>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Core.EntitySystemHelper.<DynamicEvent>d__0<RD.Client.UIEventPanelOpenBefore>>(RD.Core.EntitySystemHelper.<DynamicEvent>d__0<RD.Client.UIEventPanelOpenBefore>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Core.EventSystem.<PublishAsync>d__4<object,RD.AppStartInitFinish>>(RD.Core.EventSystem.<PublishAsync>d__4<object,RD.AppStartInitFinish>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Core.EventSystem.<PublishAsync>d__4<object,RD.Client.LoginFinish>>(RD.Core.EventSystem.<PublishAsync>d__4<object,RD.Client.LoginFinish>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Core.EventSystem.<PublishAsync>d__4<object,RD.Client.UIEventInitializeAfter>>(RD.Core.EventSystem.<PublishAsync>d__4<object,RD.Client.UIEventInitializeAfter>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Core.EventSystem.<PublishAsync>d__4<object,RD.Client.UIEventInitializeBefore>>(RD.Core.EventSystem.<PublishAsync>d__4<object,RD.Client.UIEventInitializeBefore>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Core.EventSystem.<PublishAsync>d__4<object,RD.Client.UIEventPanelCloseAfter>>(RD.Core.EventSystem.<PublishAsync>d__4<object,RD.Client.UIEventPanelCloseAfter>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Core.EventSystem.<PublishAsync>d__4<object,RD.Client.UIEventPanelCloseBefore>>(RD.Core.EventSystem.<PublishAsync>d__4<object,RD.Client.UIEventPanelCloseBefore>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Core.EventSystem.<PublishAsync>d__4<object,RD.Client.UIEventPanelOpenAfter>>(RD.Core.EventSystem.<PublishAsync>d__4<object,RD.Client.UIEventPanelOpenAfter>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Core.EventSystem.<PublishAsync>d__4<object,RD.Client.UIEventPanelOpenBefore>>(RD.Core.EventSystem.<PublishAsync>d__4<object,RD.Client.UIEventPanelOpenBefore>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Core.EventSystem.<PublishAsync>d__4<object,RD.Client.UIEventViewOpenBefore>>(RD.Core.EventSystem.<PublishAsync>d__4<object,RD.Client.UIEventViewOpenBefore>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Core.EventSystem.<PublishAsync>d__4<object,RD.Client.UIEvent_YooAssetsLoad_Completed>>(RD.Core.EventSystem.<PublishAsync>d__4<object,RD.Client.UIEvent_YooAssetsLoad_Completed>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.HashWaitSystem.<Notify>d__4>(RD.HashWaitSystem.<Notify>d__4&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.MailBoxType_UnOrderedMessageHandler.<HandleAsync>d__1>(RD.MailBoxType_UnOrderedMessageHandler.<HandleAsync>d__1&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.MessageHandler.<Handle>d__1<object,object,object>>(RD.MessageHandler.<Handle>d__1<object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.MessageHandler.<Handle>d__1<object,object>>(RD.MessageHandler.<Handle>d__1<object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.MessageSessionHandler.<HandleAsync>d__2<object,object>>(RD.MessageSessionHandler.<HandleAsync>d__2<object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.MessageSessionHandler.<HandleAsync>d__2<object>>(RD.MessageSessionHandler.<HandleAsync>d__2<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.NumericChangeEvent_NotifyWatcher.<Run>d__0>(RD.NumericChangeEvent_NotifyWatcher.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.ProcessInnerSenderSystem.<>c__DisplayClass10_0.<<Call>g__Timeout|0>d>(RD.ProcessInnerSenderSystem.<>c__DisplayClass10_0.<<Call>g__Timeout|0>d&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.PromiseCancellationTokenHelper.<AddCancel>d__1>(RD.PromiseCancellationTokenHelper.<AddCancel>d__1&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.PromiseCancellationTokenHelper.<TimeoutAsync>d__0>(RD.PromiseCancellationTokenHelper.<TimeoutAsync>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.PromiseCancellationTokenHelper.<TimeoutAsync>d__3>(RD.PromiseCancellationTokenHelper.<TimeoutAsync>d__3&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.PromiseCancellationTokenHelper.<TimeoutAsync>d__5>(RD.PromiseCancellationTokenHelper.<TimeoutAsync>d__5&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.PromiseHelperExtend.<WaitUntil>d__0>(RD.PromiseHelperExtend.<WaitUntil>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.PromiseHelperExtend.<WaitUntil>d__1>(RD.PromiseHelperExtend.<WaitUntil>d__1&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Server.A2NetInner_MessageHandler.<Run>d__0>(RD.Server.A2NetInner_MessageHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Server.A2NetInner_RequestHandler.<Run>d__0>(RD.Server.A2NetInner_RequestHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Server.C2G_EnterMapHandler.<Run>d__0>(RD.Server.C2G_EnterMapHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Server.C2G_LoginGateHandler.<Run>d__0>(RD.Server.C2G_LoginGateHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Server.C2G_PingHandler.<Run>d__0>(RD.Server.C2G_PingHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Server.C2M_PathfindingResultHandler.<Run>d__0>(RD.Server.C2M_PathfindingResultHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Server.C2M_StopHandler.<Run>d__0>(RD.Server.C2M_StopHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Server.C2M_TestRobotCaseHandler.<Run>d__0>(RD.Server.C2M_TestRobotCaseHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Server.C2M_TransferMapHandler.<Run>d__0>(RD.Server.C2M_TransferMapHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Server.C2R_LoginHandler.<CloseSession>d__1>(RD.Server.C2R_LoginHandler.<CloseSession>d__1&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Server.C2R_LoginHandler.<Run>d__0>(RD.Server.C2R_LoginHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Server.ChangePosition_NotifyAOI.<Run>d__0>(RD.Server.ChangePosition_NotifyAOI.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Server.ConsoleComponentSystem.<Start>d__1>(RD.Server.ConsoleComponentSystem.<Start>d__1&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Server.CreateRobotConsoleHandler.<Run>d__0>(RD.Server.CreateRobotConsoleHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Server.G2M_SessionDisconnectHandler.<Run>d__0>(RD.Server.G2M_SessionDisconnectHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Server.GateFiberInit.<Handle>d__0>(RD.Server.GateFiberInit.<Handle>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Server.GateSessionKeyComponentSystem.<TimeoutRemoveKey>d__3>(RD.Server.GateSessionKeyComponentSystem.<TimeoutRemoveKey>d__3&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Server.HttpComponentSystem.<Accept>d__2>(RD.Server.HttpComponentSystem.<Accept>d__2&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Server.HttpComponentSystem.<Handle>d__3>(RD.Server.HttpComponentSystem.<Handle>d__3&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Server.HttpGetRouterHandler.<Handle>d__0>(RD.Server.HttpGetRouterHandler.<Handle>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Server.LocationFiberInit.<Handle>d__0>(RD.Server.LocationFiberInit.<Handle>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Server.LocationOneTypeSystem.<>c__DisplayClass3_0.<<Lock>g__TimeWaitAsync|0>d>(RD.Server.LocationOneTypeSystem.<>c__DisplayClass3_0.<<Lock>g__TimeWaitAsync|0>d&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Server.LocationOneTypeSystem.<Add>d__1>(RD.Server.LocationOneTypeSystem.<Add>d__1&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Server.LocationOneTypeSystem.<Lock>d__3>(RD.Server.LocationOneTypeSystem.<Lock>d__3&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Server.LocationOneTypeSystem.<Remove>d__2>(RD.Server.LocationOneTypeSystem.<Remove>d__2&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Server.LocationProxyComponentSystem.<Add>d__1>(RD.Server.LocationProxyComponentSystem.<Add>d__1&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Server.LocationProxyComponentSystem.<AddLocation>d__6>(RD.Server.LocationProxyComponentSystem.<AddLocation>d__6&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Server.LocationProxyComponentSystem.<Lock>d__2>(RD.Server.LocationProxyComponentSystem.<Lock>d__2&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Server.LocationProxyComponentSystem.<Remove>d__4>(RD.Server.LocationProxyComponentSystem.<Remove>d__4&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Server.LocationProxyComponentSystem.<RemoveLocation>d__7>(RD.Server.LocationProxyComponentSystem.<RemoveLocation>d__7&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Server.LocationProxyComponentSystem.<UnLock>d__3>(RD.Server.LocationProxyComponentSystem.<UnLock>d__3&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Server.M2M_UnitTransferRequestHandler.<Run>d__0>(RD.Server.M2M_UnitTransferRequestHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Server.MailBoxType_OrderedMessageHandler.<HandleInner>d__1>(RD.Server.MailBoxType_OrderedMessageHandler.<HandleInner>d__1&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Server.MapFiberInit.<Handle>d__0>(RD.Server.MapFiberInit.<Handle>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Server.MessageLocationHandler.<Handle>d__1<object,object,object>>(RD.Server.MessageLocationHandler.<Handle>d__1<object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Server.MessageLocationHandler.<Handle>d__1<object,object>>(RD.Server.MessageLocationHandler.<Handle>d__1<object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Server.MessageLocationSenderComponentSystem.<SendInner>d__7>(RD.Server.MessageLocationSenderComponentSystem.<SendInner>d__7&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Server.MoveHelper.<FindPathMoveToAsync>d__0>(RD.Server.MoveHelper.<FindPathMoveToAsync>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Server.NetComponentOnReadInvoker_Gate.<HandleAsync>d__1>(RD.Server.NetComponentOnReadInvoker_Gate.<HandleAsync>d__1&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Server.NetInnerFiberInit.<Handle>d__0>(RD.Server.NetInnerFiberInit.<Handle>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Server.ObjectAddRequestHandler.<Run>d__0>(RD.Server.ObjectAddRequestHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Server.ObjectGetRequestHandler.<Run>d__0>(RD.Server.ObjectGetRequestHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Server.ObjectLockRequestHandler.<Run>d__0>(RD.Server.ObjectLockRequestHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Server.ObjectRemoveRequestHandler.<Run>d__0>(RD.Server.ObjectRemoveRequestHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Server.ObjectUnLockRequestHandler.<Run>d__0>(RD.Server.ObjectUnLockRequestHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Server.ProcessOuterSenderSystem.<>c__DisplayClass13_0.<<Call>g__Timeout|0>d>(RD.Server.ProcessOuterSenderSystem.<>c__DisplayClass13_0.<<Call>g__Timeout|0>d&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Server.ProcessOuterSenderSystem.<>c__DisplayClass3_0.<<OnRead>g__CallInner|0>d>(RD.Server.ProcessOuterSenderSystem.<>c__DisplayClass3_0.<<OnRead>g__CallInner|0>d&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Server.ProfilerConsoleHandler.<Run>d__0>(RD.Server.ProfilerConsoleHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Server.R2G_GetLoginKeyHandler.<Run>d__0>(RD.Server.R2G_GetLoginKeyHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Server.RealmFiberInit.<Handle>d__0>(RD.Server.RealmFiberInit.<Handle>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Server.ReloadConfigConsoleHandler.<Run>d__0>(RD.Server.ReloadConfigConsoleHandler.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Server.RobotFiberInit.<Handle>d__0>(RD.Server.RobotFiberInit.<Handle>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Server.RobotManagerComponentSystem.<<Destroy>g__Remove|1_0>d>(RD.Server.RobotManagerComponentSystem.<<Destroy>g__Remove|1_0>d&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Server.RobotManagerComponentSystem.<NewRobot>d__2>(RD.Server.RobotManagerComponentSystem.<NewRobot>d__2&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Server.RouterFiberInit.<Handle>d__0>(RD.Server.RouterFiberInit.<Handle>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Server.RouterManagerFiberInit.<Handle>d__0>(RD.Server.RouterManagerFiberInit.<Handle>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Server.ServerInitEvent.<Run>d__0>(RD.Server.ServerInitEvent.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Server.TransferHelper.<Transfer>d__1>(RD.Server.TransferHelper.<Transfer>d__1&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Server.TransferHelper.<TransferAtFrameFinish>d__0>(RD.Server.TransferHelper.<TransferAtFrameFinish>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Server.UnitEnterSightRange_NotifyClient.<Run>d__0>(RD.Server.UnitEnterSightRange_NotifyClient.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.Server.UnitLeaveSightRange_NotifyClient.<Run>d__0>(RD.Server.UnitLeaveSightRange_NotifyClient.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.ShareInitEvent.<Run>d__0>(RD.ShareInitEvent.<Run>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.TimerComponentSystem.<WaitAsync>d__10>(RD.TimerComponentSystem.<WaitAsync>d__10&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.TimerComponentSystem.<WaitFrameAsync>d__9>(RD.TimerComponentSystem.<WaitFrameAsync>d__9&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder.Start<RD.TimerComponentSystem.<WaitTillAsync>d__8>(RD.TimerComponentSystem.<WaitTillAsync>d__8&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<RD.Client.Wait_CreateMyUnit>.Start<RD.ObjectWaitSystem.<Wait>d__2<RD.Client.Wait_CreateMyUnit>>(RD.ObjectWaitSystem.<Wait>d__2<RD.Client.Wait_CreateMyUnit>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<RD.Client.Wait_SceneChangeFinish>.Start<RD.ObjectWaitSystem.<Wait>d__2<RD.Client.Wait_SceneChangeFinish>>(RD.ObjectWaitSystem.<Wait>d__2<RD.Client.Wait_SceneChangeFinish>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<RD.Client.Wait_UnitStop>.Start<RD.ObjectWaitSystem.<Wait>d__2<RD.Client.Wait_UnitStop>>(RD.ObjectWaitSystem.<Wait>d__2<RD.Client.Wait_UnitStop>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<RD.Core.ActorId>.Start<RD.Server.LocationOneTypeSystem.<Get>d__5>(RD.Server.LocationOneTypeSystem.<Get>d__5&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<RD.Core.ActorId>.Start<RD.Server.LocationProxyComponentSystem.<Get>d__5>(RD.Server.LocationProxyComponentSystem.<Get>d__5&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<System.ValueTuple<object,int>>.Start<RD.Client.UIYooAssetsLoadComponentSystem.<LoadAssetAsyncFunc>d__4>(RD.Client.UIYooAssetsLoadComponentSystem.<LoadAssetAsyncFunc>d__4&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<System.ValueTuple<uint,object>>.Start<RD.Client.RouterHelper.<GetRouterAddress>d__1>(RD.Client.RouterHelper.<GetRouterAddress>d__1&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.Start<RD.Client.LobbyPanelComponentSystem.<UIOpen>d__5>(RD.Client.LobbyPanelComponentSystem.<UIOpen>d__5&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.Start<RD.Client.LoginPanelComponentSystem.<UIOpen>d__5>(RD.Client.LoginPanelComponentSystem.<UIOpen>d__5&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.Start<RD.Client.MainPanelComponentSystem.<UIOpen>d__5>(RD.Client.MainPanelComponentSystem.<UIOpen>d__5&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.Start<RD.Client.UIChildSystem.<CloseAllChild>d__14>(RD.Client.UIChildSystem.<CloseAllChild>d__14&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.Start<RD.Client.UIChildSystem.<OpenAllChild>d__12>(RD.Client.UIChildSystem.<OpenAllChild>d__12&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.Start<RD.Client.UICloseCommonComponentSystem.<CloseUI>d__5>(RD.Client.UICloseCommonComponentSystem.<CloseUI>d__5&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.Start<RD.Client.UIEventSystem.<Open>d__13<object>>(RD.Client.UIEventSystem.<Open>d__13<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.Start<RD.Client.UIEventSystem.<Open>d__14<object,object>>(RD.Client.UIEventSystem.<Open>d__14<object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.Start<RD.Client.UIEventSystem.<Open>d__15<object,object,object>>(RD.Client.UIEventSystem.<Open>d__15<object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.Start<RD.Client.UIEventSystem.<Open>d__16<object,object,object,object>>(RD.Client.UIEventSystem.<Open>d__16<object,object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.Start<RD.Client.UIEventSystem.<Open>d__17<object,object,object,object,object>>(RD.Client.UIEventSystem.<Open>d__17<object,object,object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.Start<RD.Client.UIInvokeClosePanelAsyncHandler.<Handle>d__0>(RD.Client.UIInvokeClosePanelAsyncHandler.<Handle>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.Start<RD.Client.UIInvokeHomePanelWaitHandler.<Handle>d__0>(RD.Client.UIInvokeHomePanelWaitHandler.<Handle>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.Start<RD.Client.UIInvokeRootOpenPanelAsyncHandler.<Handle>d__0>(RD.Client.UIInvokeRootOpenPanelAsyncHandler.<Handle>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.Start<RD.Client.UIInvokeViewClosePanelAsyncHandler.<Handle>d__0>(RD.Client.UIInvokeViewClosePanelAsyncHandler.<Handle>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.Start<RD.Client.UIInvokeYooAssetsHandler.<Handle>d__0>(RD.Client.UIInvokeYooAssetsHandler.<Handle>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.Start<RD.Client.UILoadComponentSystem.<Initialize>d__2>(RD.Client.UILoadComponentSystem.<Initialize>d__2&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.Start<RD.Client.UIMgrComponentSystem.<CloseLayerTopPanelAsync>d__33>(RD.Client.UIMgrComponentSystem.<CloseLayerTopPanelAsync>d__33&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.Start<RD.Client.UIMgrComponentSystem.<ClosePanelAsync>d__25>(RD.Client.UIMgrComponentSystem.<ClosePanelAsync>d__25&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.Start<RD.Client.UIMgrComponentSystem.<ClosePanelAsync>d__28<object>>(RD.Client.UIMgrComponentSystem.<ClosePanelAsync>d__28<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.Start<RD.Client.UIMgrComponentSystem.<CloseTopPanelAsync>d__35>(RD.Client.UIMgrComponentSystem.<CloseTopPanelAsync>d__35&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.Start<RD.Client.UIMgrComponentSystem.<HomePanel>d__30>(RD.Client.UIMgrComponentSystem.<HomePanel>d__30&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.Start<RD.Client.UIMgrComponentSystem.<HomePanel>d__31<object>>(RD.Client.UIMgrComponentSystem.<HomePanel>d__31<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.Start<RD.Client.UIMgrComponentSystem.<InitRoot>d__84>(RD.Client.UIMgrComponentSystem.<InitRoot>d__84&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.Start<RD.Client.UIMgrComponentSystem.<Initialize>d__43>(RD.Client.UIMgrComponentSystem.<Initialize>d__43&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.Start<RD.Client.UIMgrComponentSystem.<PreLoadPanelAsync>d__79<object>>(RD.Client.UIMgrComponentSystem.<PreLoadPanelAsync>d__79<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.Start<RD.Client.UIMgrComponentSystem.<PreLoadPanelAsync>d__80>(RD.Client.UIMgrComponentSystem.<PreLoadPanelAsync>d__80&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.Start<RD.Client.UIMgrComponentSystem.<RemoveUIToHome>d__14>(RD.Client.UIMgrComponentSystem.<RemoveUIToHome>d__14&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.Start<RD.Client.UIPanelComponentSystem.<CloseAsync>d__4>(RD.Client.UIPanelComponentSystem.<CloseAsync>d__4&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.Start<RD.Client.UIPanelComponentSystem.<CloseViewAsync>d__10>(RD.Client.UIPanelComponentSystem.<CloseViewAsync>d__10&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.Start<RD.Client.UIPanelComponentSystem.<CloseViewAsync>d__11>(RD.Client.UIPanelComponentSystem.<CloseViewAsync>d__11&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.Start<RD.Client.UIPanelComponentSystem.<CloseViewAsync>d__9<object>>(RD.Client.UIPanelComponentSystem.<CloseViewAsync>d__9<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.Start<RD.Client.UIPanelComponentSystem.<Home>d__5<object>>(RD.Client.UIPanelComponentSystem.<Home>d__5<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.Start<RD.Client.UIPanelComponentSystem.<Home>d__6>(RD.Client.UIPanelComponentSystem.<Home>d__6&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.Start<RD.Client.UIPanelComponentSystem.<Open>d__27>(RD.Client.UIPanelComponentSystem.<Open>d__27&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.Start<RD.Client.UIPanelComponentSystem.<Open>d__28>(RD.Client.UIPanelComponentSystem.<Open>d__28&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.Start<RD.Client.UIPanelComponentSystem.<Open>d__29<object>>(RD.Client.UIPanelComponentSystem.<Open>d__29<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.Start<RD.Client.UIPanelComponentSystem.<Open>d__30<object,object>>(RD.Client.UIPanelComponentSystem.<Open>d__30<object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.Start<RD.Client.UIPanelComponentSystem.<Open>d__31<object,object,object>>(RD.Client.UIPanelComponentSystem.<Open>d__31<object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.Start<RD.Client.UIPanelComponentSystem.<Open>d__32<object,object,object,object>>(RD.Client.UIPanelComponentSystem.<Open>d__32<object,object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.Start<RD.Client.UIPanelComponentSystem.<Open>d__33<object,object,object,object,object>>(RD.Client.UIPanelComponentSystem.<Open>d__33<object,object,object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.Start<RD.Client.UIPanelComponentSystem.<UseBaseOpen>d__26>(RD.Client.UIPanelComponentSystem.<UseBaseOpen>d__26&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.Start<RD.Client.UIRootComponentSystem.<PreLoadPanelAsync>d__29<object>>(RD.Client.UIRootComponentSystem.<PreLoadPanelAsync>d__29<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.Start<RD.Client.UIRootComponentSystem.<PreLoadPanelAsync>d__30>(RD.Client.UIRootComponentSystem.<PreLoadPanelAsync>d__30&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.Start<RD.Client.UIViewComponentSystem.<CloseAsync>d__4>(RD.Client.UIViewComponentSystem.<CloseAsync>d__4&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.Start<RD.Client.UIViewComponentSystem.<ClosePanelAsync>d__6>(RD.Client.UIViewComponentSystem.<ClosePanelAsync>d__6&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.Start<RD.Client.UIViewComponentSystem.<Open>d__11>(RD.Client.UIViewComponentSystem.<Open>d__11&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.Start<RD.Client.UIViewComponentSystem.<Open>d__12>(RD.Client.UIViewComponentSystem.<Open>d__12&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.Start<RD.Client.UIViewComponentSystem.<Open>d__13<object>>(RD.Client.UIViewComponentSystem.<Open>d__13<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.Start<RD.Client.UIViewComponentSystem.<Open>d__14<object,object>>(RD.Client.UIViewComponentSystem.<Open>d__14<object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.Start<RD.Client.UIViewComponentSystem.<Open>d__15<object,object,object>>(RD.Client.UIViewComponentSystem.<Open>d__15<object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.Start<RD.Client.UIViewComponentSystem.<Open>d__16<object,object,object,object>>(RD.Client.UIViewComponentSystem.<Open>d__16<object,object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.Start<RD.Client.UIViewComponentSystem.<Open>d__17<object,object,object,object,object>>(RD.Client.UIViewComponentSystem.<Open>d__17<object,object,object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.Start<RD.Client.UIViewComponentSystem.<UseBaseOpen>d__10>(RD.Client.UIViewComponentSystem.<UseBaseOpen>d__10&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.Start<RD.Client.UIYooAssetsLoadComponentSystem.<Initialize>d__2>(RD.Client.UIYooAssetsLoadComponentSystem.<Initialize>d__2&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<byte>.Start<RD.MoveComponentSystem.<MoveToAsync>d__5>(RD.MoveComponentSystem.<MoveToAsync>d__5&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<int>.Start<RD.Client.MoveHelper.<MoveToAsync>d__0>(RD.Client.MoveHelper.<MoveToAsync>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<int>.Start<RD.Client.UIMgrComponentSystem.<OpenPanelWaitAsync>d__58<object>>(RD.Client.UIMgrComponentSystem.<OpenPanelWaitAsync>d__58<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<int>.Start<RD.Client.UIMgrComponentSystem.<OpenPanelWaitAsync>d__60<object,object>>(RD.Client.UIMgrComponentSystem.<OpenPanelWaitAsync>d__60<object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<int>.Start<RD.Client.UIMgrComponentSystem.<OpenPanelWaitAsync>d__61<object,object,object>>(RD.Client.UIMgrComponentSystem.<OpenPanelWaitAsync>d__61<object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<int>.Start<RD.Client.UIMgrComponentSystem.<OpenPanelWaitAsync>d__62<object,object,object,object>>(RD.Client.UIMgrComponentSystem.<OpenPanelWaitAsync>d__62<object,object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<int>.Start<RD.Client.UIMgrComponentSystem.<OpenPanelWaitAsync>d__63<object,object,object,object,object>>(RD.Client.UIMgrComponentSystem.<OpenPanelWaitAsync>d__63<object,object,object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<int>.Start<RD.Client.UIMgrComponentSystem.<OpenPanelWaitAsync>d__64<object,object,object,object,object,object>>(RD.Client.UIMgrComponentSystem.<OpenPanelWaitAsync>d__64<object,object,object,object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<int>.Start<RD.Client.UIMgrComponentSystem.<OpenPanelWaitAsync>d__72>(RD.Client.UIMgrComponentSystem.<OpenPanelWaitAsync>d__72&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<int>.Start<RD.Client.UIMgrComponentSystem.<OpenPanelWaitAsync>d__74<object>>(RD.Client.UIMgrComponentSystem.<OpenPanelWaitAsync>d__74<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<int>.Start<RD.Client.UIMgrComponentSystem.<OpenPanelWaitAsync>d__75<object,object>>(RD.Client.UIMgrComponentSystem.<OpenPanelWaitAsync>d__75<object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<int>.Start<RD.Client.UIMgrComponentSystem.<OpenPanelWaitAsync>d__76<object,object,object>>(RD.Client.UIMgrComponentSystem.<OpenPanelWaitAsync>d__76<object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<int>.Start<RD.Client.UIMgrComponentSystem.<OpenPanelWaitAsync>d__77<object,object,object,object>>(RD.Client.UIMgrComponentSystem.<OpenPanelWaitAsync>d__77<object,object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<int>.Start<RD.Client.UIMgrComponentSystem.<OpenPanelWaitAsync>d__78<object,object,object,object,object>>(RD.Client.UIMgrComponentSystem.<OpenPanelWaitAsync>d__78<object,object,object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<int>.Start<RD.Client.UIMgrComponentSystem.<OpenPanelWaitParamAsync>d__59<object>>(RD.Client.UIMgrComponentSystem.<OpenPanelWaitParamAsync>d__59<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<int>.Start<RD.Client.UIMgrComponentSystem.<OpenPanelWaitParamAsync>d__73>(RD.Client.UIMgrComponentSystem.<OpenPanelWaitParamAsync>d__73&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<int>.Start<RD.Client.UIMgrComponentSystem.<PanelWait>d__57>(RD.Client.UIMgrComponentSystem.<PanelWait>d__57&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<int>.Start<RD.Client.UIRootComponentSystem.<OpenPanelWaitAsync>d__10<object,object>>(RD.Client.UIRootComponentSystem.<OpenPanelWaitAsync>d__10<object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<int>.Start<RD.Client.UIRootComponentSystem.<OpenPanelWaitAsync>d__11<object,object,object>>(RD.Client.UIRootComponentSystem.<OpenPanelWaitAsync>d__11<object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<int>.Start<RD.Client.UIRootComponentSystem.<OpenPanelWaitAsync>d__12<object,object,object,object>>(RD.Client.UIRootComponentSystem.<OpenPanelWaitAsync>d__12<object,object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<int>.Start<RD.Client.UIRootComponentSystem.<OpenPanelWaitAsync>d__13<object,object,object,object,object>>(RD.Client.UIRootComponentSystem.<OpenPanelWaitAsync>d__13<object,object,object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<int>.Start<RD.Client.UIRootComponentSystem.<OpenPanelWaitAsync>d__14<object,object,object,object,object,object>>(RD.Client.UIRootComponentSystem.<OpenPanelWaitAsync>d__14<object,object,object,object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<int>.Start<RD.Client.UIRootComponentSystem.<OpenPanelWaitAsync>d__22>(RD.Client.UIRootComponentSystem.<OpenPanelWaitAsync>d__22&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<int>.Start<RD.Client.UIRootComponentSystem.<OpenPanelWaitAsync>d__24<object>>(RD.Client.UIRootComponentSystem.<OpenPanelWaitAsync>d__24<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<int>.Start<RD.Client.UIRootComponentSystem.<OpenPanelWaitAsync>d__25<object,object>>(RD.Client.UIRootComponentSystem.<OpenPanelWaitAsync>d__25<object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<int>.Start<RD.Client.UIRootComponentSystem.<OpenPanelWaitAsync>d__26<object,object,object>>(RD.Client.UIRootComponentSystem.<OpenPanelWaitAsync>d__26<object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<int>.Start<RD.Client.UIRootComponentSystem.<OpenPanelWaitAsync>d__27<object,object,object,object>>(RD.Client.UIRootComponentSystem.<OpenPanelWaitAsync>d__27<object,object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<int>.Start<RD.Client.UIRootComponentSystem.<OpenPanelWaitAsync>d__28<object,object,object,object,object>>(RD.Client.UIRootComponentSystem.<OpenPanelWaitAsync>d__28<object,object,object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<int>.Start<RD.Client.UIRootComponentSystem.<OpenPanelWaitAsync>d__8<object>>(RD.Client.UIRootComponentSystem.<OpenPanelWaitAsync>d__8<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<int>.Start<RD.Client.UIRootComponentSystem.<OpenPanelWaitParamAsync>d__23>(RD.Client.UIRootComponentSystem.<OpenPanelWaitParamAsync>d__23&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<int>.Start<RD.Client.UIRootComponentSystem.<OpenPanelWaitParamAsync>d__9<object>>(RD.Client.UIRootComponentSystem.<OpenPanelWaitParamAsync>d__9<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<int>.Start<RD.HashWaitSystem.<Wait>d__2>(RD.HashWaitSystem.<Wait>d__2&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<int>.Start<RD.HashWaitSystem.<WaitObject>d__5>(RD.HashWaitSystem.<WaitObject>d__5&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<long>.Start<RD.Client.ClientSenderComponentSystem.<LoginAsync>d__4>(RD.Client.ClientSenderComponentSystem.<LoginAsync>d__4&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.ClientSenderComponentSystem.<Call>d__6>(RD.Client.ClientSenderComponentSystem.<Call>d__6&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.GetAllConfigBytes.<Handle>d__0>(RD.Client.GetAllConfigBytes.<Handle>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.GetOneConfigBytes.<Handle>d__0>(RD.Client.GetOneConfigBytes.<Handle>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.ResourcesLoaderComponentSystem.<LoadAllAssetsAsync>d__4<object>>(RD.Client.ResourcesLoaderComponentSystem.<LoadAllAssetsAsync>d__4<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.ResourcesLoaderComponentSystem.<LoadAssetAsync>d__3<object>>(RD.Client.ResourcesLoaderComponentSystem.<LoadAssetAsync>d__3<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.RouterHelper.<CreateRouterSession>d__0>(RD.Client.RouterHelper.<CreateRouterSession>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UIFactory.<CreateAsync>d__13>(RD.Client.UIFactory.<CreateAsync>d__13&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UIFactory.<CreateGameObjectAsync>d__15>(RD.Client.UIFactory.<CreateGameObjectAsync>d__15&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UIFactory.<CreatePanelAsync>d__12>(RD.Client.UIFactory.<CreatePanelAsync>d__12&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UIFactory.<CreatePanelGameObjectAsync>d__14>(RD.Client.UIFactory.<CreatePanelGameObjectAsync>d__14&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UIFactory.<InstantiateAsync>d__10>(RD.Client.UIFactory.<InstantiateAsync>d__10&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UIFactory.<InstantiateAsync>d__11>(RD.Client.UIFactory.<InstantiateAsync>d__11&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UIFactory.<InstantiateAsync>d__6<object>>(RD.Client.UIFactory.<InstantiateAsync>d__6<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UIFactory.<InstantiateAsync>d__7<object>>(RD.Client.UIFactory.<InstantiateAsync>d__7<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UIFactory.<InstantiateAsync>d__8>(RD.Client.UIFactory.<InstantiateAsync>d__8&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UIFactory.<InstantiateAsync>d__9>(RD.Client.UIFactory.<InstantiateAsync>d__9&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UIFactory.<InstantiateGameObjectAsync>d__1>(RD.Client.UIFactory.<InstantiateGameObjectAsync>d__1&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UIFactory.<InstantiateGameObjectAsync>d__27>(RD.Client.UIFactory.<InstantiateGameObjectAsync>d__27&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UIInvokeCoroutineLockHandler.<Handle>d__0>(RD.Client.UIInvokeCoroutineLockHandler.<Handle>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UIInvokeInstantiateGameObjectAsyncHandler.<Handle>d__0>(RD.Client.UIInvokeInstantiateGameObjectAsyncHandler.<Handle>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UIInvokeLoadAsyncHandler.<Handle>d__0>(RD.Client.UIInvokeLoadAsyncHandler.<Handle>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UIInvokeLoadInstantiateAsyncHandler.<Handle>d__0>(RD.Client.UIInvokeLoadInstantiateAsyncHandler.<Handle>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UIInvokeLoadInstantiateByVoAsyncHandler.<Handle>d__0>(RD.Client.UIInvokeLoadInstantiateByVoAsyncHandler.<Handle>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UIInvokeLoadSpriteAsyncHandler.<Handle>d__0>(RD.Client.UIInvokeLoadSpriteAsyncHandler.<Handle>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UIInvokeLoadTexture2DAsyncHandler.<Handle>d__0>(RD.Client.UIInvokeLoadTexture2DAsyncHandler.<Handle>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UILoadComponentSystem.<LoadAssetAsync>d__15>(RD.Client.UILoadComponentSystem.<LoadAssetAsync>d__15&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UILoadComponentSystem.<LoadAssetAsync>d__19<object>>(RD.Client.UILoadComponentSystem.<LoadAssetAsync>d__19<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UILoadComponentSystem.<LoadAssetAsync>d__23>(RD.Client.UILoadComponentSystem.<LoadAssetAsync>d__23&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UILoadComponentSystem.<LoadAssetAsync>d__3<object>>(RD.Client.UILoadComponentSystem.<LoadAssetAsync>d__3<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UILoadComponentSystem.<LoadAssetAsyncInstantiate>d__7>(RD.Client.UILoadComponentSystem.<LoadAssetAsyncInstantiate>d__7&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UIMgrComponentSystem.<OpenPanelAsync>d__50<object>>(RD.Client.UIMgrComponentSystem.<OpenPanelAsync>d__50<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UIMgrComponentSystem.<OpenPanelAsync>d__52<object,object>>(RD.Client.UIMgrComponentSystem.<OpenPanelAsync>d__52<object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UIMgrComponentSystem.<OpenPanelAsync>d__53<object,object,object>>(RD.Client.UIMgrComponentSystem.<OpenPanelAsync>d__53<object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UIMgrComponentSystem.<OpenPanelAsync>d__54<object,object,object,object>>(RD.Client.UIMgrComponentSystem.<OpenPanelAsync>d__54<object,object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UIMgrComponentSystem.<OpenPanelAsync>d__55<object,object,object,object,object>>(RD.Client.UIMgrComponentSystem.<OpenPanelAsync>d__55<object,object,object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UIMgrComponentSystem.<OpenPanelAsync>d__56<object,object,object,object,object,object>>(RD.Client.UIMgrComponentSystem.<OpenPanelAsync>d__56<object,object,object,object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UIMgrComponentSystem.<OpenPanelAsync>d__65>(RD.Client.UIMgrComponentSystem.<OpenPanelAsync>d__65&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UIMgrComponentSystem.<OpenPanelAsync>d__67<object>>(RD.Client.UIMgrComponentSystem.<OpenPanelAsync>d__67<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UIMgrComponentSystem.<OpenPanelAsync>d__68<object,object>>(RD.Client.UIMgrComponentSystem.<OpenPanelAsync>d__68<object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UIMgrComponentSystem.<OpenPanelAsync>d__69<object,object,object>>(RD.Client.UIMgrComponentSystem.<OpenPanelAsync>d__69<object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UIMgrComponentSystem.<OpenPanelAsync>d__70<object,object,object,object>>(RD.Client.UIMgrComponentSystem.<OpenPanelAsync>d__70<object,object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UIMgrComponentSystem.<OpenPanelAsync>d__71<object,object,object,object,object>>(RD.Client.UIMgrComponentSystem.<OpenPanelAsync>d__71<object,object,object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UIMgrComponentSystem.<OpenPanelParamAsync>d__51<object>>(RD.Client.UIMgrComponentSystem.<OpenPanelParamAsync>d__51<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UIMgrComponentSystem.<OpenPanelParamAsync>d__66>(RD.Client.UIMgrComponentSystem.<OpenPanelParamAsync>d__66&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UIMgrComponentSystem.<OpenPanelStartAsync>d__47>(RD.Client.UIMgrComponentSystem.<OpenPanelStartAsync>d__47&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UIPanelComponentSystem.<GetView>d__42>(RD.Client.UIPanelComponentSystem.<GetView>d__42&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UIPanelComponentSystem.<GetView>d__43<object>>(RD.Client.UIPanelComponentSystem.<GetView>d__43<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UIPanelComponentSystem.<OpenViewAsync>d__12<object>>(RD.Client.UIPanelComponentSystem.<OpenViewAsync>d__12<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UIPanelComponentSystem.<OpenViewAsync>d__14<object,object>>(RD.Client.UIPanelComponentSystem.<OpenViewAsync>d__14<object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UIPanelComponentSystem.<OpenViewAsync>d__15<object,object,object>>(RD.Client.UIPanelComponentSystem.<OpenViewAsync>d__15<object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UIPanelComponentSystem.<OpenViewAsync>d__16<object,object,object,object>>(RD.Client.UIPanelComponentSystem.<OpenViewAsync>d__16<object,object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UIPanelComponentSystem.<OpenViewAsync>d__17<object,object,object,object,object>>(RD.Client.UIPanelComponentSystem.<OpenViewAsync>d__17<object,object,object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UIPanelComponentSystem.<OpenViewAsync>d__18<object,object,object,object,object,object>>(RD.Client.UIPanelComponentSystem.<OpenViewAsync>d__18<object,object,object,object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UIPanelComponentSystem.<OpenViewAsync>d__19>(RD.Client.UIPanelComponentSystem.<OpenViewAsync>d__19&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UIPanelComponentSystem.<OpenViewAsync>d__21<object>>(RD.Client.UIPanelComponentSystem.<OpenViewAsync>d__21<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UIPanelComponentSystem.<OpenViewAsync>d__22<object,object>>(RD.Client.UIPanelComponentSystem.<OpenViewAsync>d__22<object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UIPanelComponentSystem.<OpenViewAsync>d__23<object,object,object>>(RD.Client.UIPanelComponentSystem.<OpenViewAsync>d__23<object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UIPanelComponentSystem.<OpenViewAsync>d__24<object,object,object,object>>(RD.Client.UIPanelComponentSystem.<OpenViewAsync>d__24<object,object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UIPanelComponentSystem.<OpenViewAsync>d__25<object,object,object,object,object>>(RD.Client.UIPanelComponentSystem.<OpenViewAsync>d__25<object,object,object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UIPanelComponentSystem.<OpenViewParamAsync>d__13<object>>(RD.Client.UIPanelComponentSystem.<OpenViewParamAsync>d__13<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UIPanelComponentSystem.<OpenViewParamAsync>d__20>(RD.Client.UIPanelComponentSystem.<OpenViewParamAsync>d__20&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UIRootComponentSystem.<OpenPanelAsync>d__15>(RD.Client.UIRootComponentSystem.<OpenPanelAsync>d__15&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UIRootComponentSystem.<OpenPanelAsync>d__17<object>>(RD.Client.UIRootComponentSystem.<OpenPanelAsync>d__17<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UIRootComponentSystem.<OpenPanelAsync>d__18<object,object>>(RD.Client.UIRootComponentSystem.<OpenPanelAsync>d__18<object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UIRootComponentSystem.<OpenPanelAsync>d__19<object,object,object>>(RD.Client.UIRootComponentSystem.<OpenPanelAsync>d__19<object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UIRootComponentSystem.<OpenPanelAsync>d__1<object>>(RD.Client.UIRootComponentSystem.<OpenPanelAsync>d__1<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UIRootComponentSystem.<OpenPanelAsync>d__20<object,object,object,object>>(RD.Client.UIRootComponentSystem.<OpenPanelAsync>d__20<object,object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UIRootComponentSystem.<OpenPanelAsync>d__21<object,object,object,object,object>>(RD.Client.UIRootComponentSystem.<OpenPanelAsync>d__21<object,object,object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UIRootComponentSystem.<OpenPanelAsync>d__3<object,object>>(RD.Client.UIRootComponentSystem.<OpenPanelAsync>d__3<object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UIRootComponentSystem.<OpenPanelAsync>d__4<object,object,object>>(RD.Client.UIRootComponentSystem.<OpenPanelAsync>d__4<object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UIRootComponentSystem.<OpenPanelAsync>d__5<object,object,object,object>>(RD.Client.UIRootComponentSystem.<OpenPanelAsync>d__5<object,object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UIRootComponentSystem.<OpenPanelAsync>d__6<object,object,object,object,object>>(RD.Client.UIRootComponentSystem.<OpenPanelAsync>d__6<object,object,object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UIRootComponentSystem.<OpenPanelAsync>d__7<object,object,object,object,object,object>>(RD.Client.UIRootComponentSystem.<OpenPanelAsync>d__7<object,object,object,object,object,object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UIRootComponentSystem.<OpenPanelParamAsync>d__16>(RD.Client.UIRootComponentSystem.<OpenPanelParamAsync>d__16&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UIRootComponentSystem.<OpenPanelParamAsync>d__2<object>>(RD.Client.UIRootComponentSystem.<OpenPanelParamAsync>d__2<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Client.UIYooAssetsSpriteComponentSystem.<GetSpriteAsync>d__3>(RD.Client.UIYooAssetsSpriteComponentSystem.<GetSpriteAsync>d__3&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Core.PromiseHelper.<GetContextAsync>d__0<object>>(RD.Core.PromiseHelper.<GetContextAsync>d__0<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.CoroutineLockComponentSystem.<Wait>d__3>(RD.CoroutineLockComponentSystem.<Wait>d__3&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.CoroutineLockQueueSystem.<Wait>d__2>(RD.CoroutineLockQueueSystem.<Wait>d__2&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.CoroutineLockQueueTypeSystem.<Wait>d__4>(RD.CoroutineLockQueueTypeSystem.<Wait>d__4&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.ObjectWaitSystem.<Wait>d__2<object>>(RD.ObjectWaitSystem.<Wait>d__2<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.ProcessInnerSenderSystem.<Call>d__10>(RD.ProcessInnerSenderSystem.<Call>d__10&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.PromiseCancellationTokenHelper.<AddCancel>d__2<object>>(RD.PromiseCancellationTokenHelper.<AddCancel>d__2<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.PromiseCancellationTokenHelper.<TimeoutAsync>d__4<object>>(RD.PromiseCancellationTokenHelper.<TimeoutAsync>d__4<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.PromiseCancellationTokenHelper.<TimeoutAsync>d__6<object>>(RD.PromiseCancellationTokenHelper.<TimeoutAsync>d__6<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Server.GateMapFactory.<Create>d__0>(RD.Server.GateMapFactory.<Create>d__0&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Server.MessageLocationSenderComponentSystem.<Call>d__10>(RD.Server.MessageLocationSenderComponentSystem.<Call>d__10&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Server.MessageLocationSenderComponentSystem.<Call>d__8>(RD.Server.MessageLocationSenderComponentSystem.<Call>d__8&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Server.MessageLocationSenderComponentSystem.<CallInner>d__11>(RD.Server.MessageLocationSenderComponentSystem.<CallInner>d__11&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Server.MessageSenderSystem.<Call>d__2>(RD.Server.MessageSenderSystem.<Call>d__2&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.Server.ProcessOuterSenderSystem.<Call>d__13>(RD.Server.ProcessOuterSenderSystem.<Call>d__13&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.SessionSystem.<Call>d__3>(RD.SessionSystem.<Call>d__3&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.SessionSystem.<Call>d__4>(RD.SessionSystem.<Call>d__4&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<object>.Start<RD.YooAsset.ResourcesComponent.<LoadAssetAsync>d__22<object>>(RD.YooAsset.ResourcesComponent.<LoadAssetAsync>d__22<object>&)
		// System.Void RD.Core.AsyncPromiseMethodBuilder<uint>.Start<RD.Client.RouterHelper.<Connect>d__2>(RD.Client.RouterHelper.<Connect>d__2&)
		// object RD.Core.Entity.AddChild<object,RD.Client.UIBindVo,object>(RD.Client.UIBindVo,object,bool)
		// object RD.Core.Entity.AddChild<object,RD.Core.ActorId,object>(RD.Core.ActorId,object,bool)
		// object RD.Core.Entity.AddChild<object,long,long,int>(long,long,int,bool)
		// object RD.Core.Entity.AddChild<object,object>(object,bool)
		// object RD.Core.Entity.AddChildWithId<object,int>(long,int,bool)
		// object RD.Core.Entity.AddChildWithId<object,long>(long,long,bool)
		// object RD.Core.Entity.AddChildWithId<object,object>(long,object,bool)
		// object RD.Core.Entity.AddChildWithId<object>(long,bool)
		// object RD.Core.Entity.AddComponent<object,int,Unity.Mathematics.float3>(int,Unity.Mathematics.float3,bool)
		// object RD.Core.Entity.AddComponent<object,int>(int,bool)
		// object RD.Core.Entity.AddComponent<object,long>(long,bool)
		// object RD.Core.Entity.AddComponent<object,object,object>(object,object,bool)
		// object RD.Core.Entity.AddComponent<object,object>(object,bool)
		// object RD.Core.Entity.AddComponent<object>(bool)
		// object RD.Core.Entity.AddComponentWithId<object,int,Unity.Mathematics.float3>(long,int,Unity.Mathematics.float3,bool)
		// object RD.Core.Entity.AddComponentWithId<object,int>(long,int,bool)
		// object RD.Core.Entity.AddComponentWithId<object,long>(long,long,bool)
		// object RD.Core.Entity.AddComponentWithId<object,object,object>(long,object,object,bool)
		// object RD.Core.Entity.AddComponentWithId<object,object>(long,object,bool)
		// object RD.Core.Entity.AddComponentWithId<object>(long,bool)
		// object RD.Core.Entity.GetChild<object>(long)
		// object RD.Core.Entity.GetComponent<object>()
		// object RD.Core.Entity.GetParent<object>()
		// System.Void RD.Core.Entity.RemoveComponent<object>()
		// RD.Core.EntityRef<object> RD.Core.EntityRefHelper.GetEntityRefSafety<object>(object)
		// RD.Core.Promise RD.Core.EntitySystemHelper.DynamicEvent<RD.Client.UIEventPanelCloseAfter>(RD.Core.Entity,RD.Client.UIEventPanelCloseAfter)
		// RD.Core.Promise RD.Core.EntitySystemHelper.DynamicEvent<RD.Client.UIEventPanelCloseBefore>(RD.Core.Entity,RD.Client.UIEventPanelCloseBefore)
		// RD.Core.Promise RD.Core.EntitySystemHelper.DynamicEvent<RD.Client.UIEventPanelDestroy>(RD.Core.Entity,RD.Client.UIEventPanelDestroy)
		// RD.Core.Promise RD.Core.EntitySystemHelper.DynamicEvent<RD.Client.UIEventPanelOpenAfter>(RD.Core.Entity,RD.Client.UIEventPanelOpenAfter)
		// RD.Core.Promise RD.Core.EntitySystemHelper.DynamicEvent<RD.Client.UIEventPanelOpenBefore>(RD.Core.Entity,RD.Client.UIEventPanelOpenBefore)
		// System.Void RD.Core.EntitySystemSingleton.Awake<RD.Client.UIBindVo,object>(RD.Core.Entity,RD.Client.UIBindVo,object)
		// System.Void RD.Core.EntitySystemSingleton.Awake<RD.Core.ActorId,object>(RD.Core.Entity,RD.Core.ActorId,object)
		// System.Void RD.Core.EntitySystemSingleton.Awake<int,Unity.Mathematics.float3>(RD.Core.Entity,int,Unity.Mathematics.float3)
		// System.Void RD.Core.EntitySystemSingleton.Awake<int>(RD.Core.Entity,int)
		// System.Void RD.Core.EntitySystemSingleton.Awake<long,long,int>(RD.Core.Entity,long,long,int)
		// System.Void RD.Core.EntitySystemSingleton.Awake<long>(RD.Core.Entity,long)
		// System.Void RD.Core.EntitySystemSingleton.Awake<object,object>(RD.Core.Entity,object,object)
		// System.Void RD.Core.EntitySystemSingleton.Awake<object>(RD.Core.Entity,object)
		// object RD.Core.EntityHelper.GetComponent<object>(RD.Core.Entity,bool)
		// System.Void RD.Core.EventSystem.Invoke<RD.NetComponentOnRead>(long,RD.NetComponentOnRead)
		// System.Void RD.Core.EventSystem.Invoke<RD.TimerCallback>(long,RD.TimerCallback)
		// System.Void RD.Core.EventSystem.InvokeEntity<RD.Client.UIInvokeEntity_DestroyPanel>(RD.Core.Entity,long,RD.Client.UIInvokeEntity_DestroyPanel)
		// System.Void RD.Core.EventSystem.InvokeEntity<RD.Client.UIInvokeEntity_RecoverLayerOptionForever>(RD.Core.Entity,long,RD.Client.UIInvokeEntity_RecoverLayerOptionForever)
		// System.Void RD.Core.EventSystem.InvokeEntity<RD.Client.UIInvokeEntity_ReleaseInstantiate>(RD.Core.Entity,long,RD.Client.UIInvokeEntity_ReleaseInstantiate)
		// System.Void RD.Core.EventSystem.InvokeEntity<RD.Client.UIInvokeEntity_RemoveUIReset>(RD.Core.Entity,long,RD.Client.UIInvokeEntity_RemoveUIReset)
		// System.Void RD.Core.EventSystem.InvokeEntity<RD.Client.UIInvokeEntity_ViewClosePanel>(RD.Core.Entity,long,RD.Client.UIInvokeEntity_ViewClosePanel)
		// long RD.Core.EventSystem.InvokeEntity<RD.Client.UIInvokeEntity_BanLayerOptionForever,long>(RD.Core.Entity,long,RD.Client.UIInvokeEntity_BanLayerOptionForever)
		// object RD.Core.EventSystem.InvokeEntity<RD.Client.UIInvokeEntity_ClosePanel,object>(RD.Core.Entity,long,RD.Client.UIInvokeEntity_ClosePanel)
		// object RD.Core.EventSystem.InvokeEntity<RD.Client.UIInvokeEntity_HomePanel,object>(RD.Core.Entity,long,RD.Client.UIInvokeEntity_HomePanel)
		// object RD.Core.EventSystem.InvokeEntity<RD.Client.UIInvokeEntity_LoadInitialize,object>(RD.Core.Entity,RD.Client.UIInvokeEntity_LoadInitialize)
		// object RD.Core.EventSystem.InvokeEntity<RD.Client.UIInvokeEntity_LoadInitialize,object>(RD.Core.Entity,long,RD.Client.UIInvokeEntity_LoadInitialize)
		// object RD.Core.EventSystem.InvokeEntity<RD.Client.UIInvokeEntity_SceneOpenPanel,object>(RD.Core.Entity,long,RD.Client.UIInvokeEntity_SceneOpenPanel)
		// object RD.Core.EventSystem.InvokeEntity<RD.Client.UIInvokeEntity_ViewClosePanel,object>(RD.Core.Entity,long,RD.Client.UIInvokeEntity_ViewClosePanel)
		// System.Void RD.Core.EventSystem.Publish<object,RD.AfterCreateCurrentScene>(object,RD.AfterCreateCurrentScene)
		// System.Void RD.Core.EventSystem.Publish<object,RD.AfterUnitCreate>(object,RD.AfterUnitCreate)
		// System.Void RD.Core.EventSystem.Publish<object,RD.Client.UIEventPanelCloseAfter>(object,RD.Client.UIEventPanelCloseAfter)
		// System.Void RD.Core.EventSystem.Publish<object,RD.Client.UIEventPanelCloseBefore>(object,RD.Client.UIEventPanelCloseBefore)
		// System.Void RD.Core.EventSystem.Publish<object,RD.Client.UIEventPanelDestroy>(object,RD.Client.UIEventPanelDestroy)
		// System.Void RD.Core.EventSystem.Publish<object,RD.EnterMapFinish>(object,RD.EnterMapFinish)
		// System.Void RD.Core.EventSystem.Publish<object,RD.MoveStart>(object,RD.MoveStart)
		// System.Void RD.Core.EventSystem.Publish<object,RD.MoveStop>(object,RD.MoveStop)
		// System.Void RD.Core.EventSystem.Publish<object,RD.SceneChangeFinish>(object,RD.SceneChangeFinish)
		// System.Void RD.Core.EventSystem.Publish<object,RD.SceneChangeStart>(object,RD.SceneChangeStart)
		// System.Void RD.Core.EventSystem.Publish<object,RD.Server.UnitEnterSightRange>(object,RD.Server.UnitEnterSightRange)
		// System.Void RD.Core.EventSystem.Publish<object,RD.Server.UnitLeaveSightRange>(object,RD.Server.UnitLeaveSightRange)
		// RD.Core.Promise RD.Core.EventSystem.PublishAsync<object,RD.AppStartInitFinish>(object,RD.AppStartInitFinish)
		// RD.Core.Promise RD.Core.EventSystem.PublishAsync<object,RD.Client.LoginFinish>(object,RD.Client.LoginFinish)
		// RD.Core.Promise RD.Core.EventSystem.PublishAsync<object,RD.Client.UIEventInitializeAfter>(object,RD.Client.UIEventInitializeAfter)
		// RD.Core.Promise RD.Core.EventSystem.PublishAsync<object,RD.Client.UIEventInitializeBefore>(object,RD.Client.UIEventInitializeBefore)
		// RD.Core.Promise RD.Core.EventSystem.PublishAsync<object,RD.Client.UIEventPanelCloseAfter>(object,RD.Client.UIEventPanelCloseAfter)
		// RD.Core.Promise RD.Core.EventSystem.PublishAsync<object,RD.Client.UIEventPanelCloseBefore>(object,RD.Client.UIEventPanelCloseBefore)
		// RD.Core.Promise RD.Core.EventSystem.PublishAsync<object,RD.Client.UIEventPanelOpenAfter>(object,RD.Client.UIEventPanelOpenAfter)
		// RD.Core.Promise RD.Core.EventSystem.PublishAsync<object,RD.Client.UIEventPanelOpenBefore>(object,RD.Client.UIEventPanelOpenBefore)
		// RD.Core.Promise RD.Core.EventSystem.PublishAsync<object,RD.Client.UIEventViewOpenBefore>(object,RD.Client.UIEventViewOpenBefore)
		// RD.Core.Promise RD.Core.EventSystem.PublishAsync<object,RD.Client.UIEvent_YooAssetsLoad_Completed>(object,RD.Client.UIEvent_YooAssetsLoad_Completed)
		// System.Void RD.Core.EventSystem_InvokeEntity_UIExtension.UIInvokeEntity<RD.Client.UIInvokeEntity_DestroyPanel>(RD.Core.EventSystem,RD.Core.Entity,long,RD.Client.UIInvokeEntity_DestroyPanel)
		// System.Void RD.Core.EventSystem_InvokeEntity_UIExtension.UIInvokeEntity<RD.Client.UIInvokeEntity_RecoverLayerOptionForever>(RD.Core.EventSystem,RD.Core.Entity,long,RD.Client.UIInvokeEntity_RecoverLayerOptionForever)
		// System.Void RD.Core.EventSystem_InvokeEntity_UIExtension.UIInvokeEntity<RD.Client.UIInvokeEntity_ReleaseInstantiate>(RD.Core.EventSystem,RD.Core.Entity,long,RD.Client.UIInvokeEntity_ReleaseInstantiate)
		// System.Void RD.Core.EventSystem_InvokeEntity_UIExtension.UIInvokeEntity<RD.Client.UIInvokeEntity_RemoveUIReset>(RD.Core.EventSystem,RD.Core.Entity,long,RD.Client.UIInvokeEntity_RemoveUIReset)
		// System.Void RD.Core.EventSystem_InvokeEntity_UIExtension.UIInvokeEntity<RD.Client.UIInvokeEntity_ViewClosePanel>(RD.Core.EventSystem,RD.Core.Entity,long,RD.Client.UIInvokeEntity_ViewClosePanel)
		// long RD.Core.EventSystem_InvokeEntity_UIExtension.UIInvokeEntity<RD.Client.UIInvokeEntity_BanLayerOptionForever,long>(RD.Core.EventSystem,RD.Core.Entity,long,RD.Client.UIInvokeEntity_BanLayerOptionForever)
		// object RD.Core.EventSystem_InvokeEntity_UIExtension.UIInvokeEntity<RD.Client.UIInvokeEntity_ClosePanel,object>(RD.Core.EventSystem,RD.Core.Entity,long,RD.Client.UIInvokeEntity_ClosePanel)
		// object RD.Core.EventSystem_InvokeEntity_UIExtension.UIInvokeEntity<RD.Client.UIInvokeEntity_HomePanel,object>(RD.Core.EventSystem,RD.Core.Entity,long,RD.Client.UIInvokeEntity_HomePanel)
		// object RD.Core.EventSystem_InvokeEntity_UIExtension.UIInvokeEntity<RD.Client.UIInvokeEntity_SceneOpenPanel,object>(RD.Core.EventSystem,RD.Core.Entity,long,RD.Client.UIInvokeEntity_SceneOpenPanel)
		// object RD.Core.EventSystem_InvokeEntity_UIExtension.UIInvokeEntity<RD.Client.UIInvokeEntity_ViewClosePanel,object>(RD.Core.EventSystem,RD.Core.Entity,long,RD.Client.UIInvokeEntity_ViewClosePanel)
		// object RD.Core.EventSystem_InvokeEntity_UIExtension.UIInvokeEntityAsync<RD.Client.UIInvokeEntity_ClosePanel,object>(RD.Core.EventSystem,RD.Core.Entity,RD.Client.UIInvokeEntity_ClosePanel)
		// object RD.Core.EventSystem_InvokeEntity_UIExtension.UIInvokeEntityAsync<RD.Client.UIInvokeEntity_HomePanel,object>(RD.Core.EventSystem,RD.Core.Entity,RD.Client.UIInvokeEntity_HomePanel)
		// object RD.Core.EventSystem_InvokeEntity_UIExtension.UIInvokeEntityAsync<RD.Client.UIInvokeEntity_SceneOpenPanel,object>(RD.Core.EventSystem,RD.Core.Entity,RD.Client.UIInvokeEntity_SceneOpenPanel)
		// object RD.Core.EventSystem_InvokeEntity_UIExtension.UIInvokeEntityAsync<RD.Client.UIInvokeEntity_ViewClosePanel,object>(RD.Core.EventSystem,RD.Core.Entity,RD.Client.UIInvokeEntity_ViewClosePanel)
		// System.Void RD.Core.EventSystem_InvokeEntity_UIExtension.UIInvokeEntitySync<RD.Client.UIInvokeEntity_RecoverLayerOptionForever>(RD.Core.EventSystem,RD.Core.Entity,RD.Client.UIInvokeEntity_RecoverLayerOptionForever)
		// System.Void RD.Core.EventSystem_InvokeEntity_UIExtension.UIInvokeEntitySync<RD.Client.UIInvokeEntity_RemoveUIReset>(RD.Core.EventSystem,RD.Core.Entity,RD.Client.UIInvokeEntity_RemoveUIReset)
		// System.Void RD.Core.EventSystem_InvokeEntity_UIExtension.UIInvokeEntitySync<RD.Client.UIInvokeEntity_ViewClosePanel>(RD.Core.EventSystem,RD.Core.Entity,RD.Client.UIInvokeEntity_ViewClosePanel)
		// long RD.Core.EventSystem_InvokeEntity_UIExtension.UIInvokeEntitySync<RD.Client.UIInvokeEntity_BanLayerOptionForever,long>(RD.Core.EventSystem,RD.Core.Entity,RD.Client.UIInvokeEntity_BanLayerOptionForever)
		// System.Void RD.Core.EventSystem_InvokeEntity_UISafetyExtension.UIInvokeEntitySyncSafety<RD.Client.UIInvokeEntity_DestroyPanel>(RD.Core.EventSystem,RD.Core.Entity,RD.Client.UIInvokeEntity_DestroyPanel)
		// System.Void RD.Core.EventSystem_InvokeEntity_UISafetyExtension.UIInvokeEntitySyncSafety<RD.Client.UIInvokeEntity_ReleaseInstantiate>(RD.Core.EventSystem,RD.Core.Entity,RD.Client.UIInvokeEntity_ReleaseInstantiate)
		// object RD.Core.MongoHelper.Deserialize<object>(byte[])
		// object RD.Core.MongoHelper.FromJson<object>(string)
		// System.Void RD.Core.MongoRegister.RegisterStruct<Unity.Mathematics.float2>()
		// System.Void RD.Core.MongoRegister.RegisterStruct<Unity.Mathematics.float3>()
		// System.Void RD.Core.MongoRegister.RegisterStruct<Unity.Mathematics.float4>()
		// System.Void RD.Core.MongoRegister.RegisterStruct<Unity.Mathematics.quaternion>()
		// System.Void RD.Core.ObjectHelper.Swap<object>(object&,object&)
		// System.Void RD.Core.RandomGenerator.BreakRank<object>(System.Collections.Generic.List<object>)
		// object RD.Core.World.AddSingleton<object>()
		// RD.Core.Promise<object> RD.YooAsset.ResourcesComponent.LoadAssetAsync<object>(string)
		// object System.Activator.CreateInstance<object>()
		// object System.Collections.Generic.CollectionExtensions.GetValueOrDefault<object,object>(System.Collections.Generic.IReadOnlyDictionary<object,object>,object)
		// object System.Collections.Generic.CollectionExtensions.GetValueOrDefault<object,object>(System.Collections.Generic.IReadOnlyDictionary<object,object>,object,object)
		// bool System.Linq.Enumerable.Any<object>(System.Collections.Generic.IEnumerable<object>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Empty<object>()
		// System.Collections.Generic.IEnumerable<System.Linq.IGrouping<object,object>> System.Linq.Enumerable.GroupBy<object,object,object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,object>,System.Func<object,object>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.OfType<object>(System.Collections.IEnumerable)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.OfTypeIterator<object>(System.Collections.IEnumerable)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Select<object,object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,object>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.SelectMany<object,object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,System.Collections.Generic.IEnumerable<object>>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.SelectManyIterator<object,object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,System.Collections.Generic.IEnumerable<object>>)
		// RD.RpcInfo[] System.Linq.Enumerable.ToArray<RD.RpcInfo>(System.Collections.Generic.IEnumerable<RD.RpcInfo>)
		// long[] System.Linq.Enumerable.ToArray<long>(System.Collections.Generic.IEnumerable<long>)
		// object[] System.Linq.Enumerable.ToArray<object>(System.Collections.Generic.IEnumerable<object>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Where<object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,bool>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Iterator<object>.Select<object>(System.Func<object,object>)
		// System.Threading.Tasks.Task<object> System.Threading.Tasks.TaskFactory.StartNew<object>(System.Func<object>,System.Threading.CancellationToken)
		// object UnityEngine.Component.GetComponent<object>()
		// object UnityEngine.GameObject.AddComponent<object>()
		// object UnityEngine.GameObject.GetComponent<object>()
		// object UnityEngine.GameObject.GetComponentInChildren<object>()
		// object UnityEngine.GameObject.GetComponentInChildren<object>(bool)
		// object UnityEngine.Object.Instantiate<object>(object)
		// object UnityEngine.Object.Instantiate<object>(object,UnityEngine.Transform,bool)
		// object UnityEngine.Resources.Load<object>(string)
		// YooAsset.AllAssetsHandle YooAsset.ResourcePackage.LoadAllAssetsAsync<object>(string,uint)
		// YooAsset.AssetHandle YooAsset.ResourcePackage.LoadAssetAsync<object>(string,uint)
	}
}