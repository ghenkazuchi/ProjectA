using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

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

    private HashSet<Vector3Int> fadingTiles = new HashSet<Vector3Int>();

    /// <summary>
    /// Clears the fog tile around the given grid center position within the visionRadius with a square shape.
    /// </summary>
    public void RevealArea(Vector2Int centerGridPos)
    {
        if (fogTilemap == null) return;

        for (int x = centerGridPos.x - visionRadius; x <= centerGridPos.x + visionRadius; x++)
        {
            for (int y = centerGridPos.y - visionRadius; y <= centerGridPos.y + visionRadius; y++)
            {
                Vector3Int tilePos = new Vector3Int(x, y, 0);
                
                // If there's a tile here and it's not already fading
                if (fogTilemap.HasTile(tilePos) && !fadingTiles.Contains(tilePos))
                {
                    StartCoroutine(FadeTileOut(tilePos));
                }
            }
        }
    }

    private System.Collections.IEnumerator FadeTileOut(Vector3Int pos)
    {
        fadingTiles.Add(pos);

        // We must remove the LockColor flag from the tile instance to allow color changing
        fogTilemap.SetTileFlags(pos, TileFlags.None);

        float duration = 0.3f; // seconds to fade
        float elapsed = 0f;
        Color startColor = Color.white;
        Color targetColor = new Color(1, 1, 1, 0);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            fogTilemap.SetColor(pos, Color.Lerp(startColor, targetColor, t));
            yield return null;
        }

        // Fully remove the tile once faded
        fogTilemap.SetTile(pos, null);
        fadingTiles.Remove(pos);
    }
}
