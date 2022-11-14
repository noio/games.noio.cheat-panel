using System;

namespace noio.RuntimeTools.Attributes
{
    public abstract class RuntimeToolAttribute : Attribute
    {
        public string PreferredHotkeys { get; }

        public RuntimeToolAttribute(string preferredHotkeys = "")
        {
            PreferredHotkeys = preferredHotkeys;
        }
    }
}

