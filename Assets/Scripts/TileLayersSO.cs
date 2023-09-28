using UnityEngine;

[CreateAssetMenu(fileName = "TripleTile", menuName = "Create Triple Tile Stage Data")]
public class TileLayersSO : ScriptableObject
{
    public TileLayer[] TileLayers;
}

[System.Serializable]
public class TileLayer
{
    public Tile[] Tiles;
    public ushort RowCountX;
    public ushort ColCountY;
    public ushort MatchCount;
}

[System.Serializable]
public class Tile
{
    public ushort Id;
    public ushort RowX;
    public ushort ColY;
}
