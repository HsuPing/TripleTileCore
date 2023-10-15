using UnityEngine;

[CreateAssetMenu(fileName = "TripleTile", menuName = "Create Triple Tile Stage Data")]
public class TileLayersSO : ScriptableObject
{
    public TileLayer[] TileLayers;
    public ushort DifferentIdCount;
}

[System.Serializable]
public class TileLayer
{
    public Tile[] Tiles;
    public ushort ColCountX;
    public ushort RowCountY;
}

[System.Serializable]
public class Tile
{
    public ushort Id;
    public ushort RowY;
    public ushort ColX;
}
