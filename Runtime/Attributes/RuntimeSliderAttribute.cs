using System;

namespace noio.RuntimeTools.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class RuntimeSliderAttribute : RuntimeToolAttribute
    {
        public float Min { get; }
        public float Max { get; }

        public RuntimeSliderAttribute(float min, float max, string preferredHotkeys = "") : base(preferredHotkeys)
        {
            Min = min;
            Max = max;
        }
    }
}