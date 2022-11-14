using System;

namespace noio.RuntimeTools.Attributes
{
    public abstract class RuntimeToolAttribute : Attribute
    {
        public string PreferredHotkeys { get; }
        public string Title { get; }

        public RuntimeToolAttribute(string preferredHotkeys = "", string title = "")
        {
            PreferredHotkeys = preferredHotkeys;
            Title = title;
        }
    }
}

