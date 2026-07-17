using Godot;
using Engine = twodog.Engine;

internal static class Program
{
    private static int Main(string[] args)
    {
        Console.WriteLine("fennecs.demos.godot (web) starting...");

        // The Godot project's assembly owns the source-generated plugins
        // initializer; register it before Start() (there is no
        // GodotPlugins.dll on web).
        Engine.RegisterWebPluginsInitializer(TwoDogWebBoot.PluginsInitializer());

        // args come from the page's GODOT_CONFIG.args plus the
        // '--main-pack godot.pck' the engine loader prepends.
        var engine = new Engine("fennecs.demos.godot", null, args);
        engine.Start();

        GD.Print("2dog is running in the browser!");
        GD.Print("Scene Root: ", engine.Tree.CurrentScene.Name);

        // Hands the loop to emscripten and returns immediately; the engine
        // destroys itself when Godot requests quit. Do not dispose here.
        engine.Run();

        return 0;
    }
}
