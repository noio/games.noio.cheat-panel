using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace noio.RuntimeTools.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class RuntimeToggleAttribute : RuntimeToolAttribute
    {
        public RuntimeToggleAttribute(string preferredHotkeys = "") : base(preferredHotkeys)
        {
        }
    }
}