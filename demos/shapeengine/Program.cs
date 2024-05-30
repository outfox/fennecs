using System.Drawing;
using ShapeEngine.Color;
using ShapeEngine.Core;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;

namespace fennecs.shape.demo;


public static class Program
{
    public static void Main(string[] args)
    {
        var gameSettings = new GameSettings()
        {
            DevelopmentDimensions = new Dimensions(1920, 1080),
            MultiShaderSupport = false
        };
        var game = new MyGameClass(gameSettings, WindowSettings.Default);
        game.Run();
    }
}
public class MyGameClass : Game
{
    public MyGameClass(GameSettings gameSettings, WindowSettings windowSettings) : base(gameSettings, windowSettings) { }
    protected override void DrawGame(ScreenInfo game)
    {
        game.Area.Draw(new ColorRgba(Color.DarkOliveGreen));
        game.Area.DrawLines(12f, new ColorRgba(Color.AntiqueWhite));
        game.MousePos.Draw(24f, new ColorRgba(Color.Lime), 36);
    }
}
