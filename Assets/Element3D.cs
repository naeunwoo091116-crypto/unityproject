// 3D 원소 박스 컴포넌트
// MRDL Periodic Table (MIT License) 참고하여 Meta Quest용으로 재작성
using UnityEngine;
using TMPro;

public class Element3D : MonoBehaviour
{
    // === 3D 박스 구성 요소 (PeriodicTable3D에서 자동 설정) ===
    [HideInInspector] public MeshRenderer boxRenderer;   // 바디 (검정 반투명)
    [HideInInspector] public MeshRenderer[] sideRenderers; // 옆면들 (색상 강조)
    [HideInInspector] public TextMeshPro numberText;   // 원자번호
    [HideInInspector] public TextMeshPro symbolText;   // 원소 기호
    [HideInInspector] public TextMeshPro nameText;     // 원소 이름

    // === 데이터 ===
    [HideInInspector] public ElementData data;

    // === 인터랙션 ===
    private AtomSpawner spawner;
    private Material normalMaterial;
    private Material highlightMaterial;
    private Color baseColor;

    /// <summary>
    /// 원소 데이터로 이 3D 박스를 초기화합니다
    /// </summary>
    public void Setup(ElementData elementData, Color color, GameObject atomPrefab)
    {
        data = elementData;
        baseColor = color;

        // 텍스트 설정
        if (numberText != null) numberText.text = elementData.number;
        if (symbolText != null) symbolText.text = elementData.symbol;
        if (nameText != null) nameText.text = elementData.name;

        // 1. 바디 머티리얼 (검정 반투명 고정)
        normalMaterial = CreateURPMaterial(new Color(0.1f, 0.1f, 0.1f, 0.7f));

        // 2. 악센트 머티리얼 (옆면, 카테고리 색상)
        highlightMaterial = CreateURPMaterial(new Color(color.r, color.g, color.b, 0.8f));

        if (boxRenderer != null)
            boxRenderer.material = normalMaterial;

        if (sideRenderers != null)
        {
            foreach(var r in sideRenderers) r.material = highlightMaterial;
        }

        // AtomSpawner 설정
        spawner = gameObject.GetComponent<AtomSpawner>();
        if (spawner == null) spawner = gameObject.AddComponent<AtomSpawner>();
        spawner.elementSymbol = elementData.symbol;
        spawner.elementColor = color;
        spawner.atomPrefab = atomPrefab;

        // BoxCollider만 추가 (레이캐스트 감지용) — XRGrabInteractable 없음
        if (Application.isPlaying)
        {
            SetupCollider();
        }
    }

    private Material CreateURPMaterial(Color color)
    {
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.SetFloat("_Surface", 1); // Transparent
        mat.SetFloat("_Blend", 0);   // Alpha
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        mat.color = color;
        return mat;
    }

    private void SetupCollider()
    {
        BoxCollider col = GetComponent<BoxCollider>();
        if (col == null)
        {
            col = gameObject.AddComponent<BoxCollider>();
            col.size = new Vector3(0.06f, 0.06f, 0.06f);
        }
    }

    /// <summary>
    /// ControllerPointer에서 호출 — 원자 소환 (컨트롤러 앞 위치 전달)
    /// </summary>
    public void TriggerSpawn(Vector3 spawnPosition)
    {
        if (spawner != null) spawner.SpawnAtom(spawnPosition);
    }

    /// <summary>
    /// 하이라이트 (레이가 원소에 닿을 때)
    /// </summary>
    public void Highlight()
    {
        if (boxRenderer != null)
            boxRenderer.sharedMaterial.color = new Color(0.2f, 0.2f, 0.2f, 0.85f);

        if (sideRenderers != null)
        {
            foreach (var r in sideRenderers)
                r.sharedMaterial.color = new Color(
                    Mathf.Min(baseColor.r * 1.5f, 1f),
                    Mathf.Min(baseColor.g * 1.5f, 1f),
                    Mathf.Min(baseColor.b * 1.5f, 1f),
                    1.0f
                );
        }

        if (symbolText != null) symbolText.color = Color.white;
    }

    /// <summary>
    /// 기본 상태로 복귀 (레이가 원소를 벗어날 때)
    /// </summary>
    public void Dim()
    {
        if (boxRenderer != null)
            boxRenderer.sharedMaterial.color = new Color(0.1f, 0.1f, 0.1f, 0.7f);

        if (sideRenderers != null)
        {
            foreach (var r in sideRenderers)
                r.sharedMaterial.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0.8f);
        }

        if (symbolText != null) symbolText.color = new Color(1f, 1f, 1f, 0.95f);
    }
}
