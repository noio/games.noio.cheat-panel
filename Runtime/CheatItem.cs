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
        bool _didSetLabelOnce;

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
            /*
             * If this binding doesn't have a dynamic label, we only need to set once:
             */
            if (_didSetLabelOnce && Binding.LabelGetter == null)
            {
                return;
            }
            
            string label = Binding.LabelGetter != null ? Binding.LabelGetter() : Binding.Label;
            
            var hotkey = Binding.Hotkey;
            
            if (hotkey != default)
            {
                if (label.Contains(hotkey, StringComparison.CurrentCultureIgnoreCase))
                {
                    var rx = new Regex($"({hotkey})", RegexOptions.IgnoreCase);

                    // label = rx.Replace(Title, "<b><u>$1</u></b>", 1);
                    label = rx.Replace(label, "<u>$1</u>", 1);
                }
                else
                {
                    // label = $"{Title} [<b><u>{hotkey}</u></b>]";
                    label = $"{label} [<u>{hotkey}</u>]";
                }
            }

            // label = Binding.Title;
            _label.text = label;
            _didSetLabelOnce = true;
        }

        protected virtual void InitializeInternal()
        {
        }

    }
}