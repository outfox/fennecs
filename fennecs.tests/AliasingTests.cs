namespace fennecs.tests
{
    internal struct Weight(int kilograms) : Fox<int>
    {
        public int Value { get; set; } = kilograms;
        public static implicit operator Weight(int kilograms) => new(kilograms); 
    }


    internal struct Size(int cm) : Fox<int>
    {
        public int Value { get; set; } = cm;
        public static implicit operator Size(int centimeters) => new(centimeters); 
    }


    public class AliasingTests
    {
        [Fact]
        public void WeightIsNotSize()
        {
            var weight = new Weight(50); 
            var size = new Size(160);

            // Cannot assign
            // weight = size;
            
            // Cannot compare
            // Assert.NotEqual(weight, size);
        }

        [Fact]
        public void FoxesCanBeStreamTypes()
        {
            var world = new World();
            world.Spawn()
                .Add<Weight>(50)
                .Add<Size>(150);
            
            var query = world.Query<Weight, Size>().Stream();
            
            query.For(static (weight, size) =>
            {
                Assert.Equal(50, weight.read);
                Assert.Equal(150, size.read);

                weight.write = 61;
                size.write = 155;
            });

            query.For(static (weight, size) =>
            {
                Assert.Equal(61, weight.read);
                Assert.Equal(155, size.read);
            });
        }
    }
}
