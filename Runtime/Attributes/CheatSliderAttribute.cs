using System;

namespace noio.CheatPanel.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class CheatSliderAttribute : CheatItemAttribute
    {
        public float Min { get; }
        public float Max { get; }

        public CheatSliderAttribute(float min, float max, string preferredHotkeys = "", string title = "") :
            base(preferredHotkeys, title)
        {
            Min = min;
            Max = max;
        }
    }
}