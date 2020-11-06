using UnityEngine;
using UnityEngine.UI;

public class DebugScreen : MonoBehaviour
{
    World world;
    Text text;

    float frameRate;
    float timer;

    float zeroX;
    float zeroY;

    void Start()
    {
        world = GameObject.Find("World").GetComponent<World>();
        text = GetComponent<Text>();

        zeroX = VoxelData.worldSizeInVoxels / 2;
        zeroY = VoxelData.worldSizeInVoxels / 2;
    }

    void Update()
    {
        text.text = "Alexey Kristev's Minecraft like game\n" +
                    frameRate + " fps\n" +
                    "XYZ: " + (world.player.position.x - zeroX) + " / " + world.player.position.y + " / " + (world.player.position.z - zeroY) + "\n";

        if (timer > 1f)
        {
            frameRate = (1f / Time.unscaledDeltaTime);
            timer = 0f;
        }
        else
            timer += Time.deltaTime;
    }
}
