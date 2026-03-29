using UnityEditor;
using UnityEngine;
using FillefranzTools;
using System.Collections.Generic;
using System.IO;

namespace Atlas
{
    public class MapMaker : EditorWindow
    {
        static float shading = 0.5f;
        static string exportPath = "";
        float width => position.size.x;
        float height => position.size.y;

        Texture2D mapTexture;

        Vector3 lightDirection = Vector3.up;
        int shadeSmoothing = 3;
        static float brightness = 1;

       Dictionary<Texture2D, Color> tex2Color = new Dictionary<Texture2D, Color>();


        [MenuItem("Tools/Atlas/Terrain To PNG")]
        public static void OpenWindow()
        {
            MapMaker window = GetWindow<MapMaker>();
            window.titleContent = new GUIContent("Terrain To PNG");


        }




        private void OnGUI()
        {
            Vector2 buttonSize = new Vector2(100, 50);
            float spacing = 10;

            float mapPadding = 20f;
            float mapWidth = Mathf.Min(width - mapPadding * 2, height - buttonSize.y - mapPadding *2 - spacing *2 );


            if (mapTexture != null)
            {
                

                Vector2 mapSize = new Vector2(mapWidth, mapWidth);
                Vector2 mapPos = new Vector2(mapPadding, mapPadding);
                GUI.Label(new Rect(mapPos, mapSize), mapTexture);

            }

            Colors();
            Buttons();
            ShadeParameters();

            void Colors()
            {
                float mapStopY = mapWidth + mapPadding * 2 + spacing;
                int numberOfLayers = tex2Color.Count;
                float colorWidth = (width - mapPadding * 2 - spacing * (numberOfLayers +1))/numberOfLayers;

                int i = 0;
                Dictionary<Texture2D, Color> temp = new Dictionary<Texture2D, Color>();

                foreach (KeyValuePair<Texture2D, Color> texCol in tex2Color)
                {
                    Vector2 texSize = new Vector2(colorWidth * .25f, colorWidth * .25f);
                    Vector2 texPos = new Vector2(mapPadding + colorWidth * i + spacing *(i+1), mapStopY);

                    GUI.Label(new Rect(texPos, texSize), texCol.Key);
                    
                    Vector2 colorSize = new Vector2(colorWidth * .75f, texSize.y);
                    Vector2 colorPos = new Vector2(texPos.x + texSize.x, texPos.y);

                    Color color = EditorGUI.ColorField(new Rect(colorPos, colorSize), texCol.Value);
                    temp[texCol.Key] = color;

                    i++;
                }

                tex2Color= temp;

            }

            void Buttons()
            {
                Vector2 reloadPos = new Vector2(0, height - buttonSize.y *2);
                Vector2 exportPos = new Vector2(0, height - buttonSize.y);

                if (GUI.Button(new Rect(reloadPos, buttonSize), "Reload Map"))
                {
                    RefreshMap();
                }

                if (GUI.Button(new Rect(exportPos, buttonSize), "Export Map"))
                {
                    exportPath= EditorUtility.SaveFilePanel("Save Map", exportPath, "Map", "png");

                    if (exportPath != string.Empty)
                    {
                        byte[] texData = mapTexture.EncodeToPNG();
                        File.WriteAllBytes(exportPath, texData);
                        Debug.Log("Succesfully exported map at: " + exportPath);
                    }
                }

            }

            void ShadeParameters()
            {
                
                Vector2 sliderSize = new Vector2(200, 20);
                Vector2 sliderPos = new Vector2(buttonSize.x + spacing, height -sliderSize.y - buttonSize.y);
                shading = EditorGUI.Slider(new Rect(sliderPos, sliderSize), shading, 0, 1);



                

                Vector2 textSize = new Vector2(70, 15);
                Vector2 textPos = new Vector2(sliderPos.x, sliderPos.y - sliderSize.y);

                GUI.Label(new Rect(textPos, textSize), "Shading:");


                Vector2 lightSize = new Vector2(sliderSize.x, sliderSize.y);
                Vector2 lightPos = new Vector2(sliderPos.x, sliderPos.y + spacing * 2);

                lightDirection = EditorGUI.Vector3Field(new Rect(lightPos, lightSize),"Light Direction", lightDirection);

                Vector2 alignSize = new Vector2(lightSize.x, lightSize.y);
                Vector2 alignPos = new Vector2(lightPos.x + lightSize.x + spacing, lightPos.y + lightSize .y);

                if (GUI.Button(new Rect(alignPos, alignSize), "Align To Directional Light"))
                {
                    Light[] lights = FindObjectsOfType<Light>();

                    Light directionalLight = null;

                    for(int i = 0; i < lights.Length; i++)
                    {
                        if (lights[i].type == UnityEngine.LightType.Directional)
                        {
                            directionalLight= lights[i];
                            break;
                        }
                    }

                    if (directionalLight != null)
                        lightDirection = -directionalLight.transform.forward;

                    else
                        Debug.LogWarning("No directional light found in scene");
                }

                textPos = new Vector2(sliderPos.x + sliderSize.x + spacing, sliderPos.y);
                GUI.Label(new Rect(textPos, textSize), "Smoothing:");

                Vector2 smoothingSize = new Vector2(30, sliderSize.y);
                Vector2 smootingPos = new Vector2(textPos.x + textSize.x + spacing, sliderPos.y);

                shadeSmoothing = EditorGUI.IntField(new Rect(smootingPos, smoothingSize), shadeSmoothing);

                textPos.x = smootingPos.x + spacing + smoothingSize.x;

                GUI.Label(new Rect(textPos, textSize), "Brightness");

                Vector2 brightnessSize = smoothingSize;
                Vector2 brightnessPos = new Vector2(textPos.x + textSize.x + spacing, textPos.y);

                brightness = EditorGUI.FloatField(new Rect(brightnessPos, brightnessSize), brightness);



            }


        }

        void RefreshMap()
        {
            

            Terrain[] terrains1D = FindObjectsOfType<Terrain>();
            Vector2 terrainDimensions = terrains1D[0].terrainData.size.FromXZ();

            float lowestX =  float.MaxValue;
            float lowestZ = float.MaxValue;
            Terrain terrain00 = null;

            for (int i = 0; i < terrains1D.Length; i++)
            {
                if (terrains1D[i].transform.position.x <= lowestX && terrains1D[i].transform.position.z <= lowestZ)
                {
                    lowestX = terrains1D[i].transform.position.x;
                    lowestZ = terrains1D[i].transform.position.z;
                    terrain00 = terrains1D[i];
                }
            }

            Vector2Int gridSize = Vector2Int.zero;

            for (int i = 0; i < terrains1D.Length; i++)
            {
                int x = ((terrains1D[i].transform.position.x - terrain00.transform.position.x) / terrainDimensions.x).Round();
                int y = ((terrains1D[i].transform.position.z - terrain00.transform.position.z) / terrainDimensions.y).Round();

                gridSize.x = Mathf.Max(gridSize.x, x);
                gridSize.y = Mathf.Max(gridSize.y, y);
            }

            gridSize += Vector2Int.one;
            Terrain[,] terrains = new Terrain[gridSize.x, gridSize.y];

            for (int i = 0; i < terrains1D.Length; i++)
            {
                int x = ((terrains1D[i].transform.position.x - terrain00.transform.position.x) / terrainDimensions.x).Round();
                int y = ((terrains1D[i].transform.position.z - terrain00.transform.position.z) / terrainDimensions.y).Round();

                terrains[x, y] = terrains1D[i];
            }

            Texture2D[,] maps = new Texture2D[gridSize.x, gridSize.y];

            
            

            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    int heightmapRes = terrains[x, y].terrainData.heightmapResolution;
                    int alphamapWidth = terrains[x, y].terrainData.alphamapWidth;
                    int alphamapHeight = terrains[x, y].terrainData.alphamapHeight;
                    int alphamapLayers = terrains[x, y].terrainData.alphamapLayers;
                    Texture2D texture = new Texture2D(alphamapWidth, alphamapHeight);
                    Color[] pixels = new Color[alphamapWidth * alphamapHeight];
                    

                    if (terrains[x,y] != null)
                    {
                        float[,] angleMap = new float[heightmapRes, heightmapRes];

                        for (int localX = 0; localX < heightmapRes; localX++)
                        {
                            for (int localY = 0; localY < heightmapRes; localY++)
                            {
                                if(shading > 0)
                                {
                                    float x01 = ((float)localX) / heightmapRes;
                                    float y01 = ((float)localY) / heightmapRes;
                                    Vector3 normal = terrains[x, y].terrainData.GetInterpolatedNormal(y01, x01);
                                    float angleValue = 1-Mathf.Abs(Vector3.Angle(normal, lightDirection)) / 90f;
                                    angleValue = Mathf.Clamp01(angleValue * brightness);
                                    angleValue = Helper.Remap(angleValue, 0, 1, 1-shading, 1);
                                    angleMap[localX, localY] = angleValue;
                                }

                                else
                                {
                                    angleMap[localX, localY] = 1;
                                }
                                
                                
                            }
                        }

                        if (shadeSmoothing > 0)
                            angleMap =angleMap.ApplyGaussianBlur(shadeSmoothing);

                        float[,,] alphaMap = terrains[x, y].terrainData.GetAlphamaps(0, 0, alphamapWidth, alphamapHeight);

                        for (int layer = 0; layer < alphamapLayers; layer++)
                        {
                            Texture2D terrainTex = terrains[x, y].terrainData.terrainLayers[layer].diffuseTexture;

                            if (!tex2Color.ContainsKey(terrainTex))
                                tex2Color.Add(terrainTex, AverageColor(terrainTex));
                        }

                        int pixel = 0;

                        for (int alphaX = 0; alphaX < alphamapWidth; alphaX++)
                        {
                            for (int alphaY = 0; alphaY < alphamapHeight; alphaY++)
                            {
                                Color color = Color.black;

                                for (int z = 0; z < alphamapLayers; z++)
                                {
                                    color = Color.Lerp(color, tex2Color[terrains[x, y].terrainData.terrainLayers[z].diffuseTexture], alphaMap[alphaX, alphaY, z]);
                                }

                                
                                Vector2Int angleCoords = Alpha2Height(alphaX, alphaY);
                                color = color * angleMap[angleCoords.x, angleCoords.y];
                                color.a = 1f;
                                pixels[pixel] = color;
                                
                                pixel++;
                            }
                        }
                    }

                    texture.SetPixels(pixels);
                    texture.Apply();
                    maps[x, y] = texture;


                    Vector2Int Alpha2Height(int x, int y)
                    {
                        return new Vector2Int((((float)x) / alphamapWidth * heightmapRes).Round(), (((float)y) / alphamapHeight * heightmapRes).Round());
                    }
                }
            }
            
            mapTexture = CombineTextures(maps);

        }
        public static Color AverageColor(Texture2D texture)
        {
            if (!texture.isReadable) return Color.white;

            Color[] pixels = texture.GetPixels(0, 0, texture.width, texture.height);
            Color avg = Color.black;

            for (int i = 0; i < pixels.Length; i++)
            {
                avg += pixels[i];
            }

            avg /= pixels.Length;

            return avg;
        }

        public static Texture2D CombineTextures(Texture2D[,] textures)
        {
            if (textures.Length == 1) return textures[0, 0];

            int width = textures.GetLength(0) * textures[0, 0].width;
            int height = textures.GetLength(1) * textures[0, 0].height;

            Texture2D combinedTexture = new Texture2D(width, height);

            for (int x = 0; x < textures.GetLength(0); x++)
            {
                for (int y = 0; y < textures.GetLength(1); y++)
                {
                    Texture2D texture = textures[x, y];

                    Color[] pixels = texture.GetPixels();

                    int startX = x * texture.width;
                    int startY = y * texture.height;

                    for (int i = 0; i < pixels.Length; i++)
                    {
                        int posX = i % texture.width;
                        int posY = i / texture.width;

                        int combinedX = startX + posX;
                        int combinedY = startY + posY;

                        combinedTexture.SetPixel(combinedX, combinedY, pixels[i]);
                    }
                }
            }

            combinedTexture.Apply();
            return combinedTexture;
        }


        private void OnDisable()
        {

        }

    }

}