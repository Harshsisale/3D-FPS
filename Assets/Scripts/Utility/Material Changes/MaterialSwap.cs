using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialSwap : MonoBehaviour {

    public List<Material> opaqueMaterials;
    public List<Material> transparentMaterials;

    void Start()
    {
        // Everything starts on the opaque material because it catches shadows
        opaqueMaterials = new List<Material>(this.gameObject.GetComponent<Renderer>().materials);
        GetTransparentMaterial();
    }

    void GetTransparentMaterial()
    {
        foreach (Material material in opaqueMaterials)
        {
            string materialName = material.name.Replace(" (Instance)", "");
            transparentMaterials.Add(Resources.Load<Material>("Materials/Transparent/" + materialName));
        
        }
        for (int i = 0; i < transparentMaterials.Count; i++)
        {
            if (transparentMaterials[i] == null)
            {
                transparentMaterials[i] = Resources.Load<Material>("Materials/Transparent/Default Transparent");
            }
        }
    }

    public void GoToTransparent()
    {
        if (transparentMaterials.Contains(null))
        {
            return;
        }
        Renderer renderer = this.gameObject.GetComponent<Renderer>();
        for (int i = 0; i < opaqueMaterials.Count; i++)
        {
            renderer.materials = transparentMaterials.ToArray();
        }
        SkinnedMeshRenderer skinRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();
        if (skinRenderer != null)
        {
            for (int i = 0; i < opaqueMaterials.Count; i++)
            {
                skinRenderer.materials = transparentMaterials.ToArray();
            }
        }
    }

    public void GoToOpaque()
    {
        if (transparentMaterials.Contains(null))
        {
            return;
        }
        Renderer renderer = this.gameObject.GetComponent<Renderer>();
        for (int i = 0; i < opaqueMaterials.Count; i++)
        {
            renderer.materials = opaqueMaterials.ToArray();
        }
        SkinnedMeshRenderer skinRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();
        if (skinRenderer != null)
        {
            for (int i = 0; i < opaqueMaterials.Count; i++)
            {
                skinRenderer.materials = opaqueMaterials.ToArray();
            }
        }
    }
}
