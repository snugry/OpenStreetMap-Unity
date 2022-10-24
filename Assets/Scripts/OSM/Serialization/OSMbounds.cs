using System;
using System.Xml;
using UnityEngine;

public class OSMbounds: OSM_Func
{
    /*
     * Latitude
     * Longitude
    */
    public float MinLat{get;set;} 
    public float MaxLat{get;set;} 
    public float MinLon{get;set;}
    public float MaxLon{get;set;}
    public Vector3 Centre{get;set;}//Not "Center" but "Centre"!! Because .osm file is using "centre".


    public OSMbounds(XmlNode node)
    {
        this.MinLat = GetAttribute<float>("minlat", node.Attributes);// / 1000000;
        this.MaxLat = GetAttribute<float>("maxlat", node.Attributes);//  / 1000000;
        this.MinLon = GetAttribute<float>("minlon", node.Attributes);//  / 1000000;
        this.MaxLon = GetAttribute<float>("maxlon", node.Attributes);//  / 1000000; 
        this.Centre=new Vector3((this.MaxLon+this.MinLon)*0.5f,0,(this.MaxLat+this.MinLat)*0.5f);
    }
}
