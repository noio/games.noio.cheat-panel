// (C)2024 @noio_games
// Thomas van den Berg

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace noio.CheatPanel
{
    internal class CheatToggle : CheatItem
    {
        #region SERIALIZED FIELDS

        [SerializeField] TMP_Text _valueLabel;

        #endregion

        Button _button;

        #region PROPERTIES

        bool PropertyValue
        {
            get => (Binding as CheatBinding<bool>)?.GetValue() ?? false;
            set
            {
                (Binding as CheatBinding<bool>)?.SetValue(value);
                SetValueLabel();
            }
        }

        #endregion

        #region MONOBEHAVIOUR METHODS

        void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(Execute);
        }

        #endregion

        protected override void InitializeInternal()
        {
        }

        void SetValueLabel()
        {
            _valueLabel.text = PropertyValue ? "ON" : "OFF";
        }

        protected override void Execute()
        {
            PropertyValue = !PropertyValue;
        }

        protected override void ExecuteAlt()
        {
            Execute();
        }
    }
}