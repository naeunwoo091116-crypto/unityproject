// 원소 데이터 클래스 (MRDL Periodic Table JSON 기반)
// 원본: https://github.com/microsoft/MRDL_Unity_PeriodicTable (MIT License)
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ElementData
{
    public string name;
    public string category;
    public string spectral_img;
    public int xpos;
    public int ypos;
    public string named_by;
    public float density;
    public string color;
    public float molar_heat;
    public string symbol;
    public string discovered_by;
    public string appearance;
    public float atomic_mass;
    public float melt;
    public string number;
    public string source;
    public int period;
    public string phase;
    public string summary;
    public int boil;
}

[System.Serializable]
public class ElementsData
{
    public List<ElementData> elements;

    public static ElementsData FromJSON(string json)
    {
        return JsonUtility.FromJson<ElementsData>(json);
    }
}
