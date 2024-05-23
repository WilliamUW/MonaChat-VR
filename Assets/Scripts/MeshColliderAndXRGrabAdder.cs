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

    public void AddCollidersAndInteractablesNotWorking()
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

            // Create a list to store all the child meshes
            List<Mesh> childMeshes = new List<Mesh>();

            foreach (var meshFilter in meshFilters)
            {
                if (meshFilter == null || meshFilter.sharedMesh == null) continue;

                // Add MeshCollider with convex enabled to each child
                var meshCollider = meshFilter.gameObject.AddComponent<MeshCollider>();
                meshCollider.sharedMesh = meshFilter.sharedMesh;
                meshCollider.convex = true;

                // Add the mesh to the list
                childMeshes.Add(meshFilter.sharedMesh);
            }

            // Combine all child meshes into one mesh
            if (childMeshes.Count > 0)
            {
                Mesh combinedMesh = new Mesh();
                CombineInstance[] combine = new CombineInstance[childMeshes.Count];

                for (int i = 0; i < childMeshes.Count; i++)
                {
                    combine[i].mesh = childMeshes[i];
                    combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
                }

                combinedMesh.CombineMeshes(combine);

                // Add MeshCollider to the parent object
                var parentMeshCollider = parentObject.AddComponent<MeshCollider>();
                parentMeshCollider.sharedMesh = combinedMesh;
                parentMeshCollider.convex = true;
            }
        }
    }

    // Call this method when your game objects are ready, such as after loading .glb files
    public void OnMeshesLoaded()
    {
        AddCollidersAndInteractables();
    }
}
