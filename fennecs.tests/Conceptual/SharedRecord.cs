namespace fennecs.tests.Conceptual;

public class SharedRecord(ITestOutputHelper output)
{
    record SharedData(int Value)
    {
        public int Value { get; set; } = Value;
    }

    [Fact]
    public void SharedRecord_works()
    {
        using var world = new World();
        var stream = world.Query<SharedData>().Stream();

        var sharedData = new SharedData(42);     // shared instance
        world.Entity().Add(sharedData).Spawn(5); // add it to 5 fresh Entities

        stream.For((ref data) =>
        {
            data.Value++; // increments value once for each entity in query!
            output.WriteLine(data.ToString());
        });

        sharedData.Value++; // increment outside of runner
        output.WriteLine("");

        stream.For((ref data) =>
        {
            output.WriteLine(data.ToString());
        });
    }

}
