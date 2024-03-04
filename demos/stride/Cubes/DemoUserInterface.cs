using Stride.Core;
using Stride.Input;
using Stride.Engine;
using Stride.Engine.Events;
using Stride.UI;
using Stride.UI.Controls;

namespace Cubes
{
    /// <summary>
    /// Script which dispatches an event when a key is pressed or at regular time intervals.
    /// </summary>
    public class DemoUserInterface : SyncScript
    {
        public override void Start()
        {
            var root = Entity.Get<UIComponent>().Page.RootElement;
            var simulatedSlider = root.FindVisualChildOfType<Slider>("SimulatedSlider");
            var renderedSlider = root.FindVisualChildOfType<Slider>("RenderedSlider");

            renderedSlider.Value = 1;
            //renderedSlider.ValueChanged
        }


        public override void Update()
        {
        }
    }
}
