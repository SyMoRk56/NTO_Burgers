using System.Collections.Generic;
using UnityEngine;

public class Face : MonoBehaviour
{
    public string textureParamName = "Texture";
    public Material targetMat;
    [Tooltip("обычное - улыбка - рот - грусни")]
    public List<Texture> faces = new List<Texture>();
    public void SetFace(FaceType face)
    {
        targetMat.SetTexture(textureParamName, faces[(int)face]);
    }
}
public enum FaceType
{
    def,
    smile,
    mouth,
    sad
}