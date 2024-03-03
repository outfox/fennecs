using System;
using Stride.Core.Mathematics;
using Stride.Engine;

namespace Cubes
{
    public class PivotSway : SyncScript
    {
        public float interval = 7;
        public float minRange = 50;
        public float maxRange = 300;

        public float origin;
        public float destination;

        public Quaternion rotationStart;
        public Quaternion rotationGoal;

        public override void Start()
        {
            origin = Entity.Transform.Position.Z;
            destination = origin;
            rotationStart = Entity.Transform.Rotation;
        }


        private float _time;
        public override void Update()
        {
            if (_time == 0)
            {
                var random = new Random();
                _time = interval;

                destination = random.Next((int) minRange, (int) maxRange);
                rotationStart = Entity.Transform.Rotation;
                rotationGoal = Quaternion.RotationYawPitchRoll(random.NextSingle() * MathF.Tau, random.NextSingle() * MathF.Tau, random.NextSingle() * MathF.Tau);
            }
            else
            {
                _time -= (float)Game.UpdateTime.Elapsed.TotalSeconds;
            }

            var t = MathUtil.SmootherStep(_time / interval);

            Entity.Transform.Rotation = Quaternion.Slerp(rotationStart, rotationGoal, t);
        }
    }
}
