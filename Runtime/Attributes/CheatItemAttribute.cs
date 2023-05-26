using System;

namespace noio.CheatPanel.Attributes
{
    public abstract class CheatItemAttribute : Attribute
    {
        public CheatItemData Data { get; private set; }

        public CheatItemAttribute(string preferredHotkeys = "", string title = "")
        {
            Data = new CheatItemData()
            {
                PreferredHotkeys = preferredHotkeys,
                Title = title
            };
        }
    }
}