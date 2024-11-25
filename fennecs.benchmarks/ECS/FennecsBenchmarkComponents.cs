using System.Numerics;
using fennecs;

namespace fennecs_Components
{
    internal record struct Component1(int Value) : Fox<int>;
    internal record struct Component2(int Value) : Fox<int>;
    internal record struct Component3(int Value) : Fox<int>;

    internal record struct Position(Vector3 Value) : Fox<Vector3>;
    internal record struct Velocity(Vector3 Value) : Fox<Vector3>;
    internal record struct Acceleration(Vector3 Value) : Fox<Vector3>;

}
    