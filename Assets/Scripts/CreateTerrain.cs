using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CreateTerrain : MonoBehaviour
{
    public Texture2D terrainTex;

    public float minLat;
    public float maxLat;
    public float minLon;
    public float maxLon;

    public int step = 10;

    [SerializeField]
    private float width_realX;
    [SerializeField]
    private float widthX;

    [SerializeField]
    private float width_realZ;
    [SerializeField]
    private float widthZ;

    private float stepX;
    private float stepZ;

    private int _partsX = 0;
    private int _partsZ = 0;

    private TerrainData _terrainData;
    private GameObject _terrain;

    // Start is called before the first frame update
    void Start()
    {
        width_realX = (maxLat - minLat);
        widthX = width_realX * 100000;

        Debug.Log("width: " + widthX + " with mercator: " + MercatorProjection.lonToX(widthX));
        stepX = width_realX / step;

        width_realZ = (maxLon - minLon);
        widthZ = width_realZ * 100000;
        stepZ = width_realZ / step;

        _terrainData = new TerrainData();

        _terrainData.size = new Vector3(widthX, 500, widthZ);

        TerrainLayer terrainLayer = new TerrainLayer();
        terrainLayer.diffuseTexture = terrainTex;

        TerrainLayer[] layers = new TerrainLayer[1];
        layers[0] = terrainLayer;
        _terrainData.terrainLayers = layers;
        

        _terrain = Terrain.CreateTerrainGameObject(_terrainData);
        _terrain.transform.SetPositionAndRotation(_terrain.transform.position - new Vector3(widthX, 1, widthZ), _terrain.transform.rotation);

        EleLocations locs = new EleLocations();
        for(float x = minLat; x<= maxLat; x += stepX)
        { 
            for(float z=minLon; z <= maxLon;  z += stepZ)
            {
                locs.AddLocation(x, z);
                _partsZ++;
            }
            _partsX++;
        }
        Debug.Log(locs.ToJsonString());
        StartCoroutine(Upload(locs.ToJsonString()));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator Upload(string json)
    {
        WWWForm form = new WWWForm();
        form.AddField("d", json);

        using (UnityWebRequest www = UnityWebRequest.Put("https://api.open-elevation.com/api/v1/lookup", json))
        {
            www.method = UnityWebRequest.kHttpVerbPOST;

            www.SetRequestHeader("Accept", "application/json");
            www.SetRequestHeader("Content-Type", "application/json");
            
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log(www.downloadHandler.text);

                StartCoroutine(UpdateTerrain(EleResults.CreateFromJSON(www.downloadHandler.text)));
            }
        }
    }

    IEnumerator UpdateTerrain(EleResults elevations)
    {
        _terrainData.heightmapResolution = _partsX;
        float[,] eleArray = new float[_terrainData.heightmapResolution, _terrainData.heightmapResolution];

        int x = 0, z = 0;
        foreach(EleResult ele in elevations)
        {
            float elePoint = ele.elevation / 1000;
            eleArray[x, z] = elePoint;
            //Debug.Log(" elevation for: x:" + x + " z:" + z + " ele:" +elePoint);
            yield return true;
            x++;
            if(x == _terrainData.heightmapResolution)
            {
                x = 0;
                z++;
            }
            if(z == _terrainData.heightmapResolution)
            {
                break;
            }
        }
        _terrainData.SetHeights(0, 0, eleArray);
    }
}
