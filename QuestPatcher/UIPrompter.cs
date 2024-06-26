﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Avalonia.Controls;
using QuestPatcher.Core;
using QuestPatcher.Core.Models;
using QuestPatcher.Services;
using QuestPatcher.ViewModels;
using QuestPatcher.Views;
using QuestPatcher.Utils;
using SemVer = SemanticVersioning.Version;

namespace QuestPatcher
{
    public class UIPrompter : IUserPrompter
    {
        private Window? _mainWindow;
        private Config? _config;
        private QuestPatcherUiService? _uiService;
        private SpecialFolders? _specialFolders;

        /// <summary>
        /// This exists instead of a constructor since the prompter must be immediately passed on QuestPatcherService's creation, so we initialise its members after the fact.
        /// Maybe there's a better workaround, but this works fine for now
        /// </summary>
        public void Init(Window mainWindow, Config config, QuestPatcherUiService uiService, SpecialFolders specialFolders)
        {
            _mainWindow = mainWindow;
            _config = config;
            _uiService = uiService;
            _specialFolders = specialFolders;
        }
        
        public async Task<bool> CheckUpdate()
        {
#if DEBUG
            return true;
#endif
            
            try
            {
                JsonNode? res = null;
                using HttpClient client = new();
                client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/97.0.4692.71 Safari/537.36");
                client.DefaultRequestHeaders.Add("accept", "application/json");
                // try
                // {
                //     res = JsonNode.Parse(await client.GetStringAsync(@"https://beatmods.wgzeyu.com/githubapi/MicroCBer/QuestPatcher/latest"));
                // }
                // catch (Exception e)
                // {
                //     res = JsonNode.Parse(await client.GetStringAsync(@"https://api.github.com/repos/MicroCBer/QuestPatcher/releases/latest"));
                // }
                
                res = JsonNode.Parse(await client.GetStringAsync(@"https://api.github.com/repos/MicroCBer/QuestPatcher/releases/latest"));
                
                string? newest = res?["tag_name"]?.ToString();
                if (newest == null) throw new Exception("Failed to check update.");

                bool isLatest = SemVer.TryParse(newest, out var latest) && latest == VersionUtil.QuestPatcherVersion;
                
                if (!isLatest)
                {
                    DialogBuilder builder = new()
                    {
                        Title = "有更新！",
                        Text = $"**不更新软件，可能会遇到未知问题，强烈建议更新至最新版**\n" +
                        $"同时，非最新版本将不受支持且不保证没有安全问题\n\n" +
                        $"您的版本 - v{VersionUtil.QuestPatcherVersion}\n" +
                        $"最新版本 - v{latest?.ToString() ?? newest}",
                        HideOkButton = true,
                        HideCancelButton = true
                    };
                    builder.WithButtons(
                        new ButtonInfo
                         {
                             Text = "进入QP教程",
                             CloseDialogue = true,
                             ReturnValue = true,
                             OnClick = () => Util.OpenWebpage("https://bs.wgzeyu.com/oq-guide-qp/#install_qp")
                         }, 
                        new ButtonInfo
                        {
                            Text = "进入网盘下载",
                            CloseDialogue = true,
                            ReturnValue = true,
                            OnClick = () => Util.OpenWebpage("http://share.wgzeyu.vip/?Ly8lRTUlQjclQTUlRTUlODUlQjclRUYlQkMlODglRTUlQTYlODJNb2QlRTUlQUUlODklRTglQTMlODUlRTUlOTklQTglRTMlODAlODElRTglQjAlQjElRTklOUQlQTIlRTclQkMlOTYlRTglQkUlOTElRTUlOTklQTglRTclQUQlODlCUyVFNyU5QiVCOCVFNSU4NSVCMyVFOCVCRCVBRiVFNCVCQiVCNiVFRiVCQyU4OS8=")
                        },
                        new ButtonInfo
                        {
                            Text = "进入GitHub下载",
                            CloseDialogue = true,
                            ReturnValue = true,
                            OnClick = () => Util.OpenWebpage("https://github.com/MicroCBer/QuestPatcher/releases/latest")
                        });

                    await builder.OpenDialogue(_mainWindow);
                }
                return true;
            }
            catch (Exception ex)
            {
                DialogBuilder builder = new()
                {
                    Title = "检查更新失败"+ex,
                    Text = "请手动检查更新",
                    HideOkButton = true
                };
                builder.WithButtons(
                    new ButtonInfo
                    {
                        Text = "进入QP教程",
                        CloseDialogue = true,
                        ReturnValue = true,
                        OnClick = () => Util.OpenWebpage("https://bs.wgzeyu.com/oq-guide-qp/#install_qp")
                    }, 
                    new ButtonInfo
                    {
                        Text = "进入网盘下载",
                        CloseDialogue = true,
                        ReturnValue = true,
                        OnClick = () => Util.OpenWebpage("http://share.wgzeyu.vip/?Ly8lRTUlQjclQTUlRTUlODUlQjclRUYlQkMlODglRTUlQTYlODJNb2QlRTUlQUUlODklRTglQTMlODUlRTUlOTklQTglRTMlODAlODElRTglQjAlQjElRTklOUQlQTIlRTclQkMlOTYlRTglQkUlOTElRTUlOTklQTglRTclQUQlODlCUyVFNyU5QiVCOCVFNSU4NSVCMyVFOCVCRCVBRiVFNCVCQiVCNiVFRiVCQyU4OS8=")
                    },
                    new ButtonInfo
                    {
                        Text = "进入GitHub下载",
                        CloseDialogue = true,
                        ReturnValue = true,
                        OnClick = () => Util.OpenWebpage("https://github.com/MicroCBer/QuestPatcher/releases/latest")
                    });

                await builder.OpenDialogue(_mainWindow);
                return false; 
            }
        }
        
        public Task<bool> PromptAppNotInstalled()
        {
            Debug.Assert(_config != null);

            DialogBuilder builder = new()
            {
                Title = "应用未安装",
                Text = $"所选择的APP - {_config.AppId} - 没有在你的Quest2上安装",
                HideOkButton = true
            };
            builder.WithButtons(
                new ButtonInfo
                {
                    Text = "切换APP",
                    CloseDialogue = true,
                    ReturnValue = true,
                    OnClick = async () =>
                    {
                        Debug.Assert(_uiService != null);
                        await _uiService.OpenChangeAppMenu(true);
                    }
                }
            );

            return builder.OpenDialogue(_mainWindow);
        }

        public Task<bool> PromptAdbDisconnect(DisconnectionType type)
        {
            DialogBuilder builder = new();
            builder.OkButton.Text = "重试";
            switch (type)
            {
                case DisconnectionType.NoDevice:
                    builder.Title = "Quest没有连接";
                    builder.Text = "QuestPatcher 无法检测到你的 Quest2.\n请检查你的 Quest2 是否插入了电脑, 并且是否已按照 SideQuest 安装说明设置开发人员模式。";
                    builder.WithButtons(
                        new ButtonInfo
                        {
                            Text = "BeatSaber新手教程",
                            OnClick = () => Util.OpenWebpage("https://bs.wgzeyu.com/oq-guide-qp/")
                        }
                    );
                    break;
                case DisconnectionType.DeviceOffline:
                    builder.Title = "设备离线";
                    builder.Text = "已检测到您的 Quest 处于离线状态。\n请尝试重新启动您的 Quest 和您的电脑";
                    break;
                case DisconnectionType.Unauthorized:
                    builder.Title = "设备未经授权";
                    builder.Text = "请在头显中允许此PC的连接（即使您之前已为 SideQuest 执行过此操作）";
                    break;
                default:
                    throw new NotImplementedException($"Variant {type} has no fallback/dialogue box");
            }

            return builder.OpenDialogue(_mainWindow);
        }

        public Task<bool> PromptUnstrippedUnityUnavailable()
        {
            DialogBuilder builder = new()
            {
                Title = "缺少libunity.so",
                Text = "你准备Mod的应用暂时还没有未剥离的libunity.so，" +
                        "这可能导致某些Mod不能正常运行，直到它被添加到索引中。" +
                        "请谨慎操作 - 如果你是从旧版升级上来，最好等待它更新再打Mod。"
            };
            builder.OkButton.Text = "仍然继续";

            return builder.OpenDialogue(_mainWindow);
        }

        public Task<bool> Prompt32Bit()
        {
            DialogBuilder builder = new()
            {
                Title = "32 bit APK",
                Text = "您尝试打补丁的应用程序是 32 位 (armeabi-v7a)。 QuestPatcher 支持 32 版本的 QuestLoader，但大多数库（如 beatsaber-hook）不支持，除非您使用非常旧的版本。" +
                        "这将使打补丁变得更加困难。"
            };
            builder.OkButton.Text = "仍然继续";

            return builder.OpenDialogue(_mainWindow);
        }

        public Task<bool> PromptUnknownModLoader()
        {
            DialogBuilder builder = new()
            {
                Title = "检测到了未知的Mod注入器",
                Text = "您尝试打补丁的应用包含了一个 QuestPatcher 无法识别的Mod注入器。" + 
                "QuestPatcher 会尝试将已有的注入器替换为你选择的，但是这可能会导致最终的文件无法正常运行。"
            };
            builder.OkButton.Text = "仍然继续";

            return builder.OpenDialogue(_mainWindow);
        }

        public Task PromptUpgradeFromOld()
        {
            DialogBuilder builder = new()
            {
                Title = "Upgrading from QuestPatcher 1",
                Text = "It looks as though you've previously used QuestPatcher 1.\n\n" +
                    "Note that your mods from QuestPatcher 1 will be removed - this is deliberate as QuestPatcher 2 reworks mod installing to allow toggling of mods! " +
                    "To get your mods back, just reinstall them.\n\n" +
                    "NOTE: All save data, custom maps and cosmetics will remain safe!",
                HideCancelButton = true
            };

            return builder.OpenDialogue(_mainWindow);
        }

        public async Task<AdbDevice?> PromptSelectDevice(List<AdbDevice> devices)
        {
            var viewModel = new SelectDeviceWindowViewModel(devices);
            var window = new SelectDeviceWindow
            {
                DataContext = viewModel,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            viewModel.DeviceSelected += (_, device) => window.Close();
            await window.ShowDialog(_mainWindow!);

            return viewModel.SelectedDevice;
        }
    }
}
