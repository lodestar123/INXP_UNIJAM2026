using UnityEngine;
using UnityEngine.EventSystems;

public class StartAalarm : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData e)
    {
        gameObject.SetActive(false);
    }
}
