using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;
using GLTFast;
using Monaverse.Api.Modules.Collectibles.Dtos; // Add this using directive

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
    }

    public void LoadGltfAssetsAndAddComponents(List<CollectibleDto> collectibles)
    {
        for (int i = 0; i < collectibles.Count; i++)
        {
            var gameObject = gameObjectsWithMeshes[i];
            var collectible = collectibles[i];
            var url = collectible.Versions[collectible.ActiveVersion].Asset;

            if (gameObject != null && !string.IsNullOrEmpty(url))
            {
                var gltf = gameObject.AddComponent<GltfAsset>();
                gltf.Url = url;
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
}
