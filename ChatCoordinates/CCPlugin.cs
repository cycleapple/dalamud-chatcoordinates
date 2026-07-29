using System;
using ChatCoordinates.Configuration;
using ChatCoordinates.Extensions;
using ChatCoordinates.Functions;
using ChatCoordinates.Managers;
using ChatCoordinates.Models;
using Dalamud.Game.Command;
using Dalamud.Game.Text;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace ChatCoordinates
{
    public class CCPlugin : IDalamudPlugin
    {
        public string Name => "ChatCoordinates";
        public Config Configuration { get; init; }
        public ConfigUi ConfigUi { get; init; }
        public IDalamudPluginInterface Interface { get; init; }
        public ICommandManager CommandManager { get; init; }
        public IClientState ClientState { get; init; }
        public IChatGui ChatGui { get; init; }

        private Lazy<AetheryteManager> _aetheryteManager = null!;
        private Lazy<TerritoryManager> _territoryManager = null!;
        public TerritoryManager TerritoryManager => _territoryManager.Value;
        public AetheryteManager AetheryteManager => _aetheryteManager.Value;
        public CoordinateFunctions CoordinateFunctions { get; private set; } = null!;
        public AetheryteFunctions AetheryteFunctions { get; private set; } = null!;

        public CCPlugin(
            IDalamudPluginInterface pluginInterface,
            ICommandManager commandManager,
            IDataManager dataManager,
            IGameGui gameGui,
            IClientState clientState,
            IChatGui chatGui
        )
        {
            Interface = pluginInterface;
            CommandManager = commandManager;
            ClientState = clientState;
            ChatGui = chatGui;

            Configuration = Interface.GetPluginConfig() as Config ?? new Config();
            Configuration.Initialize(Interface);
            ConfigUi = new ConfigUi(this);

            CoordinateFunctions = new CoordinateFunctions(this, gameGui);
            AetheryteFunctions = new AetheryteFunctions(this);
            _territoryManager = new Lazy<TerritoryManager>(() => new TerritoryManager(dataManager));
            _aetheryteManager = new Lazy<AetheryteManager>(() => new AetheryteManager(dataManager));

            CommandManager.AddHandler("/coord", new CommandInfo(OnCoordinateCommand)
            {
                HelpMessage =
                    $"/coord <x> <y> [{Configuration.ZoneDelimiter} <部分地區名稱>]──在指定座標設定地圖旗標"
            });

            CommandManager.AddHandler("/ctp", new CommandInfo(OnCoordinateTeleportCommand)
            {
                HelpMessage =
                    $"/ctp <x> <y> [{Configuration.ZoneDelimiter} <部分地區名稱>]──設定地圖旗標並傳送至最近的傳送網"
            });
        }

        private void OnCoordinateCommand(string cmd, string args)
        {
            ProcessCoordinate(cmd, args);
        }

        private void OnCoordinateTeleportCommand(string cmd, string args)
        {
            var coordinate = ProcessCoordinate(cmd, args);
            if (coordinate == null) return;

            coordinate.Teleport = true;

            AetheryteFunctions.Teleport(coordinate);
        }

        private Coordinate? ProcessCoordinate(string cmd, string args)
        {
            if (ClientState.TerritoryType == 0)
            {
                PrintChat(
                    "無法取得地區資訊，請切換地區以初始化插件。");
                return null;
            }

            if (string.IsNullOrWhiteSpace(args))
            {
                ConfigUi.Visible = true;
                return null;
            }

            if (args.Equals("help", StringComparison.OrdinalIgnoreCase))
            {
                ShowHelp(cmd);
                return null;
            }

            var coordinate = args.ParseCoordinate(this);
            CoordinateFunctions.PlaceMarker(coordinate);

            return coordinate;
        }
        
        private void ShowHelp(string cmd)
        {
            switch (cmd)
            {
                case "/coord":
                    PrintChat(
                        $"在指定座標設定地圖旗標。可使用 {Configuration.ZoneDelimiter} 分隔符號指定地區；輸入 <flag> 即可分享旗標。");
                    PrintChat($"/coord <x> <y> [{Configuration.ZoneDelimiter} <地區>]");
                    PrintChat($"/coord 8.8 11.5");
                    PrintChat("/coord 8.8,11.5");
                    PrintChat("/coord X: 10.7 Y: 11.7 : 雷克蘭德");
                    PrintChat("/coord 10.7 11.7 : 雷克蘭德");
                    break;
                case "/ctp":
                    PrintChat(
                        $"在指定座標設定地圖旗標，並傳送至最近的傳送網。使用 {Configuration.ZoneDelimiter} 分隔符號指定地區。");
                    PrintChat("/ctp 10.7 11.7 : 雷克蘭德");
                    PrintChat("/ctp 10.7 11.7 : 雷克蘭德");
                    PrintChat("/ctp X: 10.7 Y: 11.7 : 雷克蘭德");
                    break;
            }
        }
        
        public void PrintChat(string msg)
        {
            ChatGui.Print(new XivChatEntry()
            {
                Message = msg,
                Type = Configuration.GeneralChatType
            });
        }

        public void PrintError(string msg)
        {
            ChatGui.Print(new XivChatEntry()
            {
                Message = msg,
                Type = Configuration.ErrorChatType
            });
        }
        
        public void PrintChat(XivChatEntry msg)
        {
            ChatGui.Print(msg);
        }

        public void Dispose()
        {
            CommandManager.RemoveHandler("/coord");
            CommandManager.RemoveHandler("/ctp");
            ConfigUi.Dispose();
        }
    }
}
