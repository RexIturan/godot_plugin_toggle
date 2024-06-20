#if TOOLS
using addons.plugin_toggle.editor;
using Godot;
using plugin_toggle.util;

namespace addons.plugin_toggle;

[Tool]
public partial class PlugInTogglePlugin : EditorPlugin {
    private const string pluginName = "plugin_toggle";
    
    private PackedScene pluginToggleControl =
        ResourceLoader.Load<PackedScene>(
            "res://addons/plugin_toggle/editor/plugin_toggle_control.tscn");
    private PluginToggleControl Instance; 
    
    public override void _EnterTree() {
        // Initialization of the plugin goes here.
        Instance = pluginToggleControl.Instantiate<PluginToggleControl>();
        Instance.PluginName = pluginName;
        AddControlToContainer(CustomControlContainer.Toolbar, Instance);
        var toolbarContainer = Instance.GetParent();
        
        // remove extra version of this plugin
        var pluginControls = toolbarContainer.GetNodes<PluginToggleControl>();
        if (pluginControls.Count > 1) {
            GD.PrintErr("Duplicate Plugin controls found");
            foreach (var control in pluginControls) {
                if (control != Instance) {
                    control.QueueFree();
                }
            }    
        }
        
        toolbarContainer.MoveChild(Instance, 4);
    }

    public override void _ExitTree() {
        // Clean-up of the plugin goes here.
        RemoveControlFromContainer(CustomControlContainer.Toolbar, Instance);
        Instance.QueueFree();
    }
}
#endif