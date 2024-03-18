using UnityEngine;

public class UnitDrag : MonoBehaviour
{
    [SerializeField]
    RectTransform boxVisual;
    Rect selectionBox;
    Vector2 startPosition;
    Vector2 endPosition;

    void Start()
    {
        startPosition = Vector2.zero;
        endPosition = Vector2.zero;
        selectionBox = new Rect();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            startPosition = Input.mousePosition;
        }
        if (Input.GetMouseButton(0))
        {
            endPosition = Input.mousePosition;
            DrawVisual();
            DrawSelection();
        }
        if (Input.GetMouseButtonUp(0))
        {
            SelectUnits();
            startPosition = Vector2.zero;
            endPosition = Vector2.zero;
            DrawVisual();
        }
    }

    void DrawVisual()
    {
        if (boxVisual != null)
        {
            Vector2 boxStart = startPosition;
            Vector2 boxEnd = endPosition;

            Vector2 boxCenter = (boxStart + boxEnd) / 2;

            boxVisual.position = boxCenter;

            Vector2 boxSize = new Vector2(Mathf.Abs(boxStart.x - boxEnd.x), Mathf.Abs(boxStart.y - boxEnd.y));

            boxVisual.sizeDelta = boxSize;
        }
    }

    void DrawSelection()
    {
        if (boxVisual != null)
        {
            if (Input.mousePosition.x < startPosition.x)
            {
                selectionBox.xMin = Input.mousePosition.x;
                selectionBox.xMax = startPosition.x;
            }
            else
            {
                selectionBox.xMin = startPosition.x;
                selectionBox.xMax = Input.mousePosition.x;
            }
            if (Input.mousePosition.y < startPosition.y)
            {
                selectionBox.yMin = Input.mousePosition.y;
                selectionBox.yMax = startPosition.y;
            }
            else
            {
                selectionBox.yMin = startPosition.y;
                selectionBox.yMax = Input.mousePosition.y;
            }
        }
    }

    void SelectUnits()
    {
        foreach (var unit in UnitSelections.Instance.unitList)
        {
            if (selectionBox.Contains(Camera.main.WorldToScreenPoint(unit.transform.position)))
            {
                UnitSelections.Instance.DragSelect(unit);
            }
        }
    }
}
