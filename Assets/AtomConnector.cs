using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq; // [필수] 종류 계산을 위해 필요

public class AtomConnector : MonoBehaviour
{
    public string elementSymbol = "H"; 
    public GameObject atomPrefab;      
    
    private TextMeshPro textDisplay;   

    void Start()
    {
        textDisplay = GetComponentInChildren<TextMeshPro>();
        if (textDisplay != null)
        {
            // 시작할 때 내 이름 화학식으로 변환 (H-H -> H₂)
            textDisplay.text = GetChemicalFormula(elementSymbol);
            
            textDisplay.enableAutoSizing = true;
            textDisplay.fontSizeMin = 1f;
            textDisplay.fontSizeMax = 50f; 
        }
    }

    void OnTriggerEnter(Collider other)
    {
        AtomConnector otherAtom = other.GetComponent<AtomConnector>();

        if (otherAtom != null && GetComponent<Collider>().enabled && other.enabled)
        {
            if (gameObject.GetInstanceID() < other.gameObject.GetInstanceID()) return;

            if (atomPrefab != null)
            {
                Vector3 spawnPos = (transform.position + other.transform.position) / 2;
                
                GameObject newMolecule = Instantiate(atomPrefab, spawnPos, Quaternion.identity);
                AtomConnector newScript = newMolecule.GetComponent<AtomConnector>();
                
                // 1. 데이터 저장 (예: "H-H-O")
                string rawSymbol = elementSymbol + "-" + otherAtom.elementSymbol;
                newScript.elementSymbol = rawSymbol;
                
                // 2. 화면 표시용 화학식 변환 (예: "H₂O")
                string formattedText = GetChemicalFormula(rawSymbol);

                // 3. 텍스트 및 좌표 설정
                TextMeshPro newText = newMolecule.GetComponentInChildren<TextMeshPro>();
                if (newText != null) 
                {
                    newText.text = formattedText; 
                    float newMaxSize = Mathf.Max(50f / formattedText.Length, 5f);
                    newText.fontSizeMax = newMaxSize;

                    // ==========================================================
                    // [핵심 로직] 원소 종류가 1가지 뿐인가요?
                    // ==========================================================
                    // "-"로 쪼갠 뒤, 중복을 제거(Distinct)하고 개수를 셉니다.
                    // 예: H-H -> 종류 1개 (H) -> 순수 원소
                    // 예: H-O -> 종류 2개 (H, O) -> 화합물
                    int typeCount = rawSymbol.Split('-').Distinct().Count();

                    float yPos = 0f; // 기본은 정중앙(0)

                    // 종류가 딱 1개라면(같은 원소끼리만 뭉쳤다면) 위로 살짝 올림
                    if (typeCount == 1)
                    {
                        yPos = 0.2f; 
                    }

                    // 결정된 좌표 적용
                    newText.rectTransform.localPosition = new Vector3(0f, yPos, -0.5f);
                    // ==========================================================
                }
            }

            GetComponent<Collider>().enabled = false;
            other.enabled = false;
            Destroy(other.gameObject);
            Destroy(gameObject);
        }
    }

    // 화학식 변환기
    string GetChemicalFormula(string rawData)
    {
        if (string.IsNullOrEmpty(rawData)) return "";

        string[] elements = rawData.Split('-');
        Dictionary<string, int> counts = new Dictionary<string, int>();
        
        foreach (var e in elements)
        {
            if (counts.ContainsKey(e)) counts[e]++;
            else counts[e] = 1;
        }

        string result = "";
        var sortedKeys = counts.Keys.OrderBy(k => k); 

        foreach (var key in sortedKeys)
        {
            result += key; 
            int count = counts[key];
            if (count > 1) result += $"<sub>{count}</sub>"; 
        }

        return result;
    }
}