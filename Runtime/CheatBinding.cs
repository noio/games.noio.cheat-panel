// (C)2024 @noio_games
// Thomas van den Berg

using System;
using UnityEngine;

namespace noio.CheatPanel
{
    public abstract class CheatBinding
    {
        public CheatBinding(
            string title,
            string preferredHotkeys = "",
            int hotkeyPriority = 0,
            string category = "")
        {
            Title = title;
            PreferredHotkeys = preferredHotkeys;
            HotkeyPriority = hotkeyPriority;
            Category = category;
        }

        #region PROPERTIES

        public string Title { get; set; }
        public string PreferredHotkeys { get; set; }
        public int HotkeyPriority { get; set; }
        public string Category { get; }
        public char Hotkey { get; private set; }

        /// <summary>
        ///     Returns a priority sorting key for this binding. Bindings with preferred hotkeys
        ///     set are always sorted to a higher priority. Otherwise just returns the
        ///     manually set HotkeyPriority
        /// </summary>
        public int HotkeyPrioritySortingKey =>
            HotkeyPriority + (string.IsNullOrEmpty(PreferredHotkeys) ? 0 : 1000);

        #endregion

        public void SetHotkey(char c)
        {
            Hotkey = c;
        }

        public abstract void Execute(bool shift = false);
    }

    public abstract class CheatBinding<T> : CheatBinding where T : struct
    {
        public event Action ValueChanged;

        public CheatBinding(string title,
            Func<T> getValue,
            Action<T> setValue,
            float min = 0,
            float max = 10,
            string preferredHotkeys = "",
            int hotkeyPriority = 0,
            string category = "")
            : base(title, preferredHotkeys, hotkeyPriority, category)
        {
            GetValue = getValue;
            SetValue = setValue;
            Min = min;
            Max = max;
        }

        #region PROPERTIES

        Func<T> GetValue { get; }
        Action<T> SetValue { get; }
        public float Min { get; set; }
        public float Max { get; set; }

        public T Value
        {
            get => GetValue();
            set
            {
                SetValue(value);
                ValueChanged?.Invoke();
            }
        }

        #endregion
    }

    public class CheatFloatBinding : CheatBinding<float>
    {
        public CheatFloatBinding(string title,
            Func<float> getValue,
            Action<float> setValue,
            float min,
            float max,
            string preferredHotkeys = "",
            string category = "",
            int hotkeyPriority = 0) :
            base(title, getValue, setValue,
                min, max, preferredHotkeys, hotkeyPriority, category)
        {
        }

        public override void Execute(bool shift)
        {
            var value = Value;
            var incr = (Max - Min) * .1f;
            if (shift)
            {
                incr = -incr;
            }

            Value = Mathf.Clamp(value + incr, Min, Max);
        }
    }

    public class CheatBoolBinding : CheatBinding<bool>
    {
        public CheatBoolBinding(
            string title,
            Func<bool> getValue,
            Action<bool> setValue,
            string preferredHotkeys = "",
            int hotkeyPriority = 0,
            string category = "")
            : base(title, getValue, setValue, 0, 1, preferredHotkeys, hotkeyPriority, category)
        {
        }

        public override void Execute(bool shift)
        {
            if (shift)
            {
                Value = false;
            }
            else
            {
                Value = !Value;
            }
        }
    }

    public class CheatActionBinding : CheatBinding
    {
        public CheatActionBinding(
            string title,
            Action action,
            string preferredHotkeys = "",
            int hotkeyPriority = 0,
            string category = "")
            : base(title, preferredHotkeys, hotkeyPriority, category)
        {
            Action = action;
        }

        #region PROPERTIES

        public Action Action { get; }

        #endregion

        public override void Execute(bool shift)
        {
            Action?.Invoke();
        }
    }

    public class CheatOpenPageBinding : CheatActionBinding
    {
        public CheatOpenPageBinding(
            string title,
            Action action,
            string preferredHotkeys = "",
            int hotkeyPriority = 0,
            string category = ""
        ) : base(title, action, preferredHotkeys, hotkeyPriority, category)
        {
        }
    }
}