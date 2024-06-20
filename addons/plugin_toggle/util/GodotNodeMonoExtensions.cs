#if TOOLS
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace plugin_toggle.util;

public static class GodotNodeMonoExtensions {
    public static List<T> GetNodes<[MustBeVariant]  T>(this Node root) {
        var list = new List<T>();
        var children = root.GetChildren();
        
        foreach (var child in children) {
            if (child is T found) {
                list.Add(found);
            }
        }

        return list;
    }

    //todo create one without garbage
    public static T GetFirstNode<T>(this Node root) {
        return root.GetNodes<T>().FirstOrDefault();
    }
    
    public static List<T> GetAllNodesRecursive<[MustBeVariant] T>(this Node node) where T : Node {
        var list = new List<T>();
        if (node != null)
            node.GetAllNodesRecursive(ref list);
        
        return list;
    }

    private static void GetAllNodesRecursive<[MustBeVariant] T>(this Node node, ref List<T> found) {
        var children = node.GetChildren();

        foreach (var child in children) {
            if (child is T foundChild) {
                found.Add(foundChild);
            }

            child.GetAllNodesRecursive(ref found);
        }
    }
    
    public static T GetFirstParentRecursive<T>(this Node node) {
        var parent = node.GetParent();

        if (parent is T found) {
            return found;
        }
        
        if (parent is not null) {
            return parent.GetFirstParentRecursive<T>();
        }

        return default(T);
    }
    
    public static bool TryGetFirstParentRecursive<T>(this Node node, out T result) {
        result = node.GetFirstParentRecursive<T>();
        return result is not null;
    }
    
    public static void RemoveAndQueueFreeChildren(this Node root, bool includeInternal = true) {
        foreach (var child in root.GetChildren(includeInternal)) {
            if (child is null) continue;
            root.RemoveChild(child);
            child.QueueFree();
        }
    }

    public static void RemoveAndQueueFree<T>(this List<T> nodes) where T : Node {
        foreach (var child in nodes) {
            if (child is null) continue;
            child.GetParent()?.RemoveChild(child);
            child.QueueFree();
        }
    }
}
#endif