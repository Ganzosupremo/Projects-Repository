using UnityEngine;

public class GridNode
{
    private int gridWidth;
    private int gridHeight;

    private AStarNode[,] gridNode;

    public int GridSize { get { return gridWidth * gridHeight; } }

    public GridNode(int gridWidth, int gridHeight)
    {
        this.gridWidth = gridWidth;
        this.gridHeight = gridHeight;

        gridNode = new AStarNode[gridWidth, gridHeight];

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                gridNode[x, y] = new AStarNode(new Vector2Int(x, y));
            }
        }
    }
    
    /// <summary>
    /// Get the 2D array of the grid node
    /// </summary>
    /// <param name="xPosition">The x position of the grid node to return</param>
    /// <param name="yPosition">The y position of the grid node to return</param>
    /// <returns>Returns the grid node with x and y positions filled up.
    /// Returns null if the passed x and y positions are out of range.</returns>
    public AStarNode GetGridNode(int xPosition, int yPosition)
    {
        if (xPosition < gridWidth && yPosition < gridHeight)
        {
            return gridNode[xPosition, yPosition];
        }
        else
        {
            Debug.Log("Requested Grid Node is out of range");
            return null;
        }
    }
}
