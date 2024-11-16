// Tsubasa.cs (type declarations at bottom of file)

using fennecs;
using Name = string;
// ReSharper disable StringLiteralTypo

if (!Console.IsOutputRedirected) Console.Clear();
Console.WriteLine("School's out!");

// Meet the Team!
Name[] names =
[
    "Kojiro", "Genzo", "Taro", "Hikaru", "Jun",
    "Shingo", "Ryo", "Takeshi", "Masao", "Kazuo",
];

var world = new World();
var random = new Random(10);

foreach (var name in names)
    world.Spawn()
        .Add<Player>()
        .Add(name)
        .Add<Talent>(component: false)
        .Add<Position>(RandomRadius(radius: 25));

// Meet our Star!
world.Spawn()
    .Add<Player>()
    .Add("Tsubasa")
    .Add<Talent>(component: true)
    .Add<Position>(new Vector2(x: 0, y: 50));


var ball = world.Spawn()
    .Add<Ball>()
    .Add<Position>(new Vector2(x: 0, y: 0));

var team = world
    .Query<Name, Position, Talent>()
    .Has<Player>()
    .Stream();


//  Game on! This is our "Game" Loop.
var kicked = false;
var goldenGoal = false;
do
{
    if (kicked)
    {
        Thread.Sleep(millisecondsTimeout: 400);
        kicked = false;
    }

    Thread.Sleep(millisecondsTimeout: 200);
    if (!Console.IsOutputRedirected) Console.Clear();

    //  Update each player on the field.
    team.For((
            playerName,
            playerPosition,
            playerTalent
        )
        =>
    {
        ref var ballPosition = ref ball.Ref<Position>();

        var direction = ballPosition.Value - playerPosition.read;
        if (direction.LengthSquared() > 1f)
        {
            var dash = direction * (random.NextSingle() * .9f + 0.1f);
            playerPosition.write += dash;
            //playerPosition._val += dash;
            
            Console.WriteLine($"{playerName.read,15} runs towards the ball!" +
                              $" ... d = {direction.Length():f2}m");
            return;
        }

        ballPosition += RandomRadius(radius: 5, onCircle: true);
        kicked = true;
        Console.WriteLine($">>>>> {playerName.read} kicks the ball!");

        if (!playerTalent.read) return;

        Console.WriteLine($"***** {playerName.read} scores!!!".ToUpper());
        goldenGoal = true;
    });
} while (!goldenGoal);

Console.WriteLine("..... Hit the Showers, boys! You've earned it.");
return;


#region Components and Maths
// Math Helpers
Vector2 RandomRadius(float radius, bool onCircle = false)
{
    var result = new Vector2(
        random.NextSingle() * radius,
        random.NextSingle() * radius);

    if (onCircle) return Vector2.Normalize(result) * radius;

    return result;
}


// "tag" (zero-size type) identifying an Entity as a Player
internal struct Player;


// "tag" (zero-size type) identifying an Entity as a Ball
internal struct Ball;


// Component that represents a truthy value for a player's talent.
internal readonly record struct Talent(bool Value)
{
    public static implicit operator Talent(bool value) => new(value);
    public static implicit operator bool(Talent talent) => talent.Value;
}


// Position component wrapping a Vector2.
internal readonly record struct Position(Vector2 Value)
{
    public static implicit operator Vector2(Position self) => self.Value;
    public static implicit operator Position(Vector2 value) => new(value);
    public override string ToString() => Value.ToString();
}
#endregion