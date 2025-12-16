using UnityEngine;
using Unity.Cinemachine;
public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    [SerializeField] private MapCameraInputController mapInput;
    [Header("Room VCams")]
    [SerializeField] private CinemachineCamera room0;
    [SerializeField] private CinemachineCamera room90;
    [SerializeField] private CinemachineCamera room180;

    [Header("Map VCam")]
    [SerializeField] private CinemachineCamera mapTop;

    [Header("Priorities")]
    [SerializeField] private int activePriority = 20;
    [SerializeField] private int inactivePriority = 0;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // по умолчанию первый вид комнаты
        ShowRoomView0();
    }

    public void ShowRoomView0()  => SetActive(room0);
    public void ShowRoomView90() => SetActive(room90);
    public void ShowRoomView180()=> SetActive(room180);
    public void ShowMapTop()     => SetActive(mapTop);

    private void SetActive(CinemachineCamera target)
    {
        SetPriority(room0, target);
        SetPriority(room90, target);
        SetPriority(room180, target);
        SetPriority(mapTop, target);
        
        if (mapInput != null)
            mapInput.enabled = (target == mapTop);
    }

    private void SetPriority(CinemachineCamera vcam, CinemachineCamera target)
    {
        if (vcam == null) return;
        vcam.Priority = (vcam == target) ? activePriority : inactivePriority;
    }
}