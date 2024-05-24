using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using GLTFast;
using Monaverse.Api.Modules.Collectibles.Dtos;
using TMPro; // Required for TextMeshPro interaction

public class MeshColliderAndXRGrabAdder : MonoBehaviour
{
    public List<GameObject> gameObjectsWithMeshes; // Assign this list with your game objects
    public TMP_Dropdown collectiblesDropdown; // Reference to the TMP Dropdown

    private List<CollectibleDto> collectibles; // Store the list of collectibles

    private Dictionary<string, (Vector3 position, float scale)> positionScaleMap = new Dictionary<string, (Vector3 position, float scale)>()
    {
        { "Statue of Liberty", (new Vector3(13, -16, 3), 0.5f) },
        { "XB-21", (new Vector3(-2, 8f, -20), 0.2f) },
        { "The Thinker", (new Vector3(-5, 1f, 0), 3f) },
        { "Car", (new Vector3(0, 1f, 4), 1f) },
        { "Mona Lisa", (new Vector3(5, 1.5f, 0), 1f) }
    };


    public void LoadGltfAssetsAndAddComponents(List<CollectibleDto> collectibles)
    {
        // Store the collectibles list for later use
        this.collectibles = collectibles;

        // Debug: Ensure collectibles list is not null and contains expected items
        if (collectibles == null)
        {
            Debug.LogError("Collectibles list is null");
            return;
        }

        Debug.Log($"Collectibles count: {collectibles.Count}");

        // Clear the dropdown options first
        collectiblesDropdown.ClearOptions();

        List<string> dropdownOptions = new List<string>();

        for (int i = 0; i < collectibles.Count; i++)
        {
            // Debug: Ensure gameObjectsWithMeshes list is not null and contains expected items
            if (gameObjectsWithMeshes == null || gameObjectsWithMeshes.Count <= i)
            {
                Debug.LogError("Game objects with meshes list is null or does not contain enough items");
                return;
            }

            var gameObject = gameObjectsWithMeshes[i];
            var collectible = collectibles[i];

            // Add title to the dropdown options
            dropdownOptions.Add(collectible.Title);

            // Debug: Print collectible details
            Debug.Log($"Processing collectible: {collectible.Title}");

            if (collectible.Versions == null || collectible.Versions.Count <= collectible.ActiveVersion)
            {
                Debug.LogError($"Collectible {collectible.Title} has invalid version data");
                continue;
            }

            var url = collectible.Versions[collectible.ActiveVersion].Asset;

            if (gameObject != null && !string.IsNullOrEmpty(url))
            {
                Debug.Log($"{collectible.Title} Loading GLTF asset: {url}");
                var gltf = gameObject.AddComponent<GltfAsset>();
                gltf.Url = url;
                gameObject.name = collectible.Title;

                // Set initial position and scale based on the title
                if (positionScaleMap.TryGetValue(collectible.Title, out var positionScale))
                {
                    gameObject.transform.position = positionScale.position;
                    gameObject.transform.localScale = Vector3.one * positionScale.scale;
                }
                else
                {
                    // Default position and scale
                    gameObject.transform.position = Vector3.zero;
                    gameObject.transform.localScale = Vector3.one;
                }
            }
        }

        // Add options to the dropdown
        collectiblesDropdown.AddOptions(dropdownOptions);
        collectiblesDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
    }

    private void OnDropdownValueChanged(int index)
    {
        Debug.Log($"Dropdown value changed: {index}");
        if (collectibles != null && index >= 0 && index < collectibles.Count)
        {
            var selectedCollectible = collectibles[index];
            InitializeGemini(selectedCollectible.Title, selectedCollectible.Description);
        }
    }

    public void InitializeGemini(string title, string description)
    {
        // Implement your logic to initialize Gemini with the selected asset's title and description
        Debug.Log($"Initializing Gemini with Title: {title} and Description: {description}");
    }

    public void AddCollidersAndInteractables()
    {
        foreach (var parentObject in gameObjectsWithMeshes)
        {
            if (parentObject == null) continue;

            var grabInteractable = parentObject.GetComponent<XRGrabInteractable>();
            if (grabInteractable == null)
            {
                grabInteractable = parentObject.AddComponent<XRGrabInteractable>();
            }

            grabInteractable.onSelectEntered.AddListener((XRBaseInteractor interactor) =>
            {
                Debug.Log($"Grabbed object: {parentObject.name}");
            });

            var meshFilters = parentObject.GetComponentsInChildren<MeshFilter>();
            foreach (var meshFilter in meshFilters)
            {
                if (meshFilter == null || meshFilter.sharedMesh == null) continue;

                var meshCollider = meshFilter.gameObject.AddComponent<MeshCollider>();
                meshCollider.sharedMesh = meshFilter.sharedMesh;
                meshCollider.convex = true;

                var childGrabInteractable = meshFilter.gameObject.GetComponent<XRGrabInteractable>();
                if (childGrabInteractable == null)
                {
                    childGrabInteractable = meshFilter.gameObject.AddComponent<XRGrabInteractable>();
                }

                childGrabInteractable.onSelectEntered.AddListener((XRBaseInteractor interactor) =>
                {
                    Debug.Log($"Grabbed object: {meshFilter.gameObject.name}");
                });
            }
        }
    }
}
