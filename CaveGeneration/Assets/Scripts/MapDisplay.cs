using UnityEngine;

public class MapDisplay : MonoBehaviour
{
    [SerializeField]
    Renderer textureRenderer;
    public float planeScale;
    
    public void DrawTexture(Texture2D texture)
    {
        textureRenderer.sharedMaterial.mainTexture = texture;
        textureRenderer.transform.localScale = new Vector3(texture.width * planeScale, 1, texture.height * planeScale);
    }
}