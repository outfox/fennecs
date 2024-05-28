using System;
using Godot;

namespace fennecs.demos.godot;

public interface LifeCycle
{
    event Action OnEnterTree;
    event Action OnExitTree;
    event Action OnDispose;
    /* optional... might be useful!
    event Action OnReady;
    event Action OnProcess;
    event Action OnPhysicsProcess;
    event Action OnInput;
    event Action OnUnhandledInput;
    event Action OnGuiInput;
    event Action OnVisibilityChanged;
    */
}

public class MyNode<T> where T : Node
{
    // Additional properties and methods specific to MyNode<T>
    public void MyNodeMethod()
    {
        // Implementation specific to MyNode
    }
}
