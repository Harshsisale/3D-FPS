using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerTransparency : MonoBehaviour {

    float aplhaValueWhenTransparent = 0.05f;
	void OnTriggerEnter(Collider triggerer)
    {
        // Get children material swaps if they exist
        List<MaterialSwap> materialSwaps = new List<MaterialSwap>(GetComponentsInChildren<MaterialSwap>());
        if (triggerer.GetComponent<MaterialSwap>() != null && triggerer.GetComponent<Renderer>() != null)
        {
            MaterialSwap materialSwap = triggerer.GetComponent<MaterialSwap>();
            materialSwap.GoToTransparent();

            Renderer renderer = materialSwap.GetComponent<Renderer>();
            Color materialColor = renderer.material.color;
            renderer.material.color = new Color(materialColor.r, materialColor.g, materialColor.b, aplhaValueWhenTransparent);
        }
        else if (materialSwaps.Count > 0)
        {
            foreach (MaterialSwap materialSwap in materialSwaps)
            {
                Renderer renderer = materialSwap.GetComponent<Renderer>();
                if (renderer != null)
                {
                    materialSwap.GoToTransparent();
                    Color materialColor = renderer.material.color;
                    renderer.material.color = new Color(materialColor.r, materialColor.g, materialColor.b, aplhaValueWhenTransparent);
                }
            }
        }
    }

    void OnTriggerStay(Collider triggerer)
    {
        // Get children material swaps if they exist
        List<MaterialSwap> materialSwaps = new List<MaterialSwap>(GetComponentsInChildren<MaterialSwap>());
        if (triggerer.GetComponent<MaterialSwap>() != null && triggerer.GetComponent<Renderer>() != null)
        {
            MaterialSwap materialSwap = triggerer.GetComponent<MaterialSwap>();
            materialSwap.GoToTransparent();

            Renderer renderer = materialSwap.GetComponent<Renderer>();
            Color materialColor = renderer.material.color;
            renderer.material.color = new Color(materialColor.r, materialColor.g, materialColor.b, aplhaValueWhenTransparent);
        }
        else if (materialSwaps.Count > 0)
        {
            foreach (MaterialSwap materialSwap in materialSwaps)
            {
                Renderer renderer = materialSwap.GetComponent<Renderer>();
                if (renderer != null)
                {
                    materialSwap.GoToTransparent();
                    Color materialColor = renderer.material.color;
                    renderer.material.color = new Color(materialColor.r, materialColor.g, materialColor.b, aplhaValueWhenTransparent);
                }
            }
        }
    }

    void OnTriggerExit(Collider triggerer)
    {
        // Get children material swaps if they exist
        List<MaterialSwap> materialSwaps = new List<MaterialSwap>(GetComponentsInChildren<MaterialSwap>());
        if (triggerer.GetComponent<MaterialSwap>() != null && triggerer.GetComponent<Renderer>() != null)
        {
            MaterialSwap materialSwap = triggerer.GetComponent<MaterialSwap>();
            materialSwap.GoToOpaque();

            Renderer renderer = materialSwap.GetComponent<Renderer>();
            Color materialColor = renderer.material.color;
            renderer.material.color = new Color(materialColor.r, materialColor.g, materialColor.b, 1);
        }
            else if (materialSwaps.Count > 0)
            {
                foreach (MaterialSwap materialSwap in materialSwaps)
                {
                    Renderer renderer = materialSwap.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        materialSwap.GoToOpaque();
                        Color materialColor = renderer.material.color;
                        renderer.material.color = new Color(materialColor.r, materialColor.g, materialColor.b, 1);
                    }
                }
            }
    }
}
