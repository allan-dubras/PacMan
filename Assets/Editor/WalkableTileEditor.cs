using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WalkableTile))]
public class WalkableTileEditor : Editor
{
    public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
    {
        WalkableTile tile = (WalkableTile)target;

        if (tile == null || tile.sprite == null)
            return null;

        Texture2D spriteTex = AssetPreview.GetAssetPreview(tile.sprite);
        if (spriteTex == null) return null;

        Texture2D tex = new Texture2D(width, height);
        EditorUtility.CopySerialized(spriteTex, tex);
        return tex;
    }
}
