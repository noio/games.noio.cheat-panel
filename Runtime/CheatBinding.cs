// (C)2024 @noio_games
// Thomas van den Berg

using System;

namespace noio.CheatPanel
{
    public abstract class CheatBinding
    {
        public CheatBinding(string title, string preferredHotkeys = "", string category = "")
        {
            Title = title;
            PreferredHotkeys = preferredHotkeys;
            Category = category;
        }

        #region PROPERTIES

        public string Title { get; set; }
        public string PreferredHotkeys { get; set; }
        public string Category { get; }
        public char Hotkey { get; private set; }

        #endregion

        public void SetHotkey(char c)
        {
            Hotkey = c;
        }
    }

    public class CheatBinding<T> : CheatBinding where T : struct
    {
        public CheatBinding(
            string    title,
            Func<T>   getValue,
            Action<T> setValue,
            float     min,
            float     max,
            string    preferredHotkeys = "",
            string    category         = "")
            : base(title, preferredHotkeys, category)
        {
            GetValue = getValue;
            SetValue = setValue;
            Min = min;
            Max = max;
        }

        #region PROPERTIES

        public Func<T> GetValue { get; }
        public Action<T> SetValue { get; }
        public float Min { get; set; }
        public float Max { get; set; }

        public T Value
        {
            get => GetValue();
            set => SetValue(value);
        }

        #endregion
    }

    public class CheatActionBinding : CheatBinding
    {
        public CheatActionBinding(
            string title,
            Action action,
            string preferredHotkeys = "",
            string category         = "")
            : base(title, preferredHotkeys, category)
        {
            Action = action;
        }

        #region PROPERTIES

        public Action Action { get; }

        #endregion
    }

    public class CheatOpenPageBinding : CheatActionBinding
    {
        public CheatOpenPageBinding(
            string title,
            Action action,
            string preferredHotkeys = "",
            string category         = "") : base(title, action, preferredHotkeys, category)
        {
        }
    }
}