namespace fennecs.tests.Query;

public static class QueryEnumeration
{
    public static class QueryTests
    {
        private static World SetupWorld(out List<Entity> intEntities, out List<Entity> notIntEntities, out List<Entity> floatEntities, out List<Entity> bothEntities, out List<Entity> anyEntities)
        {
            intEntities = [];
            notIntEntities = [];
            floatEntities = [];
            bothEntities = [];
            anyEntities = [];

            var world = new World();

            for (var i = 0; i < 234; i++)
            {
                var intEntity = world.Spawn().Add<int>();
                intEntities.Add(intEntity);

                var floatEntity = world.Spawn().Add<float>();
                notIntEntities.Add(floatEntity);
                floatEntities.Add(floatEntity);

                var bothEntity = world.Spawn().Add<int>().Add<float>();
                intEntities.Add(bothEntity);
                floatEntities.Add(bothEntity);
                bothEntities.Add(bothEntity);

                anyEntities.Add(intEntity);
                anyEntities.Add(floatEntity);
                anyEntities.Add(bothEntity);
            }

            return world;
        }


        [Fact]
        public static void Changing_Tables_Interrupts()
        {
            using var world = new World();

            world.Spawn().Add(1);
            world.Spawn().Add(2);
            world.Spawn().Add(3);

            var query = world.Query().Has<int>().Compile();

            Assert.Throws<InvalidOperationException>(() =>
            {
                foreach (var _ in query) world.Spawn().Add(4);
            });

            Assert.Throws<InvalidOperationException>(() =>
            {
                foreach (var entity in query) entity.Add("structural change");
            });
        }


        [Fact]
        public static void Changing_Irrelevant_Tables_does_not_Interrupt()
        {
            using var world = new World();

            var random = new Random(9001);

            world.Spawn().Add(1);
            world.Spawn().Add(2);
            world.Spawn().Add(3);

            var query = world.Query().Has<int>().Compile();

            foreach (var _ in query) world.Spawn().Add(random.NextSingle());
            foreach (var _ in query) world.DespawnAllWith<float>();
        }


        [Fact]
        public static void Can_Enumerate_IntEntities()
        {
            using var world = SetupWorld(out var intEntities, out _, out _, out _, out _);

            var intQuery = world.Query().Has<int>().Compile();
            var intArray = intQuery.ToArray();
            Array.Sort(intArray);
            intEntities.Sort();
            Assert.Equal(intEntities, intArray);
        }


        [Fact]
        public static void Can_Enumerate_FloatEntities()
        {
            using var world = SetupWorld(out _, out _, out var floatEntities, out _, out _);

            var floatQuery = world.Query().Has<float>().Compile();
            var floatArray = floatQuery.ToArray();
            Array.Sort(floatArray);
            floatEntities.Sort();
            Assert.Equal(floatEntities, floatArray);
        }


        [Fact]
        public static void Can_Enumerate_BothEntities()
        {
            using var world = SetupWorld(out _, out _, out _, out var bothEntities, out _);

            var bothQuery = world.Query().Has<int>().Has<float>().Compile();
            var bothArray = bothQuery.ToArray();
            Array.Sort(bothArray);
            bothEntities.Sort();
            Assert.Equal(bothEntities, bothArray);
        }


        [Fact]
        public static void Can_Enumerate_AnyEntities()
        {
            using var world = SetupWorld(out _, out _, out _, out _, out var anyEntities);

            var anyQuery = world.Query().Any<int>().Any<float>().Compile();
            var anyArray = anyQuery.ToArray();
            Array.Sort(anyArray);
            anyEntities.Sort();
            Assert.Equal(anyEntities, anyArray);
        }


        [Fact]
        public static void Can_Enumerate_NotIntEntities()
        {
            using var world = SetupWorld(out _, out var notIntEntities, out _, out _, out _);

            var notIntQuery = world.Query().Not<int>().Compile();
            var notIntArray = notIntQuery.ToArray();
            Array.Sort(notIntArray);
            notIntEntities.Sort();
            Assert.Equal(notIntEntities, notIntArray);
        }
    }
}