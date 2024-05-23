using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;

public class MeshColliderAndXRGrabAdder : MonoBehaviour
{
    public List<GameObject> gameObjectsWithMeshes; // Assign this list with your game objects

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
