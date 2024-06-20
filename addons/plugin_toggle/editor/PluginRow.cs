#if TOOLS
using System;
using System.Runtime.Loader;
using Godot;
using plugin_toggle.util;
using static plugin_toggle.util.PlugInToggleNodeExtension;

namespace addons.plugin_toggle.editor;

[Tool]
public partial class PluginRow : HBoxContainer {
    private StringName toggled = BaseButton.SignalName.Toggled;
    [Export] private CheckBox isEnabledCheckBox;
    [Export] private CheckBox shouldToggleCheckBox;
    [Export] private CheckBox hiddenCheckbox;
    [Export] private LineEdit nameLabel;

    private bool printDebug;
    
    [Signal]
    public delegate void UpdatedEventHandler(string name, bool enabled, bool useToggle, bool visible);

    public string PluginName {
        get => nameLabel?.Text ?? "";
        set {
            if (nameLabel is null) return;
            nameLabel.Text = value;
            nameLabel.TooltipText = value;
        }
    }

    private bool enabled = false;
    public bool PluginEnabled {
        get => enabled;
        set {
            enabled = value;

            if (printDebug) GD.Print($"[PluginRow] change PluginEnabled");    
            
            if (isEnabledCheckBox is not null && isEnabledCheckBox.ButtonPressed != enabled) {
                isEnabledCheckBox.SetPressedNoSignal(enabled);    
            }
            
            EmitSignal(SignalName.Updated, PluginName, enabled, PluginUseToggle, PluginVisible);
        }
    }

    private bool useToggle = false;
    public bool PluginUseToggle {
        get => useToggle;
        set {
            useToggle = value;
            
            if (printDebug) GD.Print($"[PluginRow] change PluginUseToggle");
            if (shouldToggleCheckBox is not null && shouldToggleCheckBox.ButtonPressed != enabled) {
                shouldToggleCheckBox.SetPressedNoSignal(useToggle);
            }
            
            EmitSignal(SignalName.Updated, PluginName, PluginEnabled, useToggle, PluginVisible);
        }
    }

    private bool isVisible = true;
    public bool PluginVisible {
        get => isVisible;
        set  {
            isVisible = value;
            
            if (printDebug) GD.Print($"[PluginRow] change PluginUseToggle");
            if (hiddenCheckbox is not null && hiddenCheckbox.ButtonPressed != enabled) {
                hiddenCheckbox.SetPressedNoSignal(isVisible);
            }
            
            EmitSignal(SignalName.Updated, PluginName, PluginEnabled, PluginUseToggle, isVisible);
        }
    }

    // private Action<string, bool, bool> updateFunction;

    ///// Tool Helper /////
    
    private Action<AssemblyLoadContext> unloadHandle;
    
    ///// Godot Functions /////
    
    public override void _EnterTree() {
        base._EnterTree();

        isEnabledCheckBox.TryConnect<bool>(toggled, HandleEnabledToggled);
        shouldToggleCheckBox.TryConnect<bool>(toggled, HandleUseToggleToggled);
        hiddenCheckbox.TryConnect<bool>(toggled, HandleVisibleToggled);

        unloadHandle = RegisterUnload(Cleanup);
    }

    public override void _ExitTree() {
        base._ExitTree();

        Cleanup();
    }

    private void Cleanup() {
        UnregisterUnload(unloadHandle);

        if (!IsInstanceValid(this)) return;
        isEnabledCheckBox.TryDisconnect<bool>(toggled, HandleEnabledToggled);
        shouldToggleCheckBox.TryDisconnect<bool>(toggled, HandleUseToggleToggled);
        hiddenCheckbox.TryDisconnect<bool>(toggled, HandleVisibleToggled);

        // this.TryDisconnect(SignalName.Updated, updateFunction);
        // updateFunction = null;
    }

    ///// Callback Functions /////
    
    private void HandleEnabledToggled(bool toggledOn) {
        if (printDebug) GD.Print($"[PluginRow] HandleEnabledToggled {toggledOn}");
        PluginEnabled = toggledOn;
    }
    
    private void HandleUseToggleToggled(bool toggledOn) {
        if (printDebug) GD.Print($"[PluginRow] HandleUseToggleToggled {toggledOn}");
        PluginUseToggle = toggledOn;
    }
    
    private void HandleVisibleToggled(bool toggledOn) {
        if (printDebug) GD.Print($"[PluginRow] HandleVisibleToggled {toggledOn}");
        PluginVisible = toggledOn;
    }
    
    //
    // public void RegisterUpdated(Action<string, bool, bool> updateConfigValue) {
    //     updateFunction = updateConfigValue;
    //     this.TryConnect(SignalName.Updated, updateFunction);
    // }
}
#endif