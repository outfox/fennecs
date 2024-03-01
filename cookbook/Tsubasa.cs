// Tsubasa.cs (type declarations at bottom of file)

using fennecs;

// 🏟️ Practice day!
var world = new World();

// 📄 The team roster (without our Star!)
string[] names =
[
    "Kojiro", "Genzo", "Taro", "Hikaru", "Jun",
    "Shingo", "Ryo", "Takeshi", "Masao", "Kazuo",
];

// 🥉 Meet the players
foreach (var name in names)
{
    world.Spawn()
        .Add<Player>()
        .Add<Name>(name)
        .Add<Talent>(false)
        .Add<Position>(RandomRadius(25));
}

// 🏅 One day, a new player joined after watching from the sidelines.
world.Spawn()
    .Add<Player>()
    .Add<Name>("Tsubasa")
    .Add<Talent>(true) // 🤩 Our special boi!
    .Add<Position>(new Vector2(0, 200));

// 🏐 Strangely, Mila's team was missing their volleyball...
var ball = world.Spawn()
    .Add<Ball>()
    .Add<Position>(new Vector2(0, 0));

// 📋 Let's get the team ready for the game. 
var team = world
    .Query<Name, Position, Talent>()
    .Has<Player>()
    .Build(); // Ha, talk about Team...Building! 😅

// ⚽ Game on! This is our Game Loop.
var kicked = false;
var goldenGoal = false;
do
{
    // 🕐 Let them have their moment of glory.
    if (kicked)
    {
        Thread.Sleep(500);
        kicked = false;
    }

    // 🎨 "Redraw" the field
    Thread.Sleep(100);
    Console.Clear();

    // 🏃 Control each players on the field.
    team.For((
            ref Name playerName,
            ref Position playerPosition,
            ref Talent playerTalent
        )
        =>
    {
        // ⭐ We get a true ref instead of a value because we want kick it!
        ref var ballPosition = ref ball.Ref<Position>();

        // ⁉️ Where's the ball?
        var direction = ballPosition.value - playerPosition.value;

        // 🏃‍♂️ If the ball is too far, run towards it!
        if (direction.LengthSquared() > 1f)
        {
            playerPosition += direction * Random.Shared.NextSingle() * .7f;
            Console.WriteLine($"{playerName,15} runs towards the ball!" +
                              $" ... d = {direction.Length():f2}m");
            return;
        }

        // 🎯 YES! the ball is close enough, kick it!
        kicked = true;
        Console.WriteLine($">>>>> {playerName} kicks the ball!");

        // 🎲 With those kids, the ball goes all over the place!
        ballPosition += RandomRadius(10, true);

        // ⁉️ Was it a good kick?
        if (!playerTalent) return;

        // 🌟 Tsubasa's golden goal! (or ANY talented player's goal)
        Console.WriteLine($"***** {playerName} scores!!!".ToUpper());
        goldenGoal = true;
    });
} while (!goldenGoal);

// 🚿 Hit the Showers, boys! You've earned it.
return;


// 🧮 Math Helpers
Vector2 RandomRadius(float radius, bool onCircle = false)
{
    var result = new Vector2(
        Random.Shared.NextSingle() * radius,
        Random.Shared.NextSingle() * radius);

    if (onCircle)
    {
        return Vector2.Normalize(result) * radius;
    }

    return result;
}


// 🏃 "tag" (zero-size type) identifying an Entity as a Player
struct Player;


// ⚽ "tag" (zero-size type) identifying an Entity as a Ball
struct Ball;


// 📄 A component that represents a truthy value for a player's talent.
readonly struct Talent(bool value)
{
    private bool value { get; } = value;
    public static implicit operator Talent(bool value) => new(value);
    public static implicit operator bool(Talent talent) => talent.value;
};


// ↗️ A Position component wrapping a Vector2.
readonly struct Position(Vector2 value)
{
    public Vector2 value { get; } = value;
    public static implicit operator Vector2(Position other) => other.value;
    public static implicit operator Position(Vector2 value) => new(value);
    public override string ToString() => value.ToString();
}


// 🔤 A Name component wrapping a string.
readonly struct Name(string who)
{
    // 😉 So we don't always need to invoke the Constructor.
    public static implicit operator Name(string who) => new(who);


    // ✒️ To sign those Inter Milan contracts!
    public override string ToString() => who;
};