using UnityEngine;
using UnityEngine.Tilemaps;

public class FogOfWarManager : MonoBehaviour
{
    public static FogOfWarManager Instance { get; private set; }

    [Header("References")]
    public Tilemap fogTilemap;
    public TileBase fogTile;

    [Header("Settings")]
    public int visionRadius = 3;
    
    // Add margin so fog extends slightly beyond map edges to cover the screen
    private const int margin = 10; 

    private VoronoiPathGenerator pathGenerator;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        pathGenerator = FindObjectOfType<VoronoiPathGenerator>();
        if (pathGenerator != null)
        {
            pathGenerator.OnMapGenerated += InitializeFog;
        }
    }

    private void OnDestroy()
    {
        if (pathGenerator != null)
        {
            pathGenerator.OnMapGenerated -= InitializeFog;
        }
    }

    private void InitializeFog()
    {
        if (fogTilemap == null || fogTile == null)
        {
            Debug.LogWarning("FogOfWarManager expects a fogTilemap and a fogTile to be assigned!");
            return;
        }

        fogTilemap.ClearAllTiles();

        Vector2Int mapSize = new Vector2Int(
            Mathf.CeilToInt(pathGenerator.mapRegionSize.x),
            Mathf.CeilToInt(pathGenerator.mapRegionSize.y)
        );

        // Fill the map area plus a margin with the solid black fog tile
        for (int x = -margin; x < mapSize.x + margin; x++)
        {
            for (int y = -margin; y < mapSize.y + margin; y++)
            {
                fogTilemap.SetTile(new Vector3Int(x, y, 0), fogTile);
            }
        }
    }

    /// <summary>
    /// Clears the fog tile around the given grid center position within the visionRadius.
    /// </summary>
    public void RevealArea(Vector2Int centerGridPos)
    {
        if (fogTilemap == null) return;

        for (int x = centerGridPos.x - visionRadius; x <= centerGridPos.x + visionRadius; x++)
        {
            for (int y = centerGridPos.y - visionRadius; y <= centerGridPos.y + visionRadius; y++)
            {
                // Create a rough circular reveal (or blocky, depending on the distance check)
                // Use manhattan or euclidean distance. We'll use euclidean for a rounded square/circle blocky feel.
                if (Vector2.Distance(centerGridPos, new Vector2(x, y)) <= visionRadius)
                {
                    fogTilemap.SetTile(new Vector3Int(x, y, 0), null);
                }
            }
        }
    }
}
