// (C)2025 @noio_games
// Thomas van den Berg

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace noio.CheatPanel
{
internal class CheatCategory : MonoBehaviour
{
    #region SERIALIZED FIELDS

    [SerializeField] RectTransform _contentParent;
    [SerializeField] TMP_Text _label;

    #endregion

    #region PROPERTIES

    public RectTransform ContentParent => _contentParent;

    public string Title
    {
        get => _label.text;
        set => _label.text = value;
    }

    #endregion

    public void UpdateGridHeight()
    {
        var availableHeight = ((RectTransform)transform.parent).rect.height;
        availableHeight -= 20; // header;
        var rows = Mathf.RoundToInt(availableHeight / 20);
        var gridLayout = _contentParent.GetComponent<GridLayoutGroup>();
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedRowCount;
        gridLayout.constraintCount = rows;
    }
}
}