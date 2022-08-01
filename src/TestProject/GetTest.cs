﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32;
using PathEngine;
using System.Diagnostics;
using System.Reflection;

namespace TestProject
{
    [TestClass]
    public class GetTest
    {
        /// <summary>
        /// 路径解析
        /// </summary>
        [TestMethod]
        public void GetPath()
        {
            //普通路径
            var res = PathResolver.Instance.Get("%ProgramFiles(x86)%");
            Assert.IsTrue(res == @"C:\Program Files (x86)");
            //注册表
            res = PathResolver.Instance.Get(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}");
            Assert.IsTrue(res == @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}");
            //内嵌文件
            res = PathResolver.Instance.Get(@"TestProject.Configs.config.txt");
            Assert.IsTrue(res == @"TestProject.Configs.config.txt");
            PathResolver.Instance.Variables["%my_var%"] = "test";
        }

        [TestMethod]
        public void ListPath()
        {
            //通配符搜索
            var res1 = PathResolver.Instance.List(@"path:\%ProgramData%\Microsoft\VisualStudio\Packages\Microsoft.CodeAnalysis*\");
            Assert.IsTrue(res1 != null);

            var res2 = PathResolver.Instance.List(@"path:\%ProgramData%\*\*\*.txt");
            Assert.IsTrue(res2 != null);
        }

            [TestMethod]
        public void GetEmbeddedResource()
        {
            PathResolver.EntryAssembly = Assembly.GetExecutingAssembly();
            var res = PathResolver.Instance.Get(@"embedded:\Configs\config.txt");
            using var reader = new StreamReader(PathResolver.EntryAssembly.GetManifestResourceStream("TestProject.Configs.config.txt")!);
            string tmp = reader.ReadToEnd();
            Assert.IsTrue(res == tmp);
        }

        [TestMethod]
        public void GetContent()
        {
            PathResolver.EntryAssembly = Assembly.GetExecutingAssembly();
            var res = PathResolver.Instance.Get(@"path_content:\Configs\config2.txt");

            Assert.IsTrue(res == "hello world!!!");
        }

        [TestMethod]
        public void GetVersion()
        {
            string exe = @"C:\Windows\System32\cmd.exe";
            var res = PathResolver.Instance.Get(@$"version:\{exe}");
            FileVersionInfo version = FileVersionInfo.GetVersionInfo(exe)!;
            Assert.IsTrue(res == version.FileVersion!.ToString());
        }

        [TestMethod]
        public void GetRegistryContent()
        {
            var res = PathResolver.Instance.Get(@"registry:\HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}:pv");
            using var tmpKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}")!;
            var tmpValue = tmpKey.GetValue("pv")?.ToString();
            Assert.IsTrue(res == tmpValue);

            //无协议的情况
            res = PathResolver.Instance.Get(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}:pv");
            Assert.IsTrue(res == tmpValue);
        }

        [TestMethod]
        public void GetGenericTypeValue()
        {
            int res = PathResolver.Instance.Get<int>(@"registry:\HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate:OOBEDiagnosticsSent");
            Assert.IsTrue(res > 0);

            string? res1 = PathResolver.Instance.Get<string>(@"registry:\HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate:OOBEDiagnosticsSent");
            Assert.IsTrue(res1 != null);
        }
    }
}
