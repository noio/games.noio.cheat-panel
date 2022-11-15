using System;

namespace noio.CheatPanel.Attributes
{
    public abstract class CheatItemAttribute : Attribute
    {
        public string PreferredHotkeys { get; }
        public string Title { get; }

        public CheatItemAttribute(string preferredHotkeys = "", string title = "")
        {
            PreferredHotkeys = preferredHotkeys;
            Title = title;
        }
    }
}

