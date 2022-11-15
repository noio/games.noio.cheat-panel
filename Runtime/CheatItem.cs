using System;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace noio.CheatPanel
{
    public abstract class CheatItem : MonoBehaviour,
        IPointerClickHandler, ISubmitHandler, ISelectHandler, ICancelHandler
    {
        #region PUBLIC AND SERIALIZED FIELDS

        [SerializeField] TMP_Text _label;

        #endregion

        string _title;
        char _hotkey;

        #region PROPERTIES

        public string PreferredHotkeys { get; set; }

        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                SetLabel();
            }
        }

        public char Hotkey
        {
            get => _hotkey;
            set
            {
                _hotkey = char.ToUpper(value);
                SetLabel();
            }
        }

        public float HueTint
        {
            set
            {
                var selectable = GetComponent<Selectable>();
                var colorBlock = selectable.colors;
                var color = Color.HSVToRGB(value, .3f, 1);
                colorBlock.normalColor *= color;
                colorBlock.highlightedColor *= color;
                colorBlock.selectedColor *= color;
                colorBlock.disabledColor *= color;
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
        }

        public void OnCancel(BaseEventData eventData)
        {
            ExecuteAlt();
        }

        #endregion

        public void OnHotkeyUsed(bool shift)
        {
            if (shift == false)
            {
                Execute();
            }
            else
            {
                ExecuteAlt();
            }
        }

        void SetLabel()
        {
            string label;
            if (Hotkey == default)
            {
                label = Title;
            }
            else if (Title.Contains(Hotkey, StringComparison.CurrentCultureIgnoreCase))
            {
                var rx = new Regex($"({Hotkey})", RegexOptions.IgnoreCase);
                // label = rx.Replace(Title, "<b><u>$1</u></b>", 1);
                label = rx.Replace(Title, "<u>$1</u>", 1);
            }
            else
            {
                // label = $"{Title} [<b><u>{Hotkey}</u></b>]";
                label = $"{Title} [<u>{Hotkey}</u>]";
            }

            _label.text = label;
        }

        protected abstract void Execute();
        protected abstract void ExecuteAlt();
    }
}