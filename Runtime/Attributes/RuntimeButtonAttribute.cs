using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace noio.RuntimeTools.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RuntimeButtonAttribute : RuntimeToolAttribute
    {
        public RuntimeButtonAttribute(string preferredHotkeys = "") : base(preferredHotkeys)
        {
        }
    }
}