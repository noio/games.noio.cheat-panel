// (C)2024 @noio_games
// Thomas van den Berg

using System;
using JetBrains.Annotations;

namespace noio.CheatPanel.Attributes
{
/// <summary>
///     Create a cheat binding for this Method, Property, or if this is a Method of type
///     IEnumerable CheatBinding, will then add all Cheats from the returned list.
///     Hotkeys are handled as follows: all characters are tried until a free key is found,
///     starting with the <see cref="PreferredHotkeys" />, then the <see cref="Label" />,
///     then all other available keys on the keyboard.
/// </summary>
[MeansImplicitUse]
public class CheatAttribute : Attribute
{
    #region SERIALIZED FIELDS

    /// <summary>
    ///     If the member is an IEnumerable of CheatBinding, set this to true
    ///     to add those cheats to a new page instead of on the current page.
    ///     The Title/Category/Hotkeys will be used for the button that opens that page.
    /// </summary>
    /// <returns></returns>
    public bool NewPage;

    #endregion

    public CheatAttribute(
        string preferredHotkeys = "",
        string label = "",
        string category = "",
        float min = 0,
        float max = 10,
        bool newPage = false
    )
    {
        Label = label;
        PreferredHotkeys = preferredHotkeys;
        Category = category;
        Min = min;
        Max = max;
        NewPage = newPage;
    }

    #region PROPERTIES

    /// <summary>
    ///     Preferred Hotkeys for this cheat, in order
    /// </summary>
    public string PreferredHotkeys { get; }

    /// <summary>
    ///     Display title of the cheat on the panel. Will use member name if empty
    /// </summary>
    public string Label { get; }

    /// <summary>
    ///     Min value for float/int slider cheats
    /// </summary>
    public float Min { get; }

    /// <summary>
    ///     Max value for float/int slider cheats
    /// </summary>
    public float Max { get; }

    /// <summary>
    ///     Under which category should the button display
    /// </summary>
    public string Category { get; }


    #endregion
}
}