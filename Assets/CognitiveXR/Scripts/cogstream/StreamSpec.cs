using System.Collections.Generic;

[System.Serializable]
public class Attributes : Dictionary<string, List<string>> 
{
    public Attributes(){}
}

[System.Serializable]
public struct StreamSpec
{
    public string engineAddress;
    public Attributes attributes;
}
