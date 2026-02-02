using UnityEngine;
using UnityEngine.UI; // UI 버튼 기능을 위해 필요

public class AtomSpawner : MonoBehaviour
{
    [Header("데이터 (자동 설정됨)")]
    public string elementSymbol;
    public Color elementColor;
    public GameObject atomPrefab; // 생성할 원자 프리팹

    // 버튼이 클릭되었을 때 실행될 함수
    public void SpawnAtom()
    {
        if (atomPrefab == null)
        {
            Debug.LogWarning("원자 프리팹이 연결되지 않았습니다!");
            return;
        }

        // 1. 원자 생성 (내 위치보다 0.5m 앞에서 생성)
        Vector3 spawnPos = transform.position + (transform.forward * -0.3f); 
        GameObject newAtom = Instantiate(atomPrefab, spawnPos, Quaternion.identity);

        // 2. 생성된 원자의 색상이나 텍스트 변경 (프리팹 구조에 따라 다름)
        // 예시: 렌더러 색상 변경
        var renderer = newAtom.GetComponent<Renderer>();
        if (renderer != null) renderer.material.color = elementColor;

        // 예시: 이름 변경
        newAtom.name = $"Atom_{elementSymbol}";

        Debug.Log($"{elementSymbol} 원자 소환!");
    }
}