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

    [SerializeField] RectTransform _header;
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
        var gridLayout = _contentParent.GetComponent<GridLayoutGroup>();
        availableHeight -= _header.rect.height + 2; // header;
        var cellHeight = gridLayout.cellSize.y + gridLayout.spacing.y;
        
        var rows = Mathf.RoundToInt(availableHeight / cellHeight);
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedRowCount;
        gridLayout.constraintCount = rows;
    }

    public void SetColumnWidth(float columnWidth)
    {
        var gridLayout = _contentParent.GetComponent<GridLayoutGroup>();
        gridLayout.cellSize = new Vector2(columnWidth, gridLayout.cellSize.y);
    }
}
}