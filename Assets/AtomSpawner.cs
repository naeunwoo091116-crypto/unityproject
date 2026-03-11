using UnityEngine;
using UnityEngine.UI; // UI 버튼 기능을 위해 필요
using TMPro;

public class AtomSpawner : MonoBehaviour
{
    [Header("데이터 (자동 설정됨)")]
    public string elementSymbol;
    public Color elementColor;
    public GameObject atomPrefab; // 생성할 원자 프리팹

    // 버튼이 클릭되었을 때 실행될 함수 (위치 직접 지정)
    public void SpawnAtom(Vector3 spawnPos)
    {
        if (atomPrefab == null)
        {
            Debug.LogWarning("원자 프리팹이 연결되지 않았습니다!");
            return;
        }

        // 1. 원자 생성 (전달받은 위치에 생성)
        GameObject newAtom = Instantiate(atomPrefab, spawnPos, Quaternion.identity);

        // 2. elementSymbol을 AtomConnector에 전달 (텍스트가 올바른 원소 기호를 표시하도록)
        var connector = newAtom.GetComponent<AtomConnector>();
        if (connector != null) connector.elementSymbol = elementSymbol;

        // 3. 생성된 원자의 색상 변경 — TMP 및 ParticleSystem 렌더러를 건너뜀
        //    Atom (2) prefab: 루트에 MeshRenderer 없음, Text(TMP) 자식과 Particle System 자식만 Renderer 보유
        //    - TMP 렌더러: TextMeshPro 컴포넌트로 식별하여 건너뜀
        //    - ParticleSystemRenderer: ParticleSystem 컴포넌트로 식별하여 건너뜀
        //      (건너뛰지 않으면 URP/Lit 불투명 머티리얼이 적용되어 사각형 배경처럼 보임)
        Renderer sphereRenderer = null;
        foreach (var r in newAtom.GetComponentsInChildren<Renderer>())
        {
            if (r.gameObject.GetComponent<TextMeshPro>() == null &&
                r.gameObject.GetComponent<ParticleSystem>() == null)
            {
                sphereRenderer = r;
                break;
            }
        }
        if (sphereRenderer != null)
        {
            sphereRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            sphereRenderer.material.color = elementColor;
        }

        // 4. 파티클 색상을 원소 카테고리 색상으로 설정 (원소별 다른 빛 색상)
        var ps = newAtom.GetComponentInChildren<ParticleSystem>();
        if (ps != null)
        {
            var main = ps.main;
            main.startColor = elementColor;
        }

        // 예시: 이름 변경
        newAtom.name = $"Atom_{elementSymbol}";

        Debug.Log($"{elementSymbol} 원자 소환!");
    }
}
