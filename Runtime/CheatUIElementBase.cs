// (C)2024 @noio_games
// Thomas van den Berg

using System;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace noio.Cheats
{
public abstract class CheatUIElementBase : MonoBehaviour,
    IPointerClickHandler, ISubmitHandler, ISelectHandler, IDeselectHandler, ICancelHandler
{
    #region SERIALIZED FIELDS

    [SerializeField] TMP_Text _label;
    [SerializeField] Image _outline;

    #endregion

    string _title;
    char _hotkey;
    CheatPanel _panel;
    bool _didSetLabelOnce;

    #region PROPERTIES

    public CheatBinding Binding { get; private set; }

    public float HueTint
    {
        set
        {
            var selectable = GetComponent<Selectable>();
            var colorBlock = selectable.colors;
            colorBlock.normalColor = Color.HSVToRGB(value, .4f, 1);
            colorBlock.highlightedColor = Color.HSVToRGB(value, .3f, 1);
            colorBlock.selectedColor = Color.HSVToRGB(value, .2f, 1);
            colorBlock.disabledColor = Color.HSVToRGB(value, .2f,.8f);
            selectable.colors = colorBlock;
        }
    }

    #endregion

    #region INTERFACE IMPLEMENTATIONS

    public void OnPointerClick(PointerEventData eventData)
    {
        // Execute();
    }

    public void OnSubmit(BaseEventData eventData)
    {
        // Execute();
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (_outline != null)
        {
            _outline.enabled = true;
        }
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (_outline != null)
        {
            _outline.enabled = false;
        }
    }

    public void OnCancel(BaseEventData eventData)
    {
        _panel.OnCancelAction();
    }

    #endregion

    public void Initialize(CheatBinding binding, CheatPanel panel)
    {
        name = binding.GetLabel();
        Binding = binding;
        InitializeInternal();
        _panel = panel;

        if (!string.IsNullOrEmpty(binding.Subcategory))
        {
            HueTint = GetHueFromString(binding.Subcategory);
        }

        if (_outline != null)
        {
            _outline.enabled = false;
        }

        RefreshLabel();
    }

    public abstract void RefreshValue();

    public void RefreshLabel()
    {
        /*
         * If this binding doesn't have a dynamic label, we only need to set once:
         */
        if (_didSetLabelOnce && Binding.HasDynamicLabel == false)
        {
            return;
        }

        string label = Binding.GetDynamicLabel != null ? Binding.GetDynamicLabel() : Binding.Label;
        if (string.IsNullOrEmpty(label))
        {
            label = "UNKNOWN";
        }

        var hotkey = Binding.Hotkey;

        if (hotkey != 0)
        {
            var hotkeyColor = new Color(0f, 0f, 1f); 
            string colorHex = ColorUtility.ToHtmlStringRGB(hotkeyColor);
            var startTag = $"<color=#{colorHex}><b>";
            var endTag = "</b></color>";

            var firstChar = char.ToUpper(label[0]);
            if (firstChar == hotkey)
            {
                label = $"{startTag}{hotkey}{endTag}{label[1..]}";
            }
            else
            {
                label = $"{label} [ {startTag}{hotkey}{endTag} ]";
            }

            /*
             * The code below would check to see if we can underline a letter INSIDE
             * the label (if the hotkey occurs in the label). But it's too hard to read
             */
            // if (label.Contains(hotkey, StringComparison.CurrentCultureIgnoreCase))
            // {
            //     var rx = new Regex($"({hotkey})", RegexOptions.IgnoreCase);
            //
            //     // label = rx.Replace(Title, "<b><u>$1</u></b>", 1);
            //     label = rx.Replace(label, "<u>$1</u>", 1);
            // }
            // else
            // {
            //     // label = $"{Title} [<b><u>{hotkey}</u></b>]";
            //     label = $"{label} [<u>{hotkey}</u>]";
            // }
        }

        // label = Binding.Title;
        _label.text = label;
        _didSetLabelOnce = true;
    }

    protected virtual void InitializeInternal()
    {
    }

    static float GetHueFromString(string str)
    {
        var hash = 0;
        foreach (var c in str)
        {
            hash = ((hash << 5) - hash) + c;
        }
        return Mathf.Abs(hash % 360) / 360f;
    }
}
}