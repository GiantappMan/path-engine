﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PathEngine.Pipelines.GetterMiddles
{
    /// <summary>
    /// 获取注册表内容
    /// </summary>
    [Obsolete]
    internal class GetRegistrysContentMiddle : IGetterMiddle
    {
        public const string Command = "registry";
        GetterPipelinePayload IGetterMiddle.Input(GetterPipelinePayload payload)
        {
            if (payload.Command.Schemas.Contains(Command))
            {
                List<GetterPipelinePayloadData> res = new();
                foreach (var item in payload.Data)
                {
                    var tmp = item.GetValue().Split(':');
                    if (tmp.Length > 1)
                    {
                        var registryKey = GetRegistryKey(tmp[0]);
                        var registryData = tmp[1];
                        if (registryData.Contains('*'))
                        {
                            var names = registryKey?.GetValueNames();
                            registryData = names?.FirstOrDefault(m => m.Contains(registryData.Replace("*", "")));
                        }

                        var tmpRes = registryKey?.GetValue(registryData);
                        GetterPipelinePayloadData tmpResulItem = new GetterPipelinePayloadData(null);
                        if (tmpRes is byte[] tmpBytes)
                            tmpResulItem = new GetterPipelinePayloadData(System.Text.Encoding.UTF8.GetString(tmpBytes));
                        else if (tmpRes is int tmpInt)
                            tmpResulItem = new GetterPipelinePayloadData(tmpInt);
                        else if (tmpRes != null)
                            tmpResulItem = new GetterPipelinePayloadData(tmpRes?.ToString());
                        res.Add(tmpResulItem);
                    }
                    else
                        res.Add(new GetterPipelinePayloadData(null));
                }
                payload.SetData(res.ToArray());
            }
            return payload;
        }

        internal static RegistryKey? GetRootRegistry(string path, out string keyPath)
        {
            var rootEndIndex = path.IndexOf("\\") + 1;
            var root = path[..rootEndIndex];
            keyPath = path[rootEndIndex..];
            switch (root)
            {
                case "HKEY_CLASSES_ROOT\\":
                    return Registry.ClassesRoot;
                case "HKEY_CURRENT_USER\\":
                    return Registry.CurrentUser;
                case "HKEY_LOCAL_MACHINE\\":
                    return Registry.LocalMachine;
                case "HKEY_USERS\\":
                    return Registry.Users;
                case "HKEY_CURRENT_CONFIG\\":
                    return Registry.CurrentConfig;
            };
            return null;
        }

        internal static RegistryKey? GetRegistryKey(string path, bool canWrite = false)
        {
            using RegistryKey? rootKey = GetRootRegistry(path, out string keyPath);
            RegistryKey? key = rootKey?.OpenSubKey(keyPath, canWrite);
            return key;
        }
    }
}