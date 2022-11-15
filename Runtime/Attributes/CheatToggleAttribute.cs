using System;

namespace noio.CheatPanel.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class CheatToggleAttribute : CheatItemAttribute
    {
        public CheatToggleAttribute(string preferredHotkeys = "", string title = "") : base(preferredHotkeys,
            title)
        {
        }
    }
}