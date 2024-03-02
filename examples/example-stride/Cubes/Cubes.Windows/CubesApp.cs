using Stride.Engine;

namespace Cubes
{
    class CubesApp
    {
        static void Main(string[] args)
        {
            using (var game = new Game())
            {
                game.Run();
            }
        }
    }
}
