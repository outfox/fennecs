namespace fennecs.tests
{
    internal struct Weight(int kilograms) : Fox<int>
    {
        public int Value
        {
            get => kilograms;
            set => kilograms = value;
        }
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
            
            var query = world.Query<Weight, Size>().Compile();
            
            query.For(static (ref Weight weight, ref Size size) =>
            {
                Assert.Equal(50, weight.Value);
                Assert.Equal(150, size.Value);

                weight.Value = 61;
                size.Value = 155;
            });

            query.For(static (ref Weight weight, ref Size size) =>
            {
                Assert.Equal(61, weight.Value);
                Assert.Equal(155, size.Value);
            });
        }
    }
}
