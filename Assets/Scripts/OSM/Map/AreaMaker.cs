﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaMaker : MonoBehaviour
{
    [System.SerializableAttribute]
    public class LeisureMaterial{//leisure in way
        public Material SandMaterial;
        public Material WaterMaterial;
        public Material GrassMaterial;
        public Material ElseMaterial;
        
    }
    [SerializeField]
    public LeisureMaterial leisureMaterial=new LeisureMaterial();

    /*
     * https://wiki.openstreetmap.org/wiki/Key:leisure
     * already   way: 
     * remaining way: 
     */
    class LeisureClassification{
        public List<string> LookSandValues = new List<string>(){"playground","stadium","track"}; //Apply SandMaterial 
        public List<string> LookWaterValues = new List<string>(){"fishing","slipway","swimming_area","swimming_pool","water_park"}; //Apply WaterMaterial 
        public List<string> LookGrassValues = new List<string>(){"garden","golf_course","miniature_golf","nature_reserve","park","pitch","resort","summer_camp","wildlife_hide"}; //Apply GrassMaterial 
    }
    LeisureClassification leisureClassification=new LeisureClassification();


    public IEnumerator Make(MapReader map, MapSettings set, GameObject parentObj, Vector3 areaPos)
    {
        if(map.mapData==null){
            Debug.Log("No Map Data");
            yield break;
        }
        
        GameObject areaObj = new GameObject();
        areaObj.transform.parent=parentObj.transform;
        areaObj.transform.position=areaPos;
        areaObj.name="Areas";

        foreach (var way in map.mapData.ways.FindAll((w) => { return w.Leisure!="" && w.NodeIDs.Count > 1; }))
        {
            GameObject go = new GameObject();
            Vector3 localOrigin = GetCentre(map,way);
            Vector3 TransformPos=localOrigin - map.mapData.bounds.Centre;
            go.transform.parent=areaObj.transform;

            //magnitude horizontal 
            TransformPos.x*=set.mag_h; TransformPos.z*=set.mag_h;

            go.transform.position = TransformPos;

            MeshFilter mf = go.AddComponent<MeshFilter>();
            MeshRenderer mr = go.AddComponent<MeshRenderer>();


            //classfy highway and assgin each material
            if(leisureClassification.LookSandValues.Contains(way.Leisure)){
                mr.material = leisureMaterial.SandMaterial;
            }else if(leisureClassification.LookWaterValues.Contains(way.Leisure)){
                mr.material = leisureMaterial.WaterMaterial;
            }else if(leisureClassification.LookGrassValues.Contains(way.Leisure)){
                mr.material = leisureMaterial.GrassMaterial;
            }else{
                mr.material = leisureMaterial.ElseMaterial;
            }

            List<Vector3> vectors = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<int> indices = new List<int>();

            OSMnode p0 = map.mapData.nodes[way.NodeIDs[0]];//first point of all triangle
            Vector3 v0 = new Vector3(p0.Longitude,0,p0.Latitude) - localOrigin;
            v0.x*=set.mag_h; v0.z*=set.mag_h;
            vectors.Add(v0); normals.Add(-Vector3.forward);
            int idx0=0;
            for (int i = 2; i < way.NodeIDs.Count; i++)
            {
                OSMnode p1 = map.mapData.nodes[way.NodeIDs[i - 1]];
                OSMnode p2 = map.mapData.nodes[way.NodeIDs[i]];

                Vector3 v1 = new Vector3(p1.Longitude,0,p1.Latitude) - localOrigin;
                Vector3 v2 = new Vector3(p2.Longitude,0,p2.Latitude) - localOrigin;

                //magnitude horizontal  
                v1.x*=set.mag_h; v1.z*=set.mag_h;
                v2.x*=set.mag_h; v2.z*=set.mag_h;                

                vectors.Add(v1);
                vectors.Add(v2);
                normals.Add(-Vector3.forward);
                normals.Add(-Vector3.forward);
                
                // index values
                int idx1, idx2;
                int count=vectors.Count;
                idx2 = count - 1;
                idx1 = count - 2;
                
                indices.Add(idx0); indices.Add(idx1); indices.Add(idx2); // first triangle v1, v3, v2 //one side
                indices.Add(idx2); indices.Add(idx1); indices.Add(idx0); // second triangle v2, v3, v1 //the other side  
                
            }            

            mf.mesh.vertices = vectors.ToArray();
            mf.mesh.normals = normals.ToArray();
            mf.mesh.triangles = indices.ToArray();

            yield return null;
        }

    }

    private Vector3 GetCentre(MapReader map ,OSMway way)
    {
        float lat=0.0f;
        float lon=0.0f;
        foreach (var id in way.NodeIDs)
        {
            lat+=map.mapData.nodes[id].Latitude;
            lon+=map.mapData.nodes[id].Longitude;
        }

        Vector3 total = new Vector3(lon,0,lat);

        return total / way.NodeIDs.Count;
    }
}
