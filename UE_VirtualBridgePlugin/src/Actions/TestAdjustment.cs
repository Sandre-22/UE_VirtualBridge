namespace Loupedeck.UE_VirtualBridgePlugin
{
    using System;
    public class CounterAdjustment : PluginDynamicAdjustment
    {
        private Int32 _counter = 0;

        public CounterAdjustment()
    : base(displayName: "Counter", description: "Counts rotation ticks", groupName: "Adjustments", hasReset: true)
        {
        }

        protected override void ApplyAdjustment(String actionParameter, Int32 diff)
        {
            this._counter += 3*diff; // Increase or decrease the counter by the number of ticks.
            this.AdjustmentValueChanged(); // Notify the Plugin service that the adjustment value has changed.
        }

        protected override void RunCommand(String actionParameter)
        {
            this._counter = 0; // Reset the counter.
            this.AdjustmentValueChanged(); // Notify the Plugin service that the adjustment value has changed.
        }

        protected override String GetAdjustmentValue(String actionParameter) => this._counter.ToString();
    }
}