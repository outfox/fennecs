#if LIBGODOT_ENABLED
using System;
using Godot.NativeInterop;

namespace GodotPlugins.Game
{
    /// <summary>
    /// Extends the source-generated <c>GodotPlugins.Game.Main</c> (a partial
    /// class) to expose a pointer to its private plugins-initializer method.
    /// Self-contained on purpose: it works with the stock Godot.NET.Sdk
    /// source generators, no patched SDK required.
    /// </summary>
    internal static partial class Main
    {
        internal static unsafe IntPtr TwoDogGetInitializePointer() =>
            (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, IntPtr, int, godot_bool>)&InitializeFromGameProject;
    }
}

/// <summary>
/// Web (browser-wasm) bootstrap for 2dog hosts. Exposes the game assembly's
/// plugins-initializer function pointer (internal to this assembly) so the
/// host can hand it to <c>twodog.Engine.RegisterWebPluginsInitializer()</c>.
/// Scripts are looked up in the assembly that contains the generated
/// initializer, which is why this lives in the Godot project rather than the
/// host.
/// </summary>
public static class TwoDogWebBoot
{
    public static IntPtr PluginsInitializer() => GodotPlugins.Game.Main.TwoDogGetInitializePointer();
}
#endif
