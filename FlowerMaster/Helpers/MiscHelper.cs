﻿using System;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Controls;
using System.Diagnostics;
using Microsoft.Win32;
using System.IO;
using System.Runtime.InteropServices;
using mshtml;
using SHDocVw;
using System.Drawing;
using System.Drawing.Imaging;
using FlowerMaster.Models;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace FlowerMaster.Helpers
{
    /// <summary>
    /// 通用静态函数库
    /// </summary>
    class MiscHelper
    {
        [StructLayout(LayoutKind.Sequential)]
        internal class DVTARGETDEVICE
        {
            public ushort tdSize;
            public uint tdDeviceNameOffset;
            public ushort tdDriverNameOffset;
            public ushort tdExtDevmodeOffset;
            public ushort tdPortNameOffset;
            public byte tdData;
        }
        [StructLayout(LayoutKind.Sequential)]
        internal class RECT
        {
            public int left;
            public int top;
            public int width;
            public int height;
        }

        [ComImport, Guid("6d5140c1-7436-11ce-8034-00aa006009fa"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), ComVisible(false)]
        internal interface IServiceProvider
        {
            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int QueryService(ref Guid guidService, ref Guid riid, [MarshalAs(UnmanagedType.Interface)] out object ppvObject);
        }

        [ComImport, Guid("0000010d-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IViewObject
        {
            [PreserveSig]
            int Draw(
                [In, MarshalAs(UnmanagedType.U4)] int dwDrawAspect,
                int lindex,
                IntPtr pvAspect,
                [In] DVTARGETDEVICE ptd,
                IntPtr hdcTargetDev,
                IntPtr hdcDraw,
                [In] RECT lprcBounds,
                [In] RECT lprcWBounds,
                IntPtr pfnContinue,
                [In] IntPtr dwContinue);
        }

        /// <summary>
        /// 日志记录类型
        /// </summary>
        public enum LogType
        {
            Default,
            System,
            Search,
            Stage,
            Boss,
            Gacha,
            Sell,
            Mailbox,
            Levelup,
            Debug = 99,
        }

        public static MainWindow main;

        public static void AddLog(string log, LogType type=LogType.Default)
        {
            System.Windows.Media.Color typeColor = Colors.White;
            switch (type)
            {
                case LogType.System:
                    typeColor = Colors.Bisque;
                    break;
                case LogType.Search:
                    typeColor = Colors.Violet;
                    break;
                case LogType.Stage:
                    typeColor = Colors.LightGreen;
                    break;
                case LogType.Boss:
                    typeColor = Colors.LightCoral;
                    break;
                case LogType.Gacha:
                case LogType.Levelup:
                    typeColor = Colors.Yellow;
                    break;
                case LogType.Sell:
                    typeColor = Colors.DarkOrange;
                    break;
                case LogType.Mailbox:
                    typeColor = Colors.DeepSkyBlue;
                    break;
                case LogType.Default:
                default:
                    typeColor = Colors.White;
                    break;
            }
            if (!main.Dispatcher.CheckAccess())
            {
                main.Dispatcher.Invoke(new Action(() =>
                {
                    Paragraph p = new Paragraph();
                    Run timeText = new Run() { Text = DateTime.Now.ToString("HH:mm:ss") + " ", Foreground = new SolidColorBrush(Colors.Gray) };
                    Run logText = new Run() { Text = log, Foreground = new SolidColorBrush(typeColor) };
                    p.Inlines.Add(timeText);
                    p.Inlines.Add(logText);
                    p.LineHeight = 3;
                    main.gameLog.Document.Blocks.Add(p);
                    main.gameLog.ScrollToEnd();
                }));
            }
            else
            {
                Paragraph p = new Paragraph();
                Run timeText = new Run() { Text = DateTime.Now.ToString("HH:mm:ss") + " ", Foreground = new SolidColorBrush(Colors.Gray) };
                Run logText = new Run() { Text = log, Foreground = new SolidColorBrush(typeColor) };
                p.Inlines.Add(timeText);
                p.Inlines.Add(logText);
                p.LineHeight = 3;
                main.gameLog.Document.Blocks.Add(p);
                main.gameLog.ScrollToEnd();
            }
            if (type != LogType.System && type != LogType.Debug) LogsHelper.LogGame(log);
        }

        public static void AddGachaLog(JArray cards)
        {
            if (!main.gameLog.Dispatcher.CheckAccess())
            {
                main.gameLog.Dispatcher.Invoke(new Action(() =>
                {
                    string logs = "进行了一次扭蛋，获得：";
                    Paragraph p = new Paragraph();
                    Run timeText = new Run() { Text = DateTime.Now.ToString("HH:mm:ss") + " ", Foreground = new SolidColorBrush(Colors.Gray) };
                    Run log = new Run() { Text = "进行了一次扭蛋，获得：", Foreground = new SolidColorBrush(Colors.Yellow) };
                    p.Inlines.Add(timeText);
                    p.Inlines.Add(log);
                    p.LineHeight = 3;
                    int cnt = 0;
                    foreach (JObject card in cards)
                    {
                        string cardStr = DataUtil.Cards.GetName(int.Parse(card["characterId"].ToString()));
                        System.Windows.Media.Color color = Colors.White;
                        if (cards.Count == 10 && cardStr == "★3茉莉")
                        {
                            color = Colors.Red;
                        }
                        else if (cardStr.IndexOf("★1") != -1)
                        {
                            color = Colors.LightSteelBlue;
                        }
                        else if (cardStr.IndexOf("★2") != -1)
                        {
                            color = Colors.Aquamarine;
                        }
                        else if (cardStr.IndexOf("★3") != -1)
                        {
                            color = Colors.Chocolate;
                        }
                        else if (cardStr.IndexOf("★4") != -1)
                        {
                            color = Colors.Silver;
                        }
                        else if (cardStr.IndexOf("★5") != -1)
                        {
                            color = Colors.Gold;
                        }
                        else if (cardStr.IndexOf("★6") != -1)
                        {
                            color = Colors.Violet;
                        }
                        if (cnt > 0)
                        {
                            Run cardTextEnd = new Run() { Text = "、", Foreground = new SolidColorBrush(Colors.Yellow) };
                            p.Inlines.Add(cardTextEnd);
                        }
                        Run cardText = new Run() { Text = card["bookStatus"].ToString() == "2" ? cardStr + "（新）" : cardStr, Foreground = new SolidColorBrush(color) };
                        p.Inlines.Add(cardText);
                        logs += card["bookStatus"].ToString() == "2" ? cardStr + "（新）、" : cardStr + "、";
                        cnt++;
                    }
                    main.gameLog.Document.Blocks.Add(p);
                    main.gameLog.ScrollToEnd();
                    LogsHelper.LogGame(logs.Substring(0, logs.Length - 1));
                }));
            }
            else
            {
                string logs = "进行了一次扭蛋，获得：";
                Paragraph p = new Paragraph();
                Run timeText = new Run() { Text = DateTime.Now.ToString("HH:mm:ss") + " ", Foreground = new SolidColorBrush(Colors.Gray) };
                Run log = new Run() { Text = "进行了一次扭蛋，获得：", Foreground = new SolidColorBrush(Colors.Yellow) };
                p.Inlines.Add(timeText);
                p.Inlines.Add(log);
                p.LineHeight = 3;
                int cnt = 0;
                foreach (JObject card in cards)
                {
                    string cardStr = DataUtil.Cards.GetName(int.Parse(card["characterId"].ToString()));
                    System.Windows.Media.Color color = Colors.White;
                    if (cards.Count == 10 && cardStr == "★3茉莉")
                    {
                        color = Colors.Red;
                    }
                    else if (cardStr.IndexOf("★1") != -1)
                    {
                        color = Colors.LightSteelBlue;
                    }
                    else if (cardStr.IndexOf("★2") != -1)
                    {
                        color = Colors.Aquamarine;
                    }
                    else if (cardStr.IndexOf("★3") != -1)
                    {
                        color = Colors.Chocolate;
                    }
                    else if (cardStr.IndexOf("★4") != -1)
                    {
                        color = Colors.Silver;
                    }
                    else if (cardStr.IndexOf("★5") != -1)
                    {
                        color = Colors.Gold;
                    }
                    else if (cardStr.IndexOf("★6") != -1)
                    {
                        color = Colors.Violet;
                    }
                    if (cnt > 0)
                    {
                        Run cardTextEnd = new Run() { Text = "，", Foreground = new SolidColorBrush(Colors.Yellow) };
                        p.Inlines.Add(cardTextEnd);
                    }
                    Run cardText = new Run() { Text = card["bookStatus"].ToString() == "2" ? cardStr + "（新）" : cardStr, Foreground = new SolidColorBrush(color) };
                    p.Inlines.Add(cardText);
                    logs += card["bookStatus"].ToString() == "2" ? cardStr + "（新）、" : cardStr + "、";
                    cnt++;
                }
                main.gameLog.Document.Blocks.Add(p);
                main.gameLog.ScrollToEnd();
                LogsHelper.LogGame(logs.Substring(0, logs.Length - 1));
            }
        }

        public static void SuppressScriptErrors(System.Windows.Controls.WebBrowser webBrowser, bool hide)
        {
            webBrowser.Navigating += (s, e) =>
            {
                var fiComWebBrowser = typeof(System.Windows.Controls.WebBrowser).GetField("_axIWebBrowser2", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                if (fiComWebBrowser == null)
                    return;

                object objComWebBrowser = fiComWebBrowser.GetValue(webBrowser);
                if (objComWebBrowser == null)
                    return;

                objComWebBrowser.GetType().InvokeMember("Silent", System.Reflection.BindingFlags.SetProperty, null, objComWebBrowser, new object[] { hide });
            };
        }

        public static void SetIEConfig()
        {
            string exeName = Process.GetCurrentProcess().ProcessName + ".exe";
            string ieVer = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Internet Explorer", "svcVersion", "");
            if (ieVer == null || ieVer == "")
            {
                ieVer = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Internet Explorer", "Version", "");
            }
            if (ieVer != null && ieVer != "")
            {
                string[] verInfo = ieVer.Split('.');
                int setValue = 9999;
                switch (verInfo[0])
                {
                    case "11":
                        setValue = 11001;
                        break;
                    case "10":
                        setValue = 10001;
                        break;
                    case "9":
                        setValue = 9999;
                        break;
                    case "8":
                        setValue = 8888;
                        break;
                }
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION",
                    exeName, setValue, RegistryValueKind.DWord);
            }
        }

        /// <summary>
        /// 截图游戏并保存
        /// </summary>
        public static void ScreenShot()
        {
            if (!Directory.Exists("screenshot"))
            {
                Directory.CreateDirectory("screenshot");
            }
            string path = @"screenshot\" + LogsHelper.GetServerName() + "_" + LogsHelper.GetFilePlayerName() + "_" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff") + "." + DataUtil.Config.sysConfig.capFormat.ToString().ToLower();

            var document = main.mainWeb.Document as HTMLDocument;
            if (document == null)
            {
                return;
            }

            if (document.url.Contains(".swf?"))
            {
                var viewObject = document.getElementsByTagName("embed").item(0, 0) as IViewObject;
                if (viewObject == null)
                {
                    return;
                }

                var width = ((HTMLEmbed)viewObject).clientWidth;
                var height = ((HTMLEmbed)viewObject).clientHeight;
                TakeScreenshot(width, height, viewObject, path);
            }
            else
            {
                var gameFrame = document.getElementById("game_frame").document as HTMLDocument;
                if (gameFrame == null)
                {
                    return;
                }

                var frames = document.frames;
                for (var i = 0; i < frames.length; i++)
                {
                    var item = frames.item(i);
                    var provider = item as IServiceProvider;
                    if (provider == null) continue;

                    object ppvObject;
                    provider.QueryService(typeof(IWebBrowserApp).GUID, typeof(IWebBrowser2).GUID, out ppvObject);
                    var webBrowser = ppvObject as IWebBrowser2;

                    var iframeDocument = webBrowser?.Document as HTMLDocument;
                    if (iframeDocument == null) continue;

                    IViewObject viewObject = null;
                    int width = 0, height = 0;
                    var swf = iframeDocument.getElementById("externalswf");
                    if (swf == null) continue;
                    Func<dynamic, bool> function = target =>
                    {
                        if (target == null) return false;
                        viewObject = target as IViewObject;
                        if (viewObject == null) return false;
                        width = int.Parse(target.width);
                        height = int.Parse(target.height);
                        return true;
                    };
                    if (!function(swf as HTMLEmbed) && !function(swf as HTMLObjectElement)) continue;

                    TakeScreenshot(width, height, viewObject, path);

                    break;
                }
            }
        }

        /// <summary>
        /// 截图保存函数
        /// </summary>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="viewObject">操作对象</param>
        /// <param name="path">截图文件名</param>
        private static void TakeScreenshot(int width, int height, IViewObject viewObject, string path)
        {
            try
            {
                var image = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                var rect = new RECT { left = 0, top = 0, width = width, height = height, };
                var tdevice = new DVTARGETDEVICE { tdSize = 0, };

                using (var graphics = Graphics.FromImage(image))
                {
                    var hdc = graphics.GetHdc();
                    viewObject.Draw(1, 0, IntPtr.Zero, tdevice, IntPtr.Zero, hdc, rect, null, IntPtr.Zero, IntPtr.Zero);
                    graphics.ReleaseHdc(hdc);
                }

                var format = Path.GetExtension(path) == ".jpg"
                    ? ImageFormat.Jpeg
                    : ImageFormat.Png;

                image.Save(path, format);
                AddLog("截图已经保存到文件" + path, LogType.System);
            }
            catch
            {
                AddLog("截图保存失败！", LogType.System);
            }
        }

        /// <summary>
        /// 控制显示主窗口地图信息按钮
        /// </summary>
        /// <param name="show">是否显示，默认显示</param>
        public static void ShowMapInfoButton(bool show=true)
        {
            if (!main.Dispatcher.CheckAccess())
            {
                main.Dispatcher.Invoke(new Action(() =>
                {
                    main.btnMapInfo.Visibility = show ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
                }));
            }
            else
            {
                main.btnMapInfo.Visibility = show ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
            }
        }
    }
}
