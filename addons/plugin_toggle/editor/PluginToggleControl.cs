#if TOOLS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Godot;
using plugin_toggle.util;
using static plugin_toggle.util.PlugInToggleNodeExtension;

namespace addons.plugin_toggle.editor;

[Tool]
public partial class PluginToggleControl : PanelContainer {
    private const string configPath = "res://addons/plugin_toggle/plugin_toggle.cfg";
    private const string enabledKey = "enabled";
    private const string useToggleKey = "use_toggle";
    private const string visibleKey = "visible";

    private StringName pressed = BaseButton.SignalName.Pressed;
    private StringName focusExited = Window.SignalName.FocusExited;
    private StringName closeRequested = Window.SignalName.CloseRequested;
    
    
    [Export] private Button refreshButton;
    [Export] private CheckBox checkBox;
    [Export] private Button menuToggleButton;
    [Export] private PopupPanel popupPanel;
    [Export] private PluginToggleMenu menu;

    public StringName PluginName { get; set; }
    
    private ConfigFile configFile;
    //<plugin name, { enabled: bool, use_toggle: bool, visible: bool }>
    private Godot.Collections.Dictionary<
        string,
        Godot.Collections.Dictionary<string, bool>
    > configData = new();

    private Action<AssemblyLoadContext> unloadDelegate;
    private bool printDebug;

    ///// Godot Functions /////

    public override void _EnterTree() {
        base._EnterTree();

        LoadConfig();
        LoadCurrentPlugins();
        
        refreshButton.TryConnect(pressed, HandleRefreshPressed);
        menuToggleButton.TryConnect(pressed, HandleMenuToggled);
        popupPanel.TryConnect(focusExited, HandleMenuToggled);
        popupPanel.TryConnect(closeRequested, HandleMenuToggled);
        
        unloadDelegate = RegisterUnload(Cleanup);
    }

    public override void _ExitTree() {
        base._ExitTree();
        if(printDebug) GD.Print("[Control] _ExitTree");
        Cleanup();
    }

    private void Cleanup() {
        UnregisterUnload(unloadDelegate);
        
        if (!IsInstanceValid(this)) return;
        
        refreshButton.TryDisconnect(pressed, HandleRefreshPressed);
        menuToggleButton.TryDisconnect(pressed, HandleMenuToggled);
        popupPanel.TryDisconnect(focusExited, HandleMenuToggled);
        popupPanel.TryDisconnect(closeRequested, HandleMenuToggled);
        // configData.Clear();
        // configData = null;
    }
    
    ///// Callback Function /////
    
    private void HandleRefreshPressed() {
        if(printDebug) GD.Print("[Control] Refresh Plugins");

        var reenable = configData
            .Where((dict) => dict.Value[useToggleKey])
            .Select((dict) => dict.Key).ToList();
        Deferred(() => {
            TogglePlugins(reenable);
        });
        
        TogglePlugins();
    }

    private void HandleMenuToggled() {
        if(printDebug) GD.Print($"[Control] toggle {popupPanel.Visible}");
        
        if (!popupPanel.Visible) {
            if(printDebug) GD.Print($"[Control] open");
            LoadConfig();
            LoadCurrentPlugins();
            UpdatePopupContent();
            
            popupPanel.Position = (Vector2I) (
                GetScreenPosition()
                + Size
                + new Vector2(-popupPanel.Size.X, 0)
            );
            popupPanel.Show();    
        } else {
            if(printDebug) GD.Print($"[Control] close");
            // popupPanel.Hide();
            menu.ClearRows();
            SaveConfig();
        }
        
        if(printDebug) GD.Print($"[Control] count: {menu.GetRowContainer().GetChildren().Count}");
    }
    
    ///// Private Functions /////
    
    private void UpdatePopupContent() {
        menu.Reload();
        menu.ClearRows();
        
        foreach (var (name, values) in configData) {
            var item = menu.AddRow(name, values[enabledKey], values[useToggleKey], values[visibleKey]);
            item.Updated += UpdateConfigValue;
        }
    }

    private void LoadCurrentPlugins() {
        var plugins = DirAccess.GetDirectoriesAt("res://addons");

        var configPluginsNames = configData.Keys;
        
        foreach (var name in plugins) {
            var enabled = EditorInterface.Singleton.IsPluginEnabled(name);
            
            if (configPluginsNames.Contains(name)) {
                configPluginsNames.Remove(name);
            }

            SetConfigValue(
                name,
                enabled,
                configData.ContainsKey(name) && configData[name][useToggleKey],
                configData[name][visibleKey]
            );
        }
        
        foreach (var name in configPluginsNames) {
            configData.Remove(name);
        }
    }
    
    private void TogglePlugins() {
        foreach (var (pluginName, values) in configData) {
            if (values[useToggleKey]) {
                TogglePlugin(pluginName);
            } 
        }
    }
    
    private static void TogglePlugins(List<string> plugins) {
        foreach (var pluginName in plugins) {
            TogglePlugin(pluginName);
        }
    }
    
    private static void TogglePlugin(string pluginName, bool? enabled = null) {
        var currentState = EditorInterface.Singleton.IsPluginEnabled(pluginName);
        
        if (!enabled.HasValue || currentState != enabled) {
            EditorInterface.Singleton.SetPluginEnabled(pluginName, !currentState);
        }
    }
    
    private void UpdateConfigValue(string name, bool enabled, bool useToggle, bool visible) {
        menu.UpdateRowVisibility();
        
        SetConfigValue(name, enabled, useToggle, visible);
        SaveConfig();
        
        // close before toggle
        if (name == PluginName && useToggle) {
            var currentState = EditorInterface.Singleton.IsPluginEnabled(name);
            if (currentState != enabled) {
                HandleMenuToggled();
            }
        }
        
        // delay toggle so the closing of the menu can finish
        Deferred(() => TogglePlugin(name, enabled));
    }
    
    private void SetConfigValue(string name, bool enabled, bool useToggle, bool visible) {
        if (configData.ContainsKey(name)) {
            configData[name][enabledKey] = enabled;
            configData[name][useToggleKey] = useToggle;
            configData[name][visibleKey] = visible;
        } else {
            configData.Add(
                name,
                new() {
                    {enabledKey, enabled},
                    {useToggleKey, useToggle},
                    {visibleKey, visible}
                }
            );    
        }
    }
    
    private void LoadConfig() {
        configFile = new ConfigFile();
        var error = configFile.Load(configPath);

        configData = new();
        if (error == Error.Ok) {
            var keys = configFile.GetSections();
            foreach (var name in keys) {
                configData.Add(
                    name,
                    new() {
                        {enabledKey, configFile.GetValue(name, enabledKey).AsBool()},
                        {useToggleKey, configFile.GetValue(name, useToggleKey, false).AsBool()},
                        {visibleKey, configFile.GetValue(name, visibleKey, true).AsBool()}
                    }
                );
            }
        }
    }
    
    private void SaveConfig() {
        configFile = new ConfigFile();
        if(printDebug) GD.Print($"[Control] Save Toggle Config");
        foreach (var (name, value) in configData) {
            configFile.SetValue(name, enabledKey, value[enabledKey]);
            configFile.SetValue(name, useToggleKey, value[useToggleKey]);
            configFile.SetValue(name, visibleKey, value[visibleKey]);
        }
        configFile.Save(configPath);
    }

    ///// Helper Function /////
    
    private async void Deferred(Action action) {
        await Task.Yield();
        
        action.Invoke();
    }
}
#endif