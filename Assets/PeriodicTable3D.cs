// 3D 주기율표 생성기
// MRDL Periodic Table (MIT License) 의 배치 로직을 참고하여 Meta Quest용으로 재작성
// JSON에서 118개 원소를 로드하고, 3D 공간에 원소 박스를 배치합니다.
using UnityEngine;
using TMPro;
using System.Collections.Generic;

[ExecuteAlways]
public class PeriodicTable3D : MonoBehaviour
{
    [Header("원자 프리팹 (기존 Atom 프리팹)")]
    public GameObject atomPrefab;

    [Header("배치 설정")]
    [Tooltip("원소 간 간격 (미터)")]
    public float separation = 0.07f;

    [Tooltip("원소 박스 크기 (기본적으로 모든 축에 동일한 값 권장)")]
    [Min(0.01f)]
    public Vector3 boxSize = new Vector3(0.06f, 0.06f, 0.06f);

    // 카테고리별 색상 (MRDL 스타일)
    private static readonly Dictionary<string, Color> categoryColors = new Dictionary<string, Color>()
    {
        { "alkali metal",            new Color(1.0f, 0.35f, 0.35f) },  // 빨강
        { "alkaline earth metal",    new Color(1.0f, 0.55f, 0.25f) },  // 주황
        { "transition metal",        new Color(1.0f, 0.75f, 0.30f) },  // 노랑
        { "metalloid",               new Color(0.35f, 0.75f, 0.75f) }, // 청록
        { "diatomic nonmetal",       new Color(0.35f, 0.55f, 1.0f) },  // 파랑
        { "polyatomic nonmetal",     new Color(0.35f, 0.55f, 1.0f) },  // 파랑
        { "post-transition metal",   new Color(0.55f, 0.78f, 0.55f) }, // 연두
        { "noble gas",               new Color(0.75f, 0.35f, 1.0f) },  // 보라
        { "actinide",                new Color(0.85f, 0.45f, 0.85f) }, // 분홍
        { "lanthanide",              new Color(0.85f, 0.55f, 0.75f) }, // 분홍 (약간 다르게)
    };
    private static readonly Color defaultColor = new Color(0.5f, 0.5f, 0.5f);

    private List<Element3D> spawnedElements = new List<Element3D>();

    void Start()
    {
        Debug.Log($"[PeriodicTable3D] Start() 호출 — isPlaying: {Application.isPlaying}, childCount: {transform.childCount}");

        if (Application.isPlaying)
        {
            // Play 모드: 기존 에디터 프리뷰 완전히 삭제 후 재생성
            ClearTableImmediate();
            GenerateTable();
        }
        else if (transform.childCount == 0)
        {
            GenerateTable();
        }
    }

    private void OnValidate()
    {
        // 정육면체 유지를 위해 한 축이 변하면 다른 축도 맞춤 (사용자 편의를 위해 구현할 수 있으나, 일단 실시간 반영만 우선)
        // boxSize.y = boxSize.x;
        // boxSize.z = boxSize.x;

#if UNITY_EDITOR
        // 인스펙터에서 값이 변경될 때 실시간으로 반영
        if (!Application.isPlaying && gameObject.activeInHierarchy)
        {
            // OnValidate에서 직접 오브젝트를 생성/삭제하는 것은 위험하므로 delayCall 사용
            UnityEditor.EditorApplication.delayCall -= Regen;
            UnityEditor.EditorApplication.delayCall += Regen;
        }
#endif
    }

#if UNITY_EDITOR
    private void Regen()
    {
        if (this == null) return;
        GenerateTable();
    }
#endif

    [ContextMenu("Generate 3D Periodic Table")]
    public void GenerateTable()
    {
        // 기존 원소 제거
        ClearTable();

        // JSON 로드
        TextAsset jsonAsset = Resources.Load<TextAsset>("JSON/PeriodicTableJSON");
        if (jsonAsset == null)
        {
            Debug.LogError("[PeriodicTable3D] PeriodicTableJSON.json을 Resources/JSON/에서 찾을 수 없습니다!");
            return;
        }

        List<ElementData> elements = ElementsData.FromJSON(jsonAsset.text).elements;
        Debug.Log($"[PeriodicTable3D] {elements.Count}개 원소 로드 완료");

        // 각 원소를 3D 박스로 생성
        foreach (ElementData elem in elements)
        {
            CreateElement3D(elem);
        }

        Debug.Log($"[PeriodicTable3D] 3D 주기율표 생성 완료! ({spawnedElements.Count}개 원소)");
    }

    private void CreateElement3D(ElementData elemData)
    {
        // === 1. 루트 GameObject ===
        GameObject elementObj = new GameObject($"Element3D_{elemData.symbol}");
        elementObj.transform.SetParent(transform, false);

        // MRDL의 배치 공식: xpos, ypos로 주기율표 위치 계산
        float x = elemData.xpos * separation - (separation * 18f / 2f);
        float y = (separation * 9f) - elemData.ypos * separation;
        elementObj.transform.localPosition = new Vector3(x, y, 0f);

        // === 2. 3D 박스 구조 (복합 오브젝트) ===
        // -- 바디 (앞/뒤 담당, 검정 반투명)
        // Z-Fighting 방지를 위해 바디를 옆면보다 1% 아주 살짝 작게 만듭니다 (0.99배)
        GameObject bodyObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bodyObj.name = "Body";
        bodyObj.transform.SetParent(elementObj.transform, false);
        bodyObj.transform.localScale = boxSize * 0.99f; 
        Object.DestroyImmediate(bodyObj.GetComponent<BoxCollider>());
        MeshRenderer bodyRenderer = bodyObj.GetComponent<MeshRenderer>();

        // -- 옆면들 (악센트 색상, 정확한 boxSize 경계에 배치)
        List<MeshRenderer> sideRenderers = new List<MeshRenderer>();
        float thickness = 0.001f; // 옆면 플레이트 두께

        // Top/Bottom (X-Z planes)
        sideRenderers.Add(CreateSidePlate(elementObj.transform, "Side_Top", 
            new Vector3(boxSize.x, thickness, boxSize.z), 
            new Vector3(0, boxSize.y / 2f, 0)));
        sideRenderers.Add(CreateSidePlate(elementObj.transform, "Side_Bottom", 
            new Vector3(boxSize.x, thickness, boxSize.z), 
            new Vector3(0, -boxSize.y / 2f, 0)));
        // Left/Right (Y-Z planes)
        sideRenderers.Add(CreateSidePlate(elementObj.transform, "Side_Left", 
            new Vector3(thickness, boxSize.y, boxSize.z), 
            new Vector3(-boxSize.x / 2f, 0, 0)));
        sideRenderers.Add(CreateSidePlate(elementObj.transform, "Side_Right", 
            new Vector3(thickness, boxSize.y, boxSize.z), 
            new Vector3(boxSize.x / 2f, 0, 0)));

        // === 3. 원자번호 텍스트 (상단) ===
        // 상단에 배치, 박스 높이의 약 1/3 위치
        TextMeshPro numberText = CreateTextMesh(
            elementObj.transform,
            "NumberText",
            elemData.number,
            new Vector3(0f, boxSize.y * 0.35f, -boxSize.z / 2f - 0.001f),
            boxSize.y * 0.6f,   // 폰트 크기를 박스 높이에 비례하게 설정
            TextAlignmentOptions.Center,
            new Color(1f, 1f, 1f, 0.8f)
        );
        numberText.enableAutoSizing = true;
        numberText.fontSizeMin = 0.01f;
        numberText.fontSizeMax = boxSize.y * 1.5f;

        // === 4. 원소 기호 텍스트 (중앙) ===
        TextMeshPro symbolText = CreateTextMesh(
            elementObj.transform,
            "SymbolText",
            elemData.symbol,
            new Vector3(0f, 0f, -boxSize.z / 2f - 0.001f),
            boxSize.y * 2.0f,   // 강조를 위해 더 크게 설정
            TextAlignmentOptions.Center,
            new Color(1f, 1f, 1f, 0.95f)
        );
        symbolText.fontStyle = FontStyles.Bold;
        symbolText.enableAutoSizing = true;
        symbolText.fontSizeMin = 0.02f;
        symbolText.fontSizeMax = boxSize.y * 3.0f;

        // === 5. 원소 이름 텍스트 (하단) ===
        TextMeshPro nameText = CreateTextMesh(
            elementObj.transform,
            "NameText",
            elemData.name,
            new Vector3(0f, -boxSize.y * 0.35f, -boxSize.z / 2f - 0.001f),
            boxSize.y * 0.4f,   // 원자 번호보다 약간 작게
            TextAlignmentOptions.Center,
            new Color(1f, 1f, 1f, 0.7f)
        );
        nameText.enableAutoSizing = true;
        nameText.fontSizeMin = 0.005f;
        nameText.fontSizeMax = boxSize.y * 1.0f;

        // === 6. BoxCollider (루트에, XR용) ===
        BoxCollider col = elementObj.AddComponent<BoxCollider>();
        col.size = new Vector3(boxSize.x, boxSize.y, boxSize.z + 0.005f);

        // === 7. Element3D 컴포넌트 설정 ===
        Element3D element3D = elementObj.AddComponent<Element3D>();
        element3D.boxRenderer = bodyRenderer;
        element3D.sideRenderers = sideRenderers.ToArray();
        element3D.numberText = numberText;
        element3D.symbolText = symbolText;
        element3D.nameText = nameText;

        Color color = GetCategoryColor(elemData.category);
        element3D.Setup(elemData, color, atomPrefab);

        spawnedElements.Add(element3D);
    }

    /// <summary>
    /// 옆면 플레이트 오브젝트를 생성합니다
    /// </summary>
    private MeshRenderer CreateSidePlate(Transform parent, string name, Vector3 scale, Vector3 localPos)
    {
        GameObject plate = GameObject.CreatePrimitive(PrimitiveType.Cube);
        plate.name = name;
        plate.transform.SetParent(parent, false);
        plate.transform.localScale = scale;
        plate.transform.localPosition = localPos;
        Object.DestroyImmediate(plate.GetComponent<BoxCollider>());
        return plate.GetComponent<MeshRenderer>();
    }

    /// <summary>
    /// 3D TextMeshPro 오브젝트를 생성합니다
    /// </summary>
    private TextMeshPro CreateTextMesh(Transform parent, string name, string text,
        Vector3 localPos, float fontSize, TextAlignmentOptions alignment, Color color)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);
        textObj.transform.localPosition = localPos;
        textObj.transform.localRotation = Quaternion.identity;

        TextMeshPro tmp = textObj.AddComponent<TextMeshPro>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.color = color;
        tmp.enableAutoSizing = false;

        // 텍스트 영역 크기를 박스에 맞춤
        RectTransform rt = textObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(boxSize.x * 0.90f, boxSize.y * 0.6f);

        return tmp;
    }

    /// <summary>
    /// 카테고리명으로 색상을 반환합니다
    /// </summary>
    private Color GetCategoryColor(string category)
    {
        if (string.IsNullOrEmpty(category)) return defaultColor;
        string trimmed = category.Trim().ToLower();

        foreach (var pair in categoryColors)
        {
            if (trimmed.Contains(pair.Key))
                return pair.Value;
        }
        return defaultColor;
    }

    /// <summary>
    /// 생성된 모든 원소를 제거합니다 (Destroy 사용 — 프레임 끝에 삭제)
    /// </summary>
    [ContextMenu("Clear Table")]
    public void ClearTable()
    {
        foreach (var elem in spawnedElements)
        {
            if (elem != null)
            {
                if (Application.isPlaying) Destroy(elem.gameObject);
                else DestroyImmediate(elem.gameObject);
            }
        }
        spawnedElements.Clear();

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (child.name.StartsWith("Element3D_"))
            {
                if (Application.isPlaying) Destroy(child.gameObject);
                else DestroyImmediate(child.gameObject);
            }
        }
    }

    /// <summary>
    /// 즉시 삭제 (Play 모드 Start에서 사용 — 새 테이블 생성 전에 깨끗한 상태 보장)
    /// </summary>
    private void ClearTableImmediate()
    {
        spawnedElements.Clear();
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
        Debug.Log($"[PeriodicTable3D] ClearTableImmediate 완료 — childCount: {transform.childCount}");
    }
}
