using UnityEngine;  
using System.Collections;  
using UnityEngine.EventSystems;  
using UnityEngine.UI;

public class MenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler 
{
    public Text theText;
    public float hoverAlphaVal = 0.5f;

    Image image;

    void Start()
    {
        image = GetComponent<Image>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
//        theText.color = Color.red; 

        Color imgColor = image.color;
        imgColor.a = hoverAlphaVal;
        image.color = imgColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
//        theText.color = Color.white; 

        Color imgColor = image.color;
        imgColor.a = 0;
        image.color = imgColor;
    }
}