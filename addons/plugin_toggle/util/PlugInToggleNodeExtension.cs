#if TOOLS
using System;
using System.Reflection;
using System.Runtime.Loader;
using Godot;

namespace plugin_toggle.util;

public static class PlugInToggleNodeExtension {
    public static Action<AssemblyLoadContext> RegisterUnload(Action action) {
        var actionDelegate = (AssemblyLoadContext alc) => action.Invoke();
        
        var assemblyContext = AssemblyLoadContext.GetLoadContext(Assembly.GetExecutingAssembly());
        if (assemblyContext is not null) {
            assemblyContext.Unloading += actionDelegate;
        }
        
        return actionDelegate;
    }
    
    public static void UnregisterUnload(Action<AssemblyLoadContext> actionDelegate) {
        var assemblyContext = AssemblyLoadContext.GetLoadContext(Assembly.GetExecutingAssembly());
        if (assemblyContext is not null) {
            assemblyContext.Unloading -= actionDelegate;
        }
    }

    public static bool IsSignalConnected<T>(this T baseObject, StringName signal, Action action)
        where T : GodotObject
    {
        return baseObject.IsConnected(signal, Callable.From(action));
    }
    
    public static bool IsSignalConnected<
        [MustBeVariant] T0
    >(this GodotObject baseObject, StringName signal, Action<T0> action) {
        return baseObject.IsConnected(signal, Callable.From(action));
    }

    ///// Connect Signals /////
    
    public static void TryConnect(this GodotObject baseObject, StringName signal, Action action) {
        if (!baseObject.IsSignalConnected(signal, action)) {
            baseObject.Connect(signal, Callable.From(action));    
        }
    }
    
    public static void TryConnect<
        [MustBeVariant] T0
    >(this GodotObject baseObject, StringName signal, Action<T0> action) {
        if (!baseObject.IsSignalConnected(signal, action)) {
            baseObject.Connect(signal, Callable.From(action));    
        }
    }
    
    public static void TryConnect<
        [MustBeVariant] T0,
        [MustBeVariant] T1,
        [MustBeVariant] T2
    >(this GodotObject baseObject, StringName signal, Action<T0, T1, T2> action) {
        if (!baseObject.IsConnected(signal, Callable.From(action))) {
            baseObject.Connect(signal, Callable.From(action));    
        }
    }

    ///// Disconnect Signals /////
    
    public static void TryDisconnect(this GodotObject baseObject, StringName signal, Action action) {
        if (action is null) return;
        
        try {
            if (baseObject.IsSignalConnected(signal, action)) {
                baseObject.Disconnect(signal, Callable.From(action));    
            }
        }
        catch (ObjectDisposedException e) {
            GD.Print("object was disposed");
            throw;
        }
    }
    
    public static void TryDisconnect<
        [MustBeVariant] T0
    >(this GodotObject baseObject, StringName signal, Action<T0> action) {
        if (action is null) return;
        
        try {
            if (baseObject.IsSignalConnected(signal, action)) {
                baseObject.Disconnect(signal, Callable.From(action));    
            }
        }
        catch (ObjectDisposedException e) {
            GD.Print("object was disposed");
            throw;
        }
    }
    
    public static void TryDisconnect<
        [MustBeVariant] T0,
        [MustBeVariant] T1,
        [MustBeVariant] T2
    >(this GodotObject baseObject, StringName signal, Action<T0, T1, T2> action) {
        if (action is null) return;
        
        try {
            if (baseObject.IsConnected(signal, Callable.From(action))) {
                baseObject.Disconnect(signal, Callable.From(action));    
            }
        }
        catch (ObjectDisposedException e) {
            GD.Print("object was disposed");
            throw;
        }
    }
}
#endif