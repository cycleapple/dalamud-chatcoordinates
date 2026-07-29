using System;
using System.Linq;
using System.Numerics;
using Dalamud.Game.Text;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using Dalamud.Bindings.ImGui;

namespace ChatCoordinates.Configuration
{
    public class ConfigUi : IDisposable
    {
        private CCPlugin _plugin;

        private bool _visible = false;

        public bool Visible
        {
            get => _visible;
            set => _visible = value;
        }

        public ConfigUi(CCPlugin plugin)
        {
            _plugin = plugin;
            _plugin.Interface.UiBuilder.Draw += DrawUI;
            _plugin.Interface.UiBuilder.OpenConfigUi += DrawConfigUI;
        }

        public void DrawConfigUI()
        {
            Visible = true;
        }
        public void DrawUI()
        {
            if (!Visible) return;

            var zoneDelimiter = _plugin.Configuration.ZoneDelimiter;
            var xivChatTypes = Enum.GetValues(typeof(XivChatType)).Cast<XivChatType>().ToList();
            
            ImGui.SetNextWindowSize(new Vector2(320, 320), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSizeConstraints(new Vector2(320, 320), new Vector2(float.MaxValue, float.MaxValue));

            if (ImGui.Begin($"{_plugin.Name}－設定", ref _visible))
            {
                ImGui.Text("地區分隔符號");
                if (ImGui.InputText("##CoordZone", ref zoneDelimiter, 10))
                {
                    if (!string.IsNullOrWhiteSpace(zoneDelimiter))
                    {
                        _plugin.Configuration.ZoneDelimiter = zoneDelimiter;
                    }
                }

                ImGui.TextColored(ImGuiColors.DalamudGrey,
                    "用來判斷指定地區的分隔符號。");

                ImGui.Spacing();

                ImGui.TextUnformatted("一般訊息頻道");

                if (ImGui.BeginCombo("##CoordChat", _plugin.Configuration.GeneralChatType.ToString()))
                {
                    foreach (var chatType in xivChatTypes.Where(chatType =>
                        ImGui.Selectable(chatType.ToString(), chatType == _plugin.Configuration.GeneralChatType)))
                    {
                        _plugin.Configuration.GeneralChatType = chatType;
                    }

                    ImGui.EndCombo();
                }

                ImGui.TextColored(ImGuiColors.DalamudGrey,
                    "ChatCoordinates 輸出一般訊息時使用的聊天頻道。");

                ImGuiHelpers.ScaledDummy(5);

                ImGui.TextUnformatted("錯誤訊息頻道");
                if (ImGui.BeginCombo("##CoordErr", _plugin.Configuration.ErrorChatType.ToString()))
                {
                    foreach (var chatType in xivChatTypes.Where(chatType =>
                        ImGui.Selectable(chatType.ToString(), chatType == _plugin.Configuration.ErrorChatType)))
                    {
                        _plugin.Configuration.ErrorChatType = chatType;
                    }

                    ImGui.EndCombo();
                }

                ImGui.TextColored(ImGuiColors.DalamudGrey,
                    "ChatCoordinates 輸出錯誤訊息時使用的聊天頻道。");

                ImGuiHelpers.ScaledDummy(5);

                ImGui.Spacing();
                ImGui.Spacing();
                ImGui.TextColored(ImGuiColors.DPSRed, zoneDelimiter);
                ImGui.SameLine();
                ImGui.TextColored(ImGuiColors.DalamudGrey, "可作為分隔符號，在指定地區設定旗標。");
                ImGui.TextColored(ImGuiColors.DalamudGrey, "輸入 <flag> 即可分享已設定的旗標。");
                ImGui.TextColored(ImGuiColors.DalamudGrey, "範例：");
                ImGui.TextColored(ImGuiColors.DalamudGrey, $"/coord <x> <y> [{zoneDelimiter} <地區>]");
                ImGui.TextColored(ImGuiColors.DalamudGrey, $"/coord 8.8 11.5");
                ImGui.TextColored(ImGuiColors.DalamudGrey, $"/coord 8.8,11.5");
                ImGui.TextColored(ImGuiColors.DalamudGrey, $"/coord X: 10.7 Y: 11.7 : 雷克蘭德");
                ImGui.TextColored(ImGuiColors.DalamudGrey, $"/coord 10.7 11.7 : 雷克");

                if (ImGui.Button("儲存並關閉"))
                {
                    _plugin.Configuration.Save();
                    Visible = false;
                }
            }

            ImGui.End();
        }

        public void Dispose()
        {
        }
    }
}
