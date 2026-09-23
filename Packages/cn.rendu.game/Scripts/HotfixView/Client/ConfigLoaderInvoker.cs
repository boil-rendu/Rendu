using System;
using System.Collections.Generic;
using System.IO;
using RD.Core;
using RD.Loader;
using RD.YooAsset;
using UnityEngine;

namespace RD.Client
{
    [Invoke]
    public class GetAllConfigBytes : AInvokeHandler<ConfigLoader.GetAllConfigBytes, Promise<Dictionary<Type, byte[]>>>
    {
        public override async Promise<Dictionary<Type, byte[]>> Handle(ConfigLoader.GetAllConfigBytes args)
        {
            Dictionary<Type, byte[]> output = new Dictionary<Type, byte[]>();
            HashSet<Type> configTypes = CodeTypes.Instance.GetTypes(typeof(ConfigAttribute));

            if (Define.IsEditor)
            {
                GlobalConfig globalConfig = Resources.Load<GlobalConfig>("GlobalConfig");
                CodeMode codeMode = globalConfig != null ? globalConfig.CodeMode : CodeMode.ClientServer;
                string ct = "ClientServer";
                switch (codeMode)
                {
                    case CodeMode.Client:
                        ct = "Client";
                        break;
                    case CodeMode.Server:
                        ct = "Server";
                        break;
                    case CodeMode.ClientServer:
                        ct = "ClientServer";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }


                List<string> startConfigs = new List<string>()
                {
                    "StartMachineConfigCategory",
                    "StartProcessConfigCategory",
                    "StartSceneConfigCategory",
                    "StartZoneConfigCategory",
                };
                foreach (Type configType in configTypes)
                {
                    string configFilePath;
                    if (startConfigs.Contains(configType.Name))
                    {
                        if (codeMode == CodeMode.Client) continue;
                        configFilePath =
                            $"{ConstValue.ExcelPackagePath}/Config/Bytes/{ct}/{Options.Instance.StartConfig}/{configType.Name}.bytes";
                    }
                    else
                    {
                        configFilePath = $"{ConstValue.ExcelPackagePath}/Config/Bytes/{ct}/{configType.Name}.bytes";
                    }

                    output[configType] = File.ReadAllBytes(configFilePath);
                }
            }
            else
            {
                foreach (Type type in configTypes)
                {
                    TextAsset v =
                        await ResourcesComponent.Instance.LoadAssetAsync<TextAsset>(
                            $"{ConstValue.ExcelPackagePath}/Config/Bytes/Client/{type.Name}.bytes");
                    output[type] = v.bytes;
                }
            }

            return output;
        }
    }

    [Invoke]
    public class GetOneConfigBytes : AInvokeHandler<ConfigLoader.GetOneConfigBytes, Promise<byte[]>>
    {
        public override async Promise<byte[]> Handle(ConfigLoader.GetOneConfigBytes args)
        {
            string ct = "ClientServer";
            GlobalConfig globalConfig = Resources.Load<GlobalConfig>("GlobalConfig");
            if (globalConfig is null)
            {
                throw new Exception("GlobalConfig is null");
            }

            CodeMode codeMode = globalConfig.CodeMode;
            switch (codeMode)
            {
                case CodeMode.Client:
                    ct = "Client";
                    break;
                case CodeMode.Server:
                    ct = "Server";
                    break;
                case CodeMode.ClientServer:
                    ct = "ClientServer";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            List<string> startConfigs = new List<string>()
            {
                "StartMachineConfigCategory",
                "StartProcessConfigCategory",
                "StartSceneConfigCategory",
                "StartZoneConfigCategory",
            };

            string configName = args.ConfigName;

            string configFilePath;
            if (startConfigs.Contains(configName))
            {
                configFilePath =
                    $"{ConstValue.ExcelPackagePath}/Config/Bytes/{ct}/{Options.Instance.StartConfig}/{configName}.bytes";
            }
            else
            {
                configFilePath = $"{ConstValue.ExcelPackagePath}/Config/Bytes/{ct}/{configName}.bytes";
            }

            await Promise.CompletedTask;
            return File.ReadAllBytes(configFilePath);
        }
    }
}