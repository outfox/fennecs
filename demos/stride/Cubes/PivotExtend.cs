using System;
using Stride.Core.Mathematics;
using Stride.Engine;

namespace Cubes
{
    public class PivotExtend : SyncScript
    {
        public float tweens = 7;
        public float pauses = 1;

        public float minimumXY = -50f;
        public float maximumXY = 50f;
        public float minimumZ = 600f;
        public float maximumZ = 1200f;
        
        private readonly Random _random = new Random();
        private float _time;

        private Vector3 _extensionStart;
        private Vector3 _extensionGoal;
        
        public override void Start()
        {
            _extensionStart = Entity.Transform.Position;
            _time = -pauses;
        }


        public override void Update()
        {
            if (_time <= -pauses)
            {
                _time = tweens;

                _extensionStart = Entity.Transform.Position;
                _extensionGoal = new Vector3(_random.NextSingle() * (maximumXY - minimumXY) + minimumXY, 
                    _random.NextSingle() * (maximumXY - minimumXY) + minimumXY, 
                    _random.NextSingle() * (maximumZ - minimumZ) + minimumZ);
            }
            else
            {
                _time -= (float) Game.UpdateTime.Elapsed.TotalSeconds;
            }

            var t = MathUtil.SmootherStep(Math.Clamp(1.0f - _time / tweens, 0, 1));

            Entity.Transform.Position = Vector3.Lerp(_extensionStart, _extensionGoal, t);
        }
    }
}
