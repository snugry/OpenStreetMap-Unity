using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NatureMaker : MonoBehaviour
{
    public Material WaterMaterial;

    public GameObject TreePrefab;

    public IEnumerator Make(MapReader map, MapSettings set, GameObject parentObj, Vector3 pos)
    {
        if (map.mapData == null)
        {
            Debug.Log("No Map Data");
            yield break;
        }

        GameObject natureObj = new GameObject();
        natureObj.transform.parent = parentObj.transform;
        natureObj.name = "Nature";

        //add waterways
        foreach (var way in map.mapData.ways.FindAll((w) => { return w.Waterway != null; }))
        {
            GameObject go = new GameObject();
            Vector3 localOrigin = GetCentre(map, way);
            go.transform.position = (localOrigin - map.mapData.bounds.Centre) * set.mag_h;//magnitude
            go.transform.parent = natureObj.transform;
            go.name = way.Name;

            MeshFilter mf = go.AddComponent<MeshFilter>();
            MeshRenderer mr = go.AddComponent<MeshRenderer>();

            mr.material = WaterMaterial;
            float this_water_w = 0.0f;
            if (way.Waterway == "drain")
            {
                this_water_w = set.road_w * 0.5f;
            }
            else if(way.Waterway == "stream" || way.Waterway == "ditch")
            {
                this_water_w = set.road_w * 2f;
            }
            else if (way.Waterway == "river" || way.Waterway == "canal")
            {
                this_water_w = set.road_w * 3f;
            }

            List<Vector3> vectors = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> indices = new List<int>();

            bool isFirstVector = true;
            Vector3 v1_old = Vector3.zero;
            Vector3 v2_old = Vector3.zero;
            Vector3 v3_old = Vector3.zero;
            Vector3 v4_old = Vector3.zero;

            int wayCount = way.NodeIDs.Count;
            for (int i = 1; i < wayCount; i++)
            {
                OSMnode p1 = map.mapData.nodes[way.NodeIDs[i - 1]];
                OSMnode p2 = map.mapData.nodes[way.NodeIDs[i]];

                Vector3 s1 = new Vector3(p1.Longitude, 0, p1.Latitude) - localOrigin;
                Vector3 s2 = new Vector3(p2.Longitude, 0, p2.Latitude) - localOrigin;

                //magnitude horizontal map 
                s1.x *= set.mag_h; s1.z *= set.mag_h;
                s2.x *= set.mag_h; s2.z *= set.mag_h;

                Vector3 diff = (s2 - s1).normalized;
                var cross = Vector3.Cross(diff, Vector3.up) * this_water_w; // width of road

                /*
                Shape: 

                    //normal squad(v1, v2, v3, v4)
                    v3  v4

                    v1  v2

                    //here squad(v1_old, v2_old, v3_old, v4_old), v1(v2) is calculated as (v1(v2) + v3_old)/2
                    (v3)    (v4)
                    v1      v2
                    v1_old  v2_old
                    
                */

                Vector3 v1 = s1 - cross;
                Vector3 v2 = s1 + cross;
                Vector3 v3 = s2 - cross;
                Vector3 v4 = s2 + cross;

                if (isFirstVector)
                {//just set v?_old (? is between 1 and 4) first
                    isFirstVector = false;
                    v1_old = v1;
                    v2_old = v2;
                    v3_old = v3;
                    v4_old = v4;
                    vectors.Add(v1); vectors.Add(v2);
                    normals.Add(-Vector3.up); normals.Add(-Vector3.up);
                    uvs.Add(new Vector2(0, 0)); uvs.Add(new Vector2(1, 0));
                    continue;
                }
                else
                {
                    v1 = (v1 + v3_old) / 2;
                    v2 = (v2 + v4_old) / 2;
                }

                //precise vectors are "v1_old. v2_old, v1, v4". So add them
                /*vectors.Add(v1_old); vectors.Add(v2_old);*/
                vectors.Add(v1); vectors.Add(v2);

                if (i % 2 == 0)
                {
                    uvs.Add(new Vector2(0, 1));
                    uvs.Add(new Vector2(1, 1));
                }
                else
                {
                    uvs.Add(new Vector2(0, 0));
                    uvs.Add(new Vector2(1, 0));
                }
                /*
                uvs.Add(new Vector2(0,0));
                uvs.Add(new Vector2(1,0));
                uvs.Add(new Vector3(0,1));
                uvs.Add(new Vector3(1,1));
                */

                normals.Add(-Vector3.up);
                normals.Add(-Vector3.up);
                /*normals.Add(-Vector3.up);
                normals.Add(-Vector3.up);*/

                int idx1, idx2, idx3, idx4; // index values
                int count = vectors.Count;
                idx4 = count - 1; //v2
                idx3 = count - 2; //v1
                idx2 = count - 3; //v2_old
                idx1 = count - 4; //v1_old


                indices.Add(idx1); indices.Add(idx2); indices.Add(idx3);  // first triangle v1, v3, v2 //one side
                indices.Add(idx3); indices.Add(idx2); indices.Add(idx4); // second triangle v3, v4, v2 //one side
                                                                         /*indices.Add(idx1); indices.Add(idx3); indices.Add(idx2); // third triangle v2, v3, v1 //the other side 
                                                                         indices.Add(idx3); indices.Add(idx2); indices.Add(idx4); // fourth triangle v2, v4, v3 //the other side*/

                if (i == wayCount - 1)
                {//if last: add last triangle v1, v2, v3, v4
                 /*vectors.Add(v1_old); vectors.Add(v2_old);*/
                    vectors.Add(v3); vectors.Add(v4);

                    uvs.Add(new Vector2(0, 1)); uvs.Add(new Vector2(1, 1)); /*uvs.Add(new Vector3(0,1)); uvs.Add(new Vector3(1,1));*/

                    normals.Add(-Vector3.up); normals.Add(-Vector3.up); /*normals.Add(-Vector3.up); normals.Add(-Vector3.up);*/

                    count = vectors.Count;
                    idx4 = count - 1; //v2
                    idx3 = count - 2; //v1
                    idx2 = count - 3; //v2_old
                    idx1 = count - 4; //v1_old

                    indices.Add(idx1); indices.Add(idx2); indices.Add(idx3);  // first triangle v1, v3, v2 //one side
                    indices.Add(idx3); indices.Add(idx2); indices.Add(idx4); // second triangle v3, v4, v2 //one side
                    /*indices.Add(idx1); indices.Add(idx3); indices.Add(idx2); // third triangle v2, v3, v1 //the other side 
                    indices.Add(idx3); indices.Add(idx2); indices.Add(idx4); // fourth triangle v2, v4, v3 //the other side*/
                }


                //ready for next triangles
                v1_old = v1; v2_old = v2; v3_old = v3; v4_old = v4;
            }

            mf.mesh.vertices = vectors.ToArray();
            mf.mesh.normals = normals.ToArray();
            mf.mesh.triangles = indices.ToArray();
            mf.mesh.uv = uvs.ToArray();

            //cast shadow off
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            go.isStatic = true;

            yield return null;
        }
        foreach (var nd in map.mapData.nodes.Values.ToList().FindAll((n) => { return n.isTree == true; }))
        {
            Vector3 treePos = new Vector3(nd.Longitude, 0, nd.Latitude);
            Instantiate(TreePrefab, (treePos - map.mapData.bounds.Centre) * set.mag_h, natureObj.transform.rotation, natureObj.transform);
        }

        natureObj.transform.position = pos;
    }

    private Vector3 GetCentre(MapReader map, OSMway way)
    {
        float lat = 0.0f;
        float lon = 0.0f;
        foreach (var id in way.NodeIDs)
        {
            lat += map.mapData.nodes[id].Latitude;
            lon += map.mapData.nodes[id].Longitude;
        }

        Vector3 total = new Vector3(lon, 0, lat);

        return total / way.NodeIDs.Count;
    }
}
