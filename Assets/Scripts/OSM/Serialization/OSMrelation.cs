using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class OSMrelation : OSM_Func
{
    public ulong ID { get; private set; }
    public bool Visible { get; private set; }

    public OSMrelation(XmlNode node)
    {

    }
}
