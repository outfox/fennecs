// Tsubasa.cs (type declarations at bottom of file)

using System.Threading;
using fennecs;

// 🏟️ Practice day! Let's play some soccer.
var soccerField = new World();

var goldenGoal = false;

// 🥉 Meet the players
foreach (var name in (string[]) ["Kojiro", "Genzo", "Taro", "Hikaru", "Jun", "Shingo", "Ryo", "Takeshi", "Masao", "Kazuo"])
{
    soccerField.Spawn()
        .Add<Player>()
        .Add<Name>(name)
        .Add<Talent>(false)
        .Add<Position>(RandomRadius(25));
}

// 🏅 One day, a new player joined after watching from the sidelines.
soccerField.Spawn()
    .Add<Player>()
    .Add<Name>("Tsubasa")
    .Add<Talent>(true)
    .Add<Position>(new Vector2(0, 200));

// 🏐 Strangely, Mila's team was missing their volleyball...
var ball = soccerField.Spawn().Add<Ball>().Add<Position>(new Vector2(0, 0));

var players = soccerField.Query<Name, Position, Talent>().Has<Player>().Build();

// ⚽ Game on! This is our Game Loop. 
while (!goldenGoal)
{
    Console.Clear();
    
    // Make everyone run after the ball!
    players.For((ref Name playerName, ref Position playerPosition, ref Talent playerTalent) =>
    {
        ref var ballPosition = ref ball.Ref<Position>();

        var direction = ballPosition.value - playerPosition.value;
        // 🥅 If the ball is too far enough, run towards it!
        if (direction.LengthSquared() > 1f) 
        {
            Console.WriteLine($"{playerName} runs towards the ball! {playerPosition} -> {playerPosition + direction * 0.5f}");
            playerPosition += direction * (0.2f + Random.Shared.NextSingle() * 0.5f);
            return;
        }

        // 🎯 YES! the ball is close enough, kick it!
        Console.WriteLine($">>> {playerName} kicks the ball!");
        
        // 🎲 With those kids, the ball goes all over the place!
        ballPosition += RandomRadius(15) + Vector2.Normalize(RandomRadius(1)) * 10f;

        // ⁉️ Was it a good kick?
        if (!playerTalent) return;

        // 🌟 Tsubasa's golden goal! (Could've been ANY talented player's goal, really)
        Console.WriteLine($">>> {playerName} scores!!!".ToUpper());
        goldenGoal = true;
    });
    
    Thread.Sleep(250);
}

// 🚿 Hit the Showers, boys! You've earned it.
return;


// 🧮 Math Helpers
Vector2 RandomRadius(float radius) => new(Random.Shared.NextSingle() * radius, Random.Shared.NextSingle() * radius);


// 🏃 A "tag" (zero-size type) identifying an Entity as a Player
struct Player;


// 🏐 Strangely, Mila's team was missing their volleyball...
struct Ball;


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
    public static implicit operator Vector2(Position position) => position.value;
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