#if DOTNET
using System;
using System.Collections.Generic;
using System.IO;
using RD.Core;

namespace RD.Server
{
    [Invoke]
    public class GetAllConfigBytes: AInvokeHandler<ConfigLoader.GetAllConfigBytes, Promise<Dictionary<Type, byte[]>>>
    {
        public override async Promise<Dictionary<Type, byte[]>> Handle(ConfigLoader.GetAllConfigBytes args)
        {
            Dictionary<Type, byte[]> output = new Dictionary<Type, byte[]>();
            List<string> startConfigs = new List<string>()
            {
                "StartMachineConfigCategory", 
                "StartProcessConfigCategory", 
                "StartSceneConfigCategory", 
                "StartZoneConfigCategory",
            };
            string basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", ConstValue.ExcelPackagePath, "Config", "Bytes", "Server");
            
            HashSet<Type> configTypes = CodeTypes.Instance.GetTypes(typeof (ConfigAttribute));
            foreach (Type configType in configTypes)
            {
                string configFilePath;
                if (startConfigs.Contains(configType.Name))
                {
                    configFilePath = Path.Combine(basePath, Options.Instance.StartConfig,$"{configType.Name}.bytes");
                }
                else
                {
                    configFilePath = Path.Combine(basePath,$"{configType.Name}.bytes");
                }
                output[configType] = File.ReadAllBytes(configFilePath);
            }

            await Promise.CompletedTask;
            return output;
        }
    }
    
    [Invoke]
    public class GetOneConfigBytes: AInvokeHandler<ConfigLoader.GetOneConfigBytes, byte[]>
    {
        public override byte[] Handle(ConfigLoader.GetOneConfigBytes args)
        {
            string basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", ConstValue.ExcelPackagePath, "Config", "Bytes", "Server");
            string configFilePath = Path.Combine(basePath,$"{args.ConfigName}.bytes");
            byte[] configBytes = File.ReadAllBytes(configFilePath);
            return configBytes;
        }
    }
}
#endif