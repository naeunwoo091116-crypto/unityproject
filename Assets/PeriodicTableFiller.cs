using UnityEngine;
using UnityEngine.UI;
using TMPro;
// [중요] VR 상호작용을 위해 이 네임스페이스가 꼭 필요합니다.
using UnityEngine.XR.Interaction.Toolkit;

[ExecuteAlways]
public class PeriodicTableFiller : MonoBehaviour
{
    [Header("원본 이미지 (자식의 Image)")]
    public GameObject templateImage;

    [Header("원자 프리팹")]
    public GameObject atomPrefab;

    [Header("생성할 원소 개수")]
    [Range(1, 36)]
    public int elementCount = 36;

    private static readonly string[] symbols = {
        "H", "He", "Li", "Be", "B", "C", "N", "O", "F", "Ne",
        "Na", "Mg", "Al", "Si", "P", "S", "Cl", "Ar",
        "K", "Ca", "Sc", "Ti", "V", "Cr", "Mn", "Fe", "Co", "Ni", "Cu", "Zn", "Ga", "Ge", "As", "Se", "Br", "Kr"
    };

    private static readonly Color[] colors = {
        new Color(0.4f, 0.6f, 1f), new Color(0.9f, 0.5f, 0.9f),
        new Color(1f, 0.4f, 0.4f), new Color(1f, 0.6f, 0.3f), new Color(0.4f, 0.8f, 0.8f), new Color(0.4f, 0.6f, 1f),
        new Color(0.4f, 0.6f, 1f), new Color(0.4f, 0.6f, 1f), new Color(0.8f, 0.4f, 1f), new Color(0.9f, 0.5f, 0.9f),
        new Color(1f, 0.4f, 0.4f), new Color(1f, 0.6f, 0.3f), new Color(0.6f, 0.8f, 0.6f), new Color(0.4f, 0.8f, 0.8f),
        new Color(0.4f, 0.6f, 1f), new Color(0.4f, 0.6f, 1f), new Color(0.8f, 0.4f, 1f), new Color(0.9f, 0.5f, 0.9f),
        new Color(1f, 0.4f, 0.4f), new Color(1f, 0.6f, 0.3f), new Color(1f, 0.8f, 0.4f), new Color(1f, 0.8f, 0.4f),
        new Color(1f, 0.8f, 0.4f), new Color(1f, 0.8f, 0.4f), new Color(1f, 0.8f, 0.4f), new Color(1f, 0.8f, 0.4f),
        new Color(1f, 0.8f, 0.4f), new Color(1f, 0.8f, 0.4f), new Color(1f, 0.8f, 0.4f), new Color(1f, 0.8f, 0.4f),
        new Color(0.6f, 0.8f, 0.6f), new Color(0.4f, 0.8f, 0.8f), new Color(0.4f, 0.8f, 0.8f), new Color(0.4f, 0.6f, 1f),
        new Color(0.8f, 0.4f, 1f), new Color(0.9f, 0.5f, 0.9f)
    };

    void OnEnable() { GenerateTable(); }
    void OnValidate()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.delayCall += () => { if (this != null) GenerateTable(); };
#endif
    }

    [ContextMenu("Generate Periodic Table")]
    public void GenerateTable()
    {
        if (templateImage == null) templateImage = transform.Find("Image")?.gameObject;
        if (templateImage == null) return;

        ClearElements();

        int count = Mathf.Min(elementCount, symbols.Length);
        for (int i = 0; i < count; i++)
        {
            CreateElement(i, symbols[i], colors[i]);
        }
        templateImage.SetActive(false);
    }

    private void CreateElement(int index, string symbol, Color color)
    {
        GameObject element = Instantiate(templateImage, transform);
        element.name = $"Element_{symbol}";
        element.SetActive(true);

        // 1. 이미지 색상
        var image = element.GetComponent<Image>();
        if (image != null) image.color = color;

        // 2. AtomSpawner 설정
        var spawner = element.GetComponent<AtomSpawner>();
        if (spawner == null) spawner = element.AddComponent<AtomSpawner>();
        spawner.elementSymbol = symbol;
        spawner.elementColor = color;
        spawner.atomPrefab = atomPrefab;

        // 3. [VR 기능] BoxCollider 추가 (손이 닿으려면 필요)
        var col = element.GetComponent<BoxCollider>();
        if (col == null) col = element.AddComponent<BoxCollider>();
        
        // 콜라이더 크기를 UI 크기에 맞게 자동 조절
        RectTransform rt = element.GetComponent<RectTransform>();
        // Z축(두께)을 0.5 정도로 줘야 손이 뚫고 지나가지 않고 잘 인식됩니다.
        col.size = new Vector3(rt.rect.width, rt.rect.height, 0.5f); 

        // 4. [VR 기능] XR Simple Interactable 추가 (잡기 감지용)
        // XRGrabInteractable은 물체를 들고 이동시키지만, 
        // XRSimpleInteractable은 제자리에 있고 클릭/잡기 이벤트만 받습니다.
        var interactable = element.GetComponent<XRSimpleInteractable>();
        if (interactable == null) interactable = element.AddComponent<XRSimpleInteractable>();

        // 이벤트 연결: 잡았을 때(SelectEntered) -> 원자 생성 함수 실행
        interactable.selectEntered.RemoveAllListeners(); // 중복 방지
        interactable.selectEntered.AddListener((args) => {
            spawner.SpawnAtom();
        });

        // 5. 텍스트 설정
        var text = element.GetComponentInChildren<TextMeshProUGUI>();
        if (text == null)
        {
            GameObject textObj = new GameObject("SymbolText");
            textObj.transform.SetParent(element.transform, false);
            text = textObj.AddComponent<TextMeshProUGUI>();
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            text.enableAutoSizing = true;
            text.fontSizeMin = 10;
            text.fontSizeMax = 70;
            text.fontStyle = FontStyles.Bold;
        }
        text.text = symbol;
    }

    private void ClearElements()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i);
            if (child.gameObject != templateImage)
            {
                if (Application.isPlaying) Destroy(child.gameObject);
                else DestroyImmediate(child.gameObject);
            }
        }
    }
}