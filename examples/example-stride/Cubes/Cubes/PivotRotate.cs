using Stride.Core.Mathematics;
using Stride.Engine;

namespace Cubes
{
    public class PivotRotate : SyncScript
    {
        public float speed = 0.2f;
        public override void Update()
        {
            var dt = (float) Game.UpdateTime.Elapsed.TotalSeconds * speed;
            Entity.Transform.Rotation *= Quaternion.RotationYawPitchRoll(4 * dt, 2 * dt, 3 * dt);
        }
    }
}