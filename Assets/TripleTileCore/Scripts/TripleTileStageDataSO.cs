using UnityEngine;

[CreateAssetMenu(fileName = "TripleTileStageDataSO", menuName = "ScriptableObject/Create Triple Tile Stage Data")]
public class TripleTileStageDataSO : ScriptableObject
{
    public TileLayer[] TileLayers;
    [Header("關卡有幾個不同ID(手動帶入的不算在內)")]
    public ushort TotalIDAmount;
}

[System.Serializable]
public struct TileLayer
{
    public Tile[] Tiles;
    public ushort ColCountX;
    public ushort RowCountY;
}

[System.Serializable]
public struct Tile
{
    public ushort ID;
    public ushort RowY;
    public ushort ColX;
    public ushort Layer;
}
