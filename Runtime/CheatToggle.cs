// (C)2024 @noio_games
// Thomas van den Berg

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace noio.Cheats
{
    internal class CheatToggle : CheatUIElementBase
    {
        #region SERIALIZED FIELDS

        [SerializeField] TMP_Text _valueLabel;

        #endregion

        Button _button;

        #region PROPERTIES

        bool Value => (Binding as CheatBoolBinding)?.Value ?? false;

        #endregion

        #region MONOBEHAVIOUR METHODS

        void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(() => Binding.Execute());
        }

        #endregion

        protected override void InitializeInternal()
        {
            (Binding as CheatBoolBinding).ValueChanged += HandleBindingValueChanged;
            SetValueLabel();
        }

        void HandleBindingValueChanged()
        {
            SetValueLabel();
        }

        void SetValueLabel()
        {
            _valueLabel.text = Value ? "YES" : "no";
        }
    }
}