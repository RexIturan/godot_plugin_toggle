using System;
using System.Runtime.Loader;
using Godot;
using plugin_toggle.util;
using static plugin_toggle.util.PlugInToggleNodeExtension;

namespace plugin_toggle.demo;

[Tool]
public partial class Demo : Control {
    [Export] private Button button;
    [Export] private Label label;

    private Action<AssemblyLoadContext> handle;
    
    public override void _EnterTree() {
        base._EnterTree();

        if (label is not null) {
            label.Text = "DEMO";
            
            // label.TreeExiting += () => GD.Print("TreeExiting");
        }

        if (button is not null) {
            button.TryConnect(BaseButton.SignalName.Pressed, HandlePressed);
            
            
            
            // button.Pressed += () => GD.Print("Press");
        }
        
        GD.Print("Enter Tree");

        handle = RegisterUnload(Cleanup);
    }

    public override void _ExitTree() {
        base._ExitTree();

        Cleanup();
    }

    private void Cleanup() {
        UnregisterUnload(handle);

        if (!IsInstanceValid(this)) return;
        
        button.TryDisconnect(BaseButton.SignalName.Pressed, HandlePressed);
    }
    
    private void HandlePressed() {
        GD.Print("Pressed");
    }
}