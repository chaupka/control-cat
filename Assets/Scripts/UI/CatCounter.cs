using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CatCounter : MonoBehaviour
{
    private TextMeshProUGUI textMesh;

    // Start is called before the first frame update
    void Start()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        textMesh.text = "Cats: " + GameStateController.singleton.aiDirector.aliens.Count;
    }
}
