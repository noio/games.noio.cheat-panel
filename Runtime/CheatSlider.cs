// (C)2024 @noio_games
// Thomas van den Berg

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace noio.Cheats
{
    internal class CheatSlider : CheatUIElementBase
    {
        #region SERIALIZED FIELDS

        [SerializeField] Slider _slider;
        [SerializeField] TMP_Text _valueLabel;

        #endregion

        #region PROPERTIES

        float Value
        {
            get => (Binding as CheatFloatBinding).Value;
            set => (Binding as CheatFloatBinding).Value = value;
        }

        #endregion

        public override void RefreshValue()
        {
            RefreshValueLabel();
        }

        protected override void InitializeInternal()
        {
            var binding = Binding as CheatFloatBinding;
            var defaultPropertyValue = binding.Value;

            binding.ValueChanged += HandleBindingValueChanged;

            _slider.minValue = binding.Min;
            _slider.maxValue = binding.Max;
            _slider.SetValueWithoutNotify(defaultPropertyValue);

            _slider.onValueChanged.RemoveAllListeners();
            _slider.onValueChanged.AddListener(HandleSliderValueChanged);

            RefreshValueLabel();
        }

        void HandleBindingValueChanged()
        {
            _slider.SetValueWithoutNotify(Value);
            RefreshValueLabel();
        }

        void HandleSliderValueChanged(float v)
        {
            Value = v;
            RefreshValueLabel();
        }

        void RefreshValueLabel()
        {
            _valueLabel.SetText("{0:0.00}", Value);
        }
    }
}