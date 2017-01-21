using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ModelRandomizer : MonoBehaviour 
{
    public List<GameObject> models = new List<GameObject>();
    private void Awake()
    {
        if (models.Count == 0)
            return;
        int rand = Random.Range(0, models.Count);
        models.ForEach(x => x.SetActive(false));
        models[rand].SetActive(true);
    }
}
