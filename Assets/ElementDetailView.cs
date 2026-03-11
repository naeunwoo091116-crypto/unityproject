using UnityEngine;
using TMPro;
using System.Collections;

public class ElementDetailView : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshPro symbolText;
    public TextMeshPro nameText;
    public TextMeshPro summaryText;
    public TextMeshPro statsText;
    public MeshRenderer backgroundRenderer;
    public Transform atomContainer;

    [Header("Animation Settings")]
    public float scaleDuration = 0.5f;

    private GameObject spawnedAtom;

    public void Setup(ElementData data, Color categoryColor, GameObject atomPrefab)
    {
        // 텍스트 설정
        if (symbolText) symbolText.text = data.symbol;
        if (nameText) nameText.text = data.name;
        if (summaryText) summaryText.text = data.summary;
        
        if (statsText)
        {
            statsText.text = $"Atomic Number: {data.number}\n" +
                             $"Atomic Mass: {data.atomic_mass}\n" +
                             $"Melting Point: {data.melt}K\n" +
                             $"Boiling Point: {data.boil}K\n" +
                             $"Category: {data.category}";
        }

        // 배경 색상 (카테고리 색상 반영)
        if (backgroundRenderer)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.SetFloat("_Surface", 1); // Transparent
            mat.SetInt("_ZWrite", 0);
            mat.renderQueue = 3000;
            mat.color = new Color(categoryColor.r, categoryColor.g, categoryColor.b, 0.4f);
            backgroundRenderer.material = mat;
        }

        // 원자 모델 생성
        if (atomPrefab && atomContainer)
        {
            spawnedAtom = Instantiate(atomPrefab, atomContainer);
            spawnedAtom.transform.localPosition = Vector3.zero;
            spawnedAtom.transform.localScale = Vector3.one * 0.5f; // 크기 조정
            
            // 색상 적용 (프리팹 구조에 따라 다를 수 있음)
            var r = spawnedAtom.GetComponentInChildren<Renderer>();
            if (r) r.material.color = categoryColor;
        }

        // 나타나는 애니메이션
        transform.localScale = Vector3.zero;
        StartCoroutine(AnimateScale(Vector3.one));
    }

    private void Update()
    {
        // 원자 모델 회전
        if (spawnedAtom)
        {
            spawnedAtom.transform.Rotate(Vector3.up, 30f * Time.deltaTime);
        }

        // 항상 인터랙터를 바라보게 설정 (빌보드)
        if (Camera.main != null)
        {
            transform.LookAt(Camera.main.transform);
            transform.Rotate(0, 180, 0); // 반대 방향 회전
        }
    }

    public void Close()
    {
        StartCoroutine(AnimateScale(Vector3.zero, () => Destroy(gameObject)));
    }

    private IEnumerator AnimateScale(Vector3 targetScale, System.Action onComplete = null)
    {
        float elapsed = 0;
        Vector3 initialScale = transform.localScale;
        while (elapsed < scaleDuration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(initialScale, targetScale, elapsed / scaleDuration);
            yield return null;
        }
        transform.localScale = targetScale;
        onComplete?.Invoke();
    }
}
