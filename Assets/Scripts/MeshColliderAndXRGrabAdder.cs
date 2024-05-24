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

    List<CollectibleDto> collectibles;

    private void Start()
    {
    }

    public void LoadGltfAssetsAndAddComponents(List<CollectibleDto> collectibles)
    {
        this.collectibles = collectibles;
        for (int i = 0; i < collectibles.Count; i++)
        {
            var gameObject = gameObjectsWithMeshes[i];
            var collectible = collectibles[i];
            var url = collectible.Versions[collectible.ActiveVersion].Asset;

            if (gameObject != null && !string.IsNullOrEmpty(url))
            {
                var gltf = gameObject.AddComponent<GltfAsset>();
                gltf.Url = url;
                gameObject.name = collectible.Title;
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
            var grabInteractable = parentObject.GetComponent<XRGrabInteractable>();
            if (grabInteractable == null)
            {
                grabInteractable = parentObject.AddComponent<XRGrabInteractable>();
            }

            // Add OnSelect listener to log the name when grabbed
            grabInteractable.onSelectEntered.AddListener((XRBaseInteractor interactor) =>
            {
                Debug.Log($"Grabbed object: {parentObject.name}");
            });

            // Find all MeshFilter components in the children
            var meshFilters = parentObject.GetComponentsInChildren<MeshFilter>();
            foreach (var meshFilter in meshFilters)
            {
                if (meshFilter == null || meshFilter.sharedMesh == null) continue;

                // Add MeshCollider with convex enabled
                var meshCollider = meshFilter.gameObject.AddComponent<MeshCollider>();
                meshCollider.sharedMesh = meshFilter.sharedMesh;
                meshCollider.convex = true;
                
                // Ensure each child mesh also has an XRGrabInteractable
                var childGrabInteractable = meshFilter.gameObject.GetComponent<XRGrabInteractable>();
                if (childGrabInteractable == null)
                {
                    childGrabInteractable = meshFilter.gameObject.AddComponent<XRGrabInteractable>();
                }

                // Add OnSelect listener to log the name when grabbed
                childGrabInteractable.onSelectEntered.AddListener((XRBaseInteractor interactor) =>
                {
                    Debug.Log($"Grabbed object: {meshFilter.gameObject.name}");
                });
            }
        }
    }
}
