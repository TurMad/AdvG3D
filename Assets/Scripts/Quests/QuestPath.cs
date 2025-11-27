using UnityEngine;
using UnityEngine.Splines;


public class QuestPath : MonoBehaviour
{
    public string pathId; 
    [SerializeField] private SplineAnimate splineAnimate;
    
    public SplineAnimate SplineAnimate => splineAnimate;
}
