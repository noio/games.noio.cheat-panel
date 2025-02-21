// (C)2025 @noio_games
// Thomas van den Berg

using System;
using UnityEngine;

namespace noio.CheatPanel
{
public abstract class CheatBinding
{
    public CheatBinding(string label)
    {
        Label = label;
    }

    #region PROPERTIES

    public string Label { get; set; }
    public string PreferredHotkeys { get; set; } = "";
    public int HotkeyPriority { get; set; } = 0;

    /// <summary>
    /// This is the page the action is on
    /// </summary>
    public string Page { get; set; } = "";

    public string Category { get; set; } = "";
    public char Hotkey { get; private set; }

    /// <summary>
    ///     Method that will be called after every interaction with the button to
    ///     update the label
    /// </summary>
    public Func<string> LabelGetter { get; set; }

    /// <summary>
    ///     Returns a priority sorting key for this binding. Bindings with preferred hotkeys
    ///     set are always sorted to a higher priority. Otherwise just returns the
    ///     manually set HotkeyPriority
    /// </summary>
    public (int, int, string, string) HotkeyPrioritySortingKey => (
        string.IsNullOrEmpty(PreferredHotkeys) ? 0 : -1000,
        -HotkeyPriority,
        Category, Label);

    public event Action NotifyLabelRefresh;

    public void RefreshLabel()
    {
        NotifyLabelRefresh?.Invoke();
    }

    #endregion

    public void SetHotkey(char c)
    {
        Hotkey = char.ToUpper(c);
    }

    public abstract void Execute(bool shift = false);
}

public abstract class CheatBinding<T> : CheatBinding where T : struct
{
    public event Action ValueChanged;

    public CheatBinding(
        string label,
        Func<T> getValue,
        Action<T> setValue
    )
        : base(label)
    {
        GetValue = getValue;
        SetValue = setValue;
    }

    #region PROPERTIES

    Func<T> GetValue { get; }
    Action<T> SetValue { get; }
    public float Min { get; set; } = 0;
    public float Max { get; set; } = 10;

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
    public CheatFloatBinding(
        string label,
        Func<float> getValue,
        Action<float> setValue
    ) :
        base(label, getValue, setValue)
    {
    }

    #region PROPERTIES

    public float Increments { get; set; } = 0;

    #endregion

    public override void Execute(bool shift)
    {
        var value = Value;
        var incr = Increments == 0 ? (Max - Min) * .1f : Increments;

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
        string label,
        Func<bool> getValue,
        Action<bool> setValue
    )
        : base(label, getValue, setValue)
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
        string label,
        Action action,
        Action altAction = null
    )
        : base(label)
    {
        Action = action;
        AltAction = altAction;
    }

    #region PROPERTIES

    public Action Action { get; }
    public Action AltAction { get; }

    #endregion

    public override void Execute(bool shift)
    {
        if (shift)
        {
            AltAction?.Invoke();
        }
        else
        {
            Action?.Invoke();
        }
    }
}

public class CheatOpenPageBinding : CheatActionBinding
{
    public CheatOpenPageBinding(
        string pageTitle,
        Action action
    ) : base(pageTitle, action)
    {
        OpenPageWithTitle = pageTitle;
    }

    #region PROPERTIES

    public string OpenPageWithTitle { get; }

    #endregion
}
}