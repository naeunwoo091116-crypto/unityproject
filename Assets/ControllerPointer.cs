using UnityEngine;

/// <summary>
/// OVRCameraRig 기반 컨트롤러 포인터
/// RightHandAnchor에 부착하여 오른쪽 트리거로 주기율표 원소 클릭 → Atom 스폰
/// </summary>
public class ControllerPointer : MonoBehaviour
{
    [Header("포인터 설정")]
    [Tooltip("레이 최대 거리 (미터)")]
    public float rayDistance = 10f;

    [Tooltip("포인터 레이 색상")]
    public Color rayColor = new Color(0.3f, 0.8f, 1f, 0.8f);

    [Tooltip("원소에 닿았을 때 레이 색상")]
    public Color hitColor = new Color(1f, 0.5f, 0.2f, 1f);

    private LineRenderer lineRenderer;
    private Element3D lastHitElement;

    void Start()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = 0.003f;
        lineRenderer.endWidth = 0.001f;
        lineRenderer.useWorldSpace = true;

        lineRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        lineRenderer.material.color = rayColor;
    }

    void Update()
    {
        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;
        Vector3 endPoint = origin + direction * rayDistance;

        RaycastHit hit;
        bool isHitting = Physics.Raycast(origin, direction, out hit, rayDistance);

        // 레이 시각화
        if (lineRenderer != null)
        {
            lineRenderer.SetPosition(0, origin);
            lineRenderer.SetPosition(1, isHitting ? hit.point : endPoint);
            lineRenderer.material.color = isHitting ? hitColor : rayColor;
        }

        // Hover 감지: 레이가 닿는 Element3D 추적
        Element3D hitElement = null;
        if (isHitting)
        {
            hitElement = hit.collider.GetComponentInParent<Element3D>();
            if (hitElement == null)
                hitElement = hit.collider.GetComponentInChildren<Element3D>();
        }

        // 이전 원소 → 새 원소 전환 시 Highlight/Dim 처리
        if (hitElement != lastHitElement)
        {
            if (lastHitElement != null) lastHitElement.Dim();
            if (hitElement != null) hitElement.Highlight();
            lastHitElement = hitElement;
        }

        // 오른쪽 트리거 클릭 시 원자 소환 (컨트롤러 앞 0.4m에 소환)
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch))
        {
            if (hitElement != null)
            {
                // 컨트롤러 앞 0.4m — 테이블과 사용자 사이 공간에 원자 배치
                Vector3 spawnPos = transform.position + transform.forward * 0.4f;
                hitElement.TriggerSpawn(spawnPos);
            }
        }
    }
}
