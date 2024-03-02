// Tsubasa.cs (type declarations at bottom of file)

using fennecs;

// Practice day!
var world = new World();

// Meet the Team!
string[] names =
[
    "Kojiro", "Genzo", "Taro", "Hikaru", "Jun",
    "Shingo", "Ryo", "Takeshi", "Masao", "Kazuo",
];

foreach (var name in names)
    world.Spawn()
        .Add<Player>()
        .Add<Name>(name)
        .Add<Talent>(false)
        .Add<Position>(RandomRadius(25));

// Meet our Star!
world.Spawn()
    .Add<Player>()
    .Add<Name>("Tsubasa")
    .Add<Talent>(true)
    .Add<Position>(new Vector2(0, 200));


var ball = world.Spawn()
    .Add<Ball>()
    .Add<Position>(new Vector2(0, 0));

var team = world
    .Query<Name, Position, Talent>()
    .Has<Player>()
    .Build();


//  Game on! This is our "Game" Loop.
var kicked = false;
var goldenGoal = false;
do
{
    if (kicked)
    {
        Thread.Sleep(500);
        kicked = false;
    }

    Thread.Sleep(100);
    Console.Clear();

    //  Update each player on the field.
    team.For((
            ref Name playerName,
            ref Position playerPosition,
            ref Talent playerTalent
        )
        =>
    {
        ref var ballPosition = ref ball.Ref<Position>();

        var direction = ballPosition.value - playerPosition.value;
        if (direction.LengthSquared() > 1f)
        {
            var dash = direction * (Random.Shared.NextSingle() * .7f + 0.1f);
            playerPosition += dash;
            Console.WriteLine($"{playerName,15} runs towards the ball!" +
                              $" ... d = {direction.Length():f2}m");
            return;
        }

        ballPosition += RandomRadius(10, true);
        kicked = true;
        Console.WriteLine($">>>>> {playerName} kicks the ball!");

        if (!playerTalent) return;

        Console.WriteLine($"***** {playerName} scores!!!".ToUpper());
        goldenGoal = true;
    });
} while (!goldenGoal);

//  Hit the Showers, boys! You've earned it.
return;


// Math Helpers
Vector2 RandomRadius(float radius, bool onCircle = false)
{
    var result = new Vector2(
        Random.Shared.NextSingle() * radius,
        Random.Shared.NextSingle() * radius);

    if (onCircle) return Vector2.Normalize(result) * radius;

    return result;
}


// "tag" (zero-size type) identifying an Entity as a Player
internal struct Player;


// "tag" (zero-size type) identifying an Entity as a Ball
internal struct Ball;


// Component that represents a truthy value for a player's talent.
internal readonly struct Talent(bool value)
{
    private bool value { get; } = value;


    public static implicit operator Talent(bool value)
    {
        return new Talent(value);
    }


    public static implicit operator bool(Talent talent)
    {
        return talent.value;
    }
}


// Position component wrapping a Vector2.
internal readonly struct Position(Vector2 value)
{
    public Vector2 value { get; } = value;


    public static implicit operator Vector2(Position other)
    {
        return other.value;
    }


    public static implicit operator Position(Vector2 value)
    {
        return new Position(value);
    }


    public override string ToString()
    {
        return value.ToString();
    }
}


// Name Component wrapping a string.
internal readonly struct Name(string who)
{
    public static implicit operator Name(string who)
    {
        return new Name(who);
    }


    public override string ToString()
    {
        return who;
    }
}