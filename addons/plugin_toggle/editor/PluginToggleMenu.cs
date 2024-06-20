#if TOOLS
using Godot;
using plugin_toggle.util;

namespace addons.plugin_toggle.editor;

[Tool]
public partial class PluginToggleMenu : MarginContainer {
    [Export] private CheckBox visibilityCheckbox;
    [Export] private Container pluginTable;

    private PackedScene rowPrefab;

    private bool ShowHidden { get; set; } = false;
    
    ///// Godot Functions /////

    public override void _EnterTree() {
        base._EnterTree();

        Reload();

        visibilityCheckbox.ButtonPressed = ShowHidden;
        
        visibilityCheckbox.TryConnect<bool>(BaseButton.SignalName.Toggled, ToggleVisibility);
    }

    public override void _ExitTree() {
        base._ExitTree();
        Cleanup();
    }

    private void Cleanup() {
        if (!IsInstanceValid(this)) return;
        
        visibilityCheckbox.TryDisconnect<bool>(BaseButton.SignalName.Toggled, ToggleVisibility);
    }
    
    ///// Public Functions /////
    
    public void ClearRows() {
        pluginTable.RemoveAndQueueFreeChildren();
    }

    public PluginRow AddRow(string name, bool enabled, bool useToggle, bool visible) {
        var row = CreateRow(name, enabled, useToggle, visible);
        pluginTable.AddChild(row);
        UpdateRowVisibility();
        return row;
    }
    
    public void Reload() {
        rowPrefab = ResourceLoader.Load<PackedScene>("res://addons/plugin_toggle/editor/plugin_row.tscn");
    }

    public void UpdateRowVisibility() {
        var rows = pluginTable.GetNodes<PluginRow>();

        foreach (var pluginRow in rows) {
            pluginRow.Visible = ShowHidden | pluginRow.PluginVisible;
        }
    }
    
    ///// Callback Functions /////
    
    private void ToggleVisibility(bool newValue) {
        ShowHidden = newValue;
        UpdateRowVisibility();
    }
    
    ///// Private Functions /////
    
    private PluginRow CreateRow(string name, bool enabled, bool useToggle, bool visible) {
        var row = rowPrefab.InstantiateOrNull<PluginRow>();

        row.PluginName = name;
        row.PluginEnabled = enabled;
        row.PluginUseToggle = useToggle;
        row.PluginVisible = visible;
        
        return row;
    }

    public Node GetRowContainer() {
        return pluginTable;
    }
}
#endif