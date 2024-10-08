﻿using Dalamud.Game.Command;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface.GameFonts;
using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.Interface.Style;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using Dalamud.Plugin;
using Lumina.Excel.GeneratedSheets;
using Lumina.Excel;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Miosuke;


namespace SimpleChatFilters;

public sealed partial class Plugin : IDalamudPlugin
{
    public string Name => "SimpleChatFilters";
    private const string CommandMainWindow = "/scf";


    public bool PluginThemeEnabled { get; set; }
    public StyleModel PluginTheme { get; set; }


    public IFontHandle Axis20 { get; set; }


    public SimpleChatFiltersConfig Config { get; init; }
    public WindowSystem WindowSystem = new("SimpleChatFilters");
    public ConfigWindow ConfigWindow { get; init; }
    public MainWindow MainWindow { get; init; }


    public SimpleChatHandler SimpleChatHandler { get; set; } = null!;
    public SimpleToastHandler SimpleToastHandler { get; set; } = null!;




    public Plugin(IDalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<Service>();

        Service.Log.Info($"[General] Plugin loading...");

        PluginTheme = StyleModel.Deserialize("DS1H4sIAAAAAAAACq1XS3PbOAz+LzpnMnpQlORbE2+bQ7uTadLpbm+MzdiqFcsry27TTP97CZIgIcr2etLqIojChxcBEHyJRDRJLuOL6CGavET/RBMOH//q98+LaBZNGCzMo0kMb2m5YssVX+aK61HJuIgWdnlpJdb2WzxY4qsFMwvOtIpVNElhobFcT4EhhmsdYJlebYPVVK9uRkbC6n/qt7arU/bpX1sL7NWC0nIR7awpe/vjm/3+bmU9O8E58f7HQXVC2OVs4MesVX6+RPfye2//J/a/fn+x78/6rfiBcVpvxUMj52PtGqDfDvC5Xs/bb1cLb1RRsizJCgSRbw0m30pIfGk/mZJ1vaybORWFIhBqIaD3tt3sNpS3ROYSuUvLXoHsq7aby86xs8yyA0GcMiE2zHdLoVw7y5q3nXiSxJq0ssxAaG4gjHzm+W/avexooBlGmqFVzMII6s2sr/e+MjiCOII4gngBW1r3DbUtSQrOiphblP/UWP9prC3LNMszLyZQnlUsLqoUg5nHPCmd8ayKk4q7fRjJum6bRmy2JACvE/dBrndXoqM+YkyAMH4xkrR3s06pfhhATm2v43/XQXdBYxMLAUJjgBgrAVC40wyxQBjvjmKDkOcIBcIkCYVeL+Vs9UF0KwcocauB0AAgvK6mVsk+8OxoHoaIMBULTMUCU7EguKtd37fYWVXloyNAaG4gXLYb7jBwWZUmOS9RUZ5WLEmw5HlaMBsMKPs8rsrYiwpTl+lcx77BEPyFJNdY1I0UtI/kWOhAGCQWepY49lGdH2yoal8cIgwsxhUdpbshN6ITfXtuc3P8rw8t6dlO2qgxvGKfPspt/UO+62p/pBYYYCBMkmCA03wACd0pcGeBMMiS9kWP/BOm35PySTH+QJjuj5lUcsP7G7GH9pxXRsyrLR9I+bR+bGc72odjjsFLVM9VDwpJWBmrxxVGkbJARmBSkuUWbyLCtTjMY5bSvjJtZ6t6vbjt5L6W/uBNeVhjIM644VF/PW36Z3oEo0bcAqLotmn79/Vabn2BYS8CwoQrOQQY7hvOUqTSsjyA3dTbvl2ok9vpchk9OGkOII4pw+gNBjcY2UzvoOcg6gJiFAaNsbNO37XrxamjLT8MfF8vlv2pzB/hPg7HxRPHrmd/0/zf+Ao5a+fXO9nIWS/pJHkihTLoItNOLKZdu7kX3UIeU+WNg7r5W+xvlO/N0P9jeoz/CmPmZZWvIfi4Y0WAnNZPxDWsLKxQ9CuFwaidi8bgzgOpYMDlR42Z0SSSzfNmKSJ1GzOXCDFKxGFcjI++DRZBntNRQF30zuA67woiz7rZPHrn3cmgKRMAezYY3sXI0yLGDaUyl6M05gc01yMu100z/RBevLOCRphB2IlIr47mKW2qjZcYhrriNIy+QaFSN3gTLj++qenpYHkYPrwqe6dZZkI4DI4/6Tn8N80jdhYOgqOu1MG25DH2JyrTT0Klm33VvQIbe0632h8BpTukcHu0dMupWFUzVqs/fwHiVFw/xBAAAA==")!;

        Axis20 = Service.PluginInterface.UiBuilder.FontAtlas.NewGameFontHandle(new GameFontStyle(GameFontFamily.Axis, 20.0f));

        Config = Service.PluginInterface.GetPluginConfig() as SimpleChatFiltersConfig ?? new SimpleChatFiltersConfig();
        Config.Initialize(Service.PluginInterface);
        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this);
        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);

        SimpleChatHandler = new SimpleChatHandler(this);
        SimpleToastHandler = new SimpleToastHandler(this);

        Service.PluginInterface.UiBuilder.Draw += DrawUI;
        Service.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
        Service.PluginInterface.UiBuilder.OpenMainUi += DrawMainUI;
        Service.Framework.Update += OnFrameUpdate;

        Service.Commands.AddHandler(CommandMainWindow, new CommandInfo(OnCommandMainWindow) { HelpMessage = "Open the main window." });

        Service.Log.Info($"[General] Plugin initialised");
    }

    public void Dispose()
    {
        Service.Commands.RemoveHandler(CommandMainWindow);

        Service.PluginInterface.UiBuilder.Draw -= DrawUI;
        Service.PluginInterface.UiBuilder.OpenConfigUi -= DrawConfigUI;
        Service.PluginInterface.UiBuilder.OpenMainUi -= DrawMainUI;
        Service.Framework.Update -= OnFrameUpdate;

        WindowSystem.RemoveAllWindows();
        ConfigWindow.Dispose();
        MainWindow.Dispose();

        SimpleChatHandler.Dispose();
        SimpleToastHandler.Dispose();

        Axis20.Dispose();
    }

    public void DrawUI()
    {
        WindowSystem.Draw();
    }

    public void DrawMainUI()
    {
        MainWindow.Toggle();
    }

    public void DrawConfigUI()
    {
        ConfigWindow.Toggle();
    }

    public void OnCommandMainWindow(string command, string args)
    {
        switch (args)
        {
            case "":
                MainWindow.Toggle();
                break;
            case "config":
                ConfigWindow.Toggle();
                break;
        }
    }

    public void OnFrameUpdate(IFramework framework)
    {
    }
}
