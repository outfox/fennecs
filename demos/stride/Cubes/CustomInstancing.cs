using Stride.Core.Mathematics;
using Stride.Engine;

namespace Cubes;

public class CustomInstancing : IInstancing
{
    public void Update()
    {
    }


    public int InstanceCount { get; }
    public BoundingBox BoundingBox { get; }
    public ModelTransformUsage ModelTransformUsage { get; }
}