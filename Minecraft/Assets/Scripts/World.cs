using UnityEngine;

public class World : MonoBehaviour
{
    [SerializeField]
    Material material;
    
    public BlockType[] blockTypes;
}

[System.Serializable]
public struct BlockType
{
    public string name;
    public bool isDolid;

    [Header("Texture values")]
    public int backFace;
    public int frontFace;
    public int topFace;
    public int buttomFace;
    public int leftFace;
    public int rightFace;

    // Back, Front, Top, Buttom, Left, Right
    public int GetTextureId(int faceId)
    {
        switch(faceId)
        {
            case 0:
                return backFace;
            case 1:
                return frontFace;
            case 2:
                return topFace;
            case 3:
                return buttomFace;
            case 4:
                return leftFace;
            case 5:
                return rightFace;
            default:
                return 0;
        }
    }
}
