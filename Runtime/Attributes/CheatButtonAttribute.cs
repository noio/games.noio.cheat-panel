using System;

namespace noio.CheatPanel.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CheatButtonAttribute : CheatItemAttribute
    {
        public CheatButtonAttribute(string preferredHotkeys = "", string title = "") :
            base(preferredHotkeys, title)
        {
        }
    }
}