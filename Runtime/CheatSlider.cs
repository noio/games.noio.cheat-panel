using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace noio.CheatPanel
{
    internal class CheatSlider : CheatItem
    {
        #region PUBLIC AND SERIALIZED FIELDS

        [SerializeField] Slider _slider;
        [SerializeField] TMP_Text _valueLabel;

        #endregion

        MemberInfo _memberInfo;
        PropertyInfo _property;
        Object _targetObject;

        #region PROPERTIES

        float PropertyValue
        {
            get => (float)_property.GetValue(_targetObject);
            set
            {
                _property.SetValue(_targetObject, value);
                _valueLabel.SetText("{0:0.00}", value);
            }
        }

        #endregion

        public void Init(Object targetObject, PropertyInfo property, float min, float max)
        {
            _targetObject = targetObject;
            _property = property;
            /*
             * Store default value because sometimes
             * property is already changed
             * by setting min/max
             */
            var defaultPropertyValue = PropertyValue;
            
            _slider.minValue = min;
            _slider.maxValue = max;
            _slider.SetValueWithoutNotify(defaultPropertyValue);

            _slider.onValueChanged.RemoveAllListeners();
            _slider.onValueChanged.AddListener(v => PropertyValue = v);

            _valueLabel.SetText("{0:0.00}", defaultPropertyValue);
        }

        protected override void Execute()
        {
            _slider.normalizedValue += 0.1f;
        }

        protected override void ExecuteAlt()
        {
            _slider.normalizedValue -= 0.1f;
        }
    }
}