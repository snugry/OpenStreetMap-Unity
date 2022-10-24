using System;
using System.Xml;

public class OSMnode: OSM_Func
{
    public ulong ID { get; set;}
    public float Latitude { get; set;}
    public float Longitude { get; set;}
    public bool Visible { get; set;}
    public bool isTree { get; set; }

    public OSMnode(XmlNode node)
    {
        this.ID = GetAttribute<ulong>("id", node.Attributes);
        this.Latitude = GetAttribute<float>("lat", node.Attributes);// / 1000000;
        this.Longitude = GetAttribute<float>("lon", node.Attributes);// / 1000000;
        this.Visible = GetAttribute<bool>("visible", node.Attributes);
        this.isTree = checkTree(node.SelectNodes("tag"));
    }

    private bool checkTree(XmlNodeList tags)
    {
        bool tree = false;
        foreach (XmlNode t in tags)
        {
            string key = GetAttribute<string>("k", t.Attributes);
            string value = GetAttribute<string>("v", t.Attributes);

            if (key == "natural" && value == "tree")
            {
                tree = true;
            }
        }

        return tree;
     }

}

