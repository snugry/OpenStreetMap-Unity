﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MapSettings))]
public class MapController : MonoBehaviour
{
    /*
     * handle "latitude" as X
     * handle "longitude" as Y
     * Of course this is not MercatorProjection, but will  look atural in a small range. <= I want to use as a game service.
     */
    BuildingMaker buildingMaker;
    RoadMaker roadMaker;
    AreaMaker areaMaker;
    NatureMaker natureMaker;


    MapSettings set;

    // OSM map data
    [SerializeField, TooltipAttribute("OSM map file ${MapName}.osm located in Assets/OSM/Data folder")]
    public string OSMfileName="MapName.osm";



    void Awake(){
        buildingMaker=GetComponent<BuildingMaker>();
        roadMaker=GetComponent<RoadMaker>();
        areaMaker=GetComponent<AreaMaker>();
        natureMaker = GetComponent<NatureMaker>();
        set =GetComponent<MapSettings>();
    }

    void Start()
    {   
        //By this function, Create 3D map from Assets/Data/OSM/${OMSFileName}  
        GenerateMapFromOSM(OSMfileName,new Vector3(0,1,0));
    }

    private void GenerateMapFromOSM(string OSMfileName,Vector3 MapPos){//MapPos will be effective especially when you use multiple maps

        //load .oms map and make buildings, roads
        MapReader map=new MapReader(OSMfileName); //map data is in mapReader.mapData
        Debug.Log("map " + OSMfileName + " loaded");


        GameObject obj=new GameObject();
        obj.name="MAP-GROUP";
        obj.transform.position=MapPos;
        StartCoroutine(buildingMaker.Make(map,set,obj,Vector3.up*0.2f));
        StartCoroutine(roadMaker.Make(map,set,obj,Vector3.up*0.1f));
        StartCoroutine(areaMaker.Make(map,set,obj,Vector3.zero));
        StartCoroutine(natureMaker.Make(map, set, obj, Vector3.zero));

        //ShowMapData
        //Debug.Log("\nLongitude: " + map.mapData.bounds.MinLon + " ~ " + map.mapData.bounds.MaxLon + " , Latitude: " + map.mapData.bounds.MinLat + " ~ " + map.mapData.bounds.MaxLat);
        //Debug.Log("Centre: " + map.mapData.bounds.Centre);
    }
}
