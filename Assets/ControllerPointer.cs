using UnityEngine;

/// <summary>
/// OVRCameraRig 기반 컨트롤러 포인터
/// RightHandAnchor에 부착하여 오른쪽 트리거로:
/// - 주기율표 원소 클릭 → Atom 스폰
/// - 소환된 Atom에 포인터 조준 + 트리거 홀드 → Atom이 포인터를 따라 이동 (포인터 그랩)
/// </summary>
public class ControllerPointer : MonoBehaviour
{
    [Header("포인터 설정")]
    [Tooltip("레이 최대 거리 (미터)")]
    public float rayDistance = 10f;

    [Tooltip("기본 레이 색상")]
    public Color rayColor = new Color(0.3f, 0.8f, 1f, 0.8f);

    [Tooltip("원소/원자에 닿았을 때 레이 색상")]
    public Color hitColor = new Color(1f, 0.5f, 0.2f, 1f);

    [Tooltip("원자를 잡고 이동 중일 때 레이 색상")]
    public Color grabColor = new Color(0.2f, 1f, 0.4f, 1f);

    private LineRenderer lineRenderer;
    private Element3D lastHitElement;

    // 포인터 그랩 상태
    private GameObject grabbedAtom = null;
    private float grabDistance = 0f;

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
        Vector3 origin    = transform.position;
        Vector3 direction = transform.forward;

        // ── 그랩 중: 원자가 포인터 방향을 따라다님 ──────────────────────
        if (grabbedAtom != null)
        {
            // 트리거를 놓으면 원자 드롭 (현재 위치에 그대로 남음)
            if (OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch))
            {
                grabbedAtom = null;
                lineRenderer.material.color = rayColor;
                return;
            }

            // 원자를 포인터 방향 grabDistance 거리에 고정
            grabbedAtom.transform.position = origin + direction * grabDistance;

            // 레이 시각화 (컨트롤러 → 원자, 초록색)
            lineRenderer.SetPosition(0, origin);
            lineRenderer.SetPosition(1, grabbedAtom.transform.position);
            lineRenderer.material.color = grabColor;
            return;
        }

        // ── 일반 레이캐스트 ──────────────────────────────────────────────
        RaycastHit hit;
        bool isHitting = Physics.Raycast(origin, direction, out hit, rayDistance);

        // 레이 시각화
        lineRenderer.SetPosition(0, origin);
        lineRenderer.SetPosition(1, isHitting ? hit.point : origin + direction * rayDistance);
        lineRenderer.material.color = isHitting ? hitColor : rayColor;

        // Hover 감지: 레이가 닿는 오브젝트 종류 파악
        Element3D  hitElement = null;
        AtomConnector hitAtomConnector = null;

        if (isHitting)
        {
            // 원소 박스인지 확인 (Element3D)
            hitElement = hit.collider.GetComponentInParent<Element3D>();
            if (hitElement == null)
                hitElement = hit.collider.GetComponentInChildren<Element3D>();

            // 소환된 원자인지 확인 (Element3D가 아닌 경우에만)
            if (hitElement == null)
                hitAtomConnector = hit.collider.GetComponent<AtomConnector>();
        }

        // 원소 Highlight / Dim 처리 (포인터가 원소 위를 지날 때)
        if (hitElement != lastHitElement)
        {
            if (lastHitElement != null) lastHitElement.Dim();
            if (hitElement != null)     hitElement.Highlight();
            lastHitElement = hitElement;
        }

        // 오른쪽 트리거 누름
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch))
        {
            if (hitAtomConnector != null)
            {
                // ① 원자 그랩 시작 — 현재 거리를 기억
                grabbedAtom  = hit.collider.gameObject;
                grabDistance = hit.distance;

                // 원소 하이라이트 해제
                if (lastHitElement != null) { lastHitElement.Dim(); lastHitElement = null; }
            }
            else if (hitElement != null)
            {
                // ② 원소 클릭 → 컨트롤러 앞 0.4m 위치에 원자 소환
                Vector3 spawnPos = origin + direction * 0.4f;
                hitElement.TriggerSpawn(spawnPos);
            }
        }
    }
}
