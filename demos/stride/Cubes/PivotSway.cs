using System;
using Stride.Core.Mathematics;
using Stride.Engine;

namespace Cubes
{
    public class PivotSway : SyncScript
    {
        public float tweens = 7;
        public float pauses = 1;
        
        private Quaternion _rotationStart;
        private Quaternion _rotationGoal;

        private readonly Random _random = new Random();
        private float _time;

        
        public override void Start()
        {
            _rotationStart = Entity.Transform.Rotation;
            _time = -pauses;
        }
        
        public override void Update()
        {
            if (_time <= -pauses)
            {
                _time = tweens;

                _rotationStart = Entity.Transform.Rotation;
                _rotationGoal = Quaternion.RotationYawPitchRoll(_random.NextSingle() * MathF.Tau, _random.NextSingle() * MathF.Tau, _random.NextSingle() * MathF.Tau);
            }
            else
            {
                _time -= (float)Game.UpdateTime.Elapsed.TotalSeconds;
            }

            var t = MathUtil.SmootherStep(Math.Clamp(1.0f - _time / tweens, 0, 1));

            Entity.Transform.Rotation = Quaternion.Slerp(_rotationStart, _rotationGoal, t);
        }
    }
}
