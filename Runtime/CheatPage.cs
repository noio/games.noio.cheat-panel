// (C)2025 @noio_games
// Thomas van den Berg

using System;
using System.Collections.Generic;
using UnityEngine;

namespace noio.CheatPanel
{
public class CheatPage
{
    readonly List<CheatBinding> _bindings = new();

    public CheatPage(string title)
    {
        Title = title;
    }

    #region PROPERTIES

    public string Title { get; set; }
    public IReadOnlyList<CheatBinding> Bindings => _bindings;
    public List<CheatCategory> Categories { get; } = new();
    public Func<IEnumerable<CheatBinding>> BindingsGetter { get; set; }
    public bool RefreshPageContentsOnOpen => BindingsGetter != null;

    #endregion

    public void AddStaticBinding(CheatBinding binding)
    {
        if (BindingsGetter != null)
        {
            Debug.LogError("Can't add Static Binding to a page that is set to Refresh Contents on Open");
            return;
        }

        _bindings.Add(binding);
    }

    public void RefreshBindings()
    {
        _bindings.Clear();
        _bindings.AddRange(BindingsGetter());
    }

    public void SortBindings()
    {
        _bindings.Sort((a, b) => a.HotkeyPrioritySortingKey.CompareTo(b.HotkeyPrioritySortingKey));
    }
}
}