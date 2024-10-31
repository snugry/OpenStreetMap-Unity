using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EleLocation
{
    public float latitude;
    public float longitude;

    public EleLocation(float latitude, float longitude)
    {
        this.latitude = latitude;
        this.longitude = longitude;
    }

    public string ToJsonString()
    {
        return JsonUtility.ToJson(this);
    }
}

[System.Serializable]
public class EleLocations
{
    [SerializeField]
    public List<EleLocation> locations;

    public EleLocations()
    {
        locations = new List<EleLocation>(); 
    }

    public string ToJsonString()
    {
        return JsonUtility.ToJson(this);
    }

    public void AddLocation(float latitude, float longitude)
    {
        locations.Add(new EleLocation(latitude, longitude));
    }
}

[System.Serializable]
public class EleResult
{
    public float latitude;
    public float longitude;
    public float elevation;
}

[System.Serializable]
public class EleResults : IEnumerable
{
    [SerializeField]
    public List<EleResult> results;

    public static EleResults CreateFromJSON(string jsonString)
    {
        return JsonUtility.FromJson<EleResults>(jsonString);
    }

    public IEnumerator GetEnumerator()
    {
        return ((IEnumerable)results).GetEnumerator();
    }
}
