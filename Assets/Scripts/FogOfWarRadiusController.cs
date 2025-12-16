using UnityEngine;

public class FogOfWarRadiusController : MonoBehaviour
{
    [SerializeField] private Renderer fogRenderer;
    [SerializeField] private Transform guildCenter;

    [Header("Radius from Reputation")]
    [SerializeField] private float baseRadius = 20f;
    [SerializeField] private float radiusPerReputation = 0.05f; // 50 репы = +2.5 радиуса

    [Header("Optional")]
    [SerializeField] private float yLock = 0f; // если хочешь фиксировать Y центра

    private Material _mat;
    private static readonly int CenterId = Shader.PropertyToID("_Center");
    private static readonly int RadiusId = Shader.PropertyToID("_Radius");

    private void Awake()
    {
        if (fogRenderer == null) fogRenderer = GetComponent<Renderer>();
        if (fogRenderer != null) _mat = fogRenderer.material; // уникальный материал для инстанса
    }

    private void Update()
    {
        if (_mat == null || guildCenter == null || GameRepository.Data == null) return;

        Vector3 center = guildCenter.position;
        center.y = yLock; // не обязательно

        float rep = GameRepository.Data.reputation;
        float radius = baseRadius + rep * radiusPerReputation;

        _mat.SetVector(CenterId, new Vector4(center.x, center.y, center.z, 0));
        _mat.SetFloat(RadiusId, radius);
    }
}