using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;
using GLTFast;

public class MeshColliderAndXRGrabAdder : MonoBehaviour
{
    public List<GameObject> gameObjectsWithMeshes; // Assign this list with your game objects
    private List<string> gltfUrls = new List<string>
    {
        "https://raw.githubusercontent.com/KhronosGroup/glTF-Sample-Models/master/2.0/Duck/glTF/Duck.gltf",
        "https://raw.githubusercontent.com/KhronosGroup/glTF-Sample-Models/main/2.0/ToyCar/glTF/ToyCar.gltf",
        "https://raw.githubusercontent.com/KhronosGroup/glTF-Sample-Models/main/2.0/ABeautifulGame/glTF/ABeautifulGame.gltf",
        "https://arweave.net/swKvgRBuTamdbuKgxmw3aiKqBMOJRHp_Hw8Qh9zGEF4",
        "https://content.mona.gallery/y6l0ryaq-pjyl-fdj9-tlm1-xavgcotw.glb"
    };

    private void Start()
    {
        // Load GLTF assets and add components
        // LoadGltfAssetsAndAddComponents();
    }

    public void LoadGltfAssetsAndAddComponents(List<string> gltfUrls)
    {
        for (int i = 0; i < gameObjectsWithMeshes.Count; i++)
        {
            var gameObject = gameObjectsWithMeshes[i];
            string gltfUrl = gltfUrls[i];

            if (gameObject != null && !string.IsNullOrEmpty(gltfUrl))
            {
                var gltf = gameObject.AddComponent<GLTFast.GltfAsset>();
                gltf.Url = gltfUrl;
            }
        }
    }

    public void AddCollidersAndInteractables()
    {
        foreach (var parentObject in gameObjectsWithMeshes)
        {
            // Ensure the parent object is not null
            if (parentObject == null) continue;

            // Add XRGrabInteractable to the parent object
            if (parentObject.GetComponent<XRGrabInteractable>() == null)
            {
                parentObject.AddComponent<XRGrabInteractable>();
            }

            // Find all MeshFilter components in the children
            var meshFilters = parentObject.GetComponentsInChildren<MeshFilter>();
            foreach (var meshFilter in meshFilters)
            {
                if (meshFilter == null || meshFilter.sharedMesh == null) continue;

                // Add MeshCollider with convex enabled
                var meshCollider = meshFilter.gameObject.AddComponent<MeshCollider>();
                meshCollider.sharedMesh = meshFilter.sharedMesh;
                meshCollider.convex = true;
                meshFilter.gameObject.AddComponent<XRGrabInteractable>();
            }
        }
    }

    // Call this method when your game objects are ready, such as after loading .glb files
    public void OnMeshesLoaded()
    {
        AddCollidersAndInteractables();
    }
}
