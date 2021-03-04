
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridInventoryTemp : MonoBehaviour
{
    public GameObject gridParent;
    public GameObject gridPrefab;
    public GameObject gridLayoutElement;
    public GridLayoutGroup _grp;
    public int maxHeight;
    // testing var
    public int numberOfSlots;

    private int numberOfColumns;
    private int numberOfRows;
    // right now the cells are square, but just in case I'll keep w/h seperate
    private float gridCellWidth;
    private float gridCellHeight;

    void Start()
    {
        // get number of columns from grid component
        numberOfColumns = gridLayoutElement.GetComponent<GridLayoutGroup>().constraintCount;
		// start with minimum number of rows
        numberOfRows = 1;
        // get cell size from grid component
        gridCellWidth = gridLayoutElement.GetComponent<GridLayoutGroup>().cellSize.x;
        gridCellHeight = gridLayoutElement.GetComponent<GridLayoutGroup>().cellSize.y;

		int j = 0;
        // TODO: replace this with actual inventory creation methods
        for (int i = 0; i < numberOfSlots; i++)
        {
            var gridElement = Instantiate(gridPrefab);
            // gridLayoutElement will take care of layout, gridElement just needs to be its child
            gridElement.transform.SetParent(gridLayoutElement.transform); ///Handles the positioning dont need to set anything else , just add as child
            // reset scale to 1, guessing another script is increasing the scale
            gridElement.gameObject.transform.localScale = new Vector3(1,1,1);

			// count rows
            if (j == numberOfColumns)
            {
				numberOfRows++;
				j = 0;
            }
			j++;
		}

		// 73 is the combined width of element padding and scrollbar
		float parentWidth = (gridCellWidth * numberOfColumns) + 73;
		// 77 is the combined height of element padding and text label
		float parentHeight = (gridCellHeight * numberOfRows) + 77;

		// activates scroll bar if element is too tall
		if (parentHeight > maxHeight)
		{
			parentHeight = maxHeight;
		}

        Debug.Log($"Size for hack = <color=orange> {new Vector2(parentWidth, parentHeight)}</color>");
		// sets calculated width and height
		gridParent.GetComponent<Image>().rectTransform.sizeDelta = new Vector2(parentWidth, parentHeight);
    }
}
