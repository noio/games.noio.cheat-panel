using System;

namespace noio.RuntimeTools.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RuntimeButtonAttribute : RuntimeToolAttribute
    {
        public RuntimeButtonAttribute(string preferredHotkeys = "", string title = "") :
            base(preferredHotkeys, title)
        {
        }
    }
}