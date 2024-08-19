// (C)2024 @noio_games
// Thomas van den Berg

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
        #region SERIALIZED FIELDS

        [SerializeField] TMP_Text _label;

        #endregion

        string _title;
        char _hotkey;
        CheatsPanel _panel;

        #region PROPERTIES

        public CheatBinding Binding { get; private set; }
        

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
            _panel.OnCancelAction();
        }

        #endregion

        public void Initialize(CheatBinding binding, CheatsPanel panel)
        {
            Binding = binding;
            InitializeInternal();
            _panel = panel;
        }


        public void RefreshLabel()
        {
            string label;
            var hotkey = Binding.Hotkey;
            if (hotkey == default)
            {
                label = Binding.Title;
            }
            else if (Binding.Title.Contains(hotkey, StringComparison.CurrentCultureIgnoreCase))
            {
                var rx = new Regex($"({hotkey})", RegexOptions.IgnoreCase);

                // label = rx.Replace(Title, "<b><u>$1</u></b>", 1);
                label = rx.Replace(Binding.Title, "<u>$1</u>", 1);
            }
            else
            {
                // label = $"{Title} [<b><u>{hotkey}</u></b>]";
                label = $"{Binding.Title} [<u>{hotkey}</u>]";
            }

            _label.text = label;
        }

        protected virtual void InitializeInternal()
        {
        }

    }
}