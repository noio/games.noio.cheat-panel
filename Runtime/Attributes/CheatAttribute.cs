using System;

namespace noio.CheatPanel.Attributes
{
    public class CheatAttribute : Attribute
    {
        public string PreferredHotkeys { get; }
        public string Title { get; }
        public float Min { get; }
        public float Max { get; }
        public string Category { get; }

        public CheatAttribute(
            string preferredHotkeys = "",
            string title            = "",
            string category         = "",
            float  min              = 0,
            float  max              = 10
        )
        {
            Title = title;
            PreferredHotkeys = preferredHotkeys;
            Category = category;
            Min = min;
            Max = max;
        }
    }
}