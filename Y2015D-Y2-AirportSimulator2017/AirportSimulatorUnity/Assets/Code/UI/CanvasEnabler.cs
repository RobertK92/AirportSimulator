using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using AI;

public class CanvasEnabler : MonoBehaviour
{
    public Canvas canvas;
    public Text liquidText;
    public Text foodText;
    public Text moneyText;
    public Image boardingPassImg;

    private void Update()
    {
        canvas.gameObject.SetActive(false);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if(Physics.Raycast(ray, out hit))
        {
            if(hit.collider.gameObject.layer == UnityConstants.Layers.Agent)
            {
                transform.SetParent(hit.transform, false);
                transform.localPosition = Vector3.zero;
                Inventory inventory = hit.collider.gameObject.GetComponent<Inventory>();
                SyncInventoryUI(inventory);
                canvas.gameObject.SetActive(true);
            }
        }
    }

    public void SyncInventoryUI(Inventory inventory)
    {
        if (moneyText != null)
            moneyText.text = inventory.MoneyString;
        if (liquidText != null)
            liquidText.text = inventory.Liquid.ToString("F2") + "L";
        if (foodText != null)
            foodText.text = inventory.Food.ToString("F2") + "KG";
        if (boardingPassImg != null)
        {
            if (inventory.BoardingPass)
                boardingPassImg.color = Color.white;
            else
                boardingPassImg.color = new Color(1.0f, 1.0f, 1.0f, 0.2f);
        }
    }
}
