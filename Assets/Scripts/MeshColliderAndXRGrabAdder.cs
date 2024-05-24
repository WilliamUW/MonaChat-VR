using System.Collections;
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
    public Transform playerTransform; // Reference to the player's transform
    public Gemini gemini;

    private List<CollectibleDto> collectibles; // Store the list of collectibles
    private GameObject currentClosestObject; // Track the current closest object

    private Dictionary<string, (Vector3 position, float scale)> positionScaleMap = new Dictionary<string, (Vector3 position, float scale)>()
    {
        { "Statue of Liberty", (new Vector3(13, -16, 3), 0.5f) },
        { "XB-21", (new Vector3(-2, 8f, -20), 0.2f) },
        { "The Thinker", (new Vector3(-5, 1f, 0), 3f) },
        { "Car", (new Vector3(0, 1f, 4), 1f) },
        { "Mona Lisa", (new Vector3(5, 1.5f, 0), 1f) }
    };

    private void Start()
    {
    }

    private void Update()
    {
        CheckClosestObject();
    }

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

        // Add listener to the dropdown for handling selection changes
        collectiblesDropdown.onValueChanged.AddListener(OnDropdownValueChanged);

        // Start movement sequence for each game object
        foreach (var gameObject in gameObjectsWithMeshes)
        {
            if (gameObject != null)
            {
                StartCoroutine(MoveInSquarePattern(gameObject));
            }
        }
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
        gemini.InitializeGemini("Name: " + title + ". Description: " + description);
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

    private IEnumerator MoveInSquarePattern(GameObject obj)
    {
        Vector3[] directions = { Vector3.forward, Vector3.right, Vector3.back, Vector3.left };
        int directionIndex = 0;
        float moveDistance = 2f;
        float stopDuration = 1f;

        while (true)
        {
            if (Vector3.Distance(obj.transform.position, playerTransform.position) <= 3f)
            {
                // Pause movement and face the player
                Vector3 directionToPlayer = (playerTransform.position - obj.transform.position).normalized;
                Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
                obj.transform.rotation = Quaternion.Slerp(obj.transform.rotation, lookRotation, Time.deltaTime * 2f);

                yield return null;
            }
            else
            {
                // Move in the current direction
                Vector3 startPosition = obj.transform.position;
                Vector3 endPosition = startPosition + directions[directionIndex] * moveDistance;

                float elapsedTime = 0f;
                while (elapsedTime < moveDistance)
                {
                    obj.transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / moveDistance);
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }

                // Wait for a while
                yield return new WaitForSeconds(stopDuration);

                // Rotate to the next direction
                obj.transform.Rotate(Vector3.up, 90f);
                directionIndex = (directionIndex + 1) % 4;
            }
        }
    }

    private void CheckClosestObject()
    {
        GameObject closestObject = null;
        float closestDistance = float.MaxValue;

        foreach (var obj in gameObjectsWithMeshes)
        {
            if (obj == null) continue;

            float distance = Vector3.Distance(obj.transform.position, playerTransform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestObject = obj;
            }
        }

        if (closestObject != null && closestObject != currentClosestObject && closestDistance <= 3f)
        {
            currentClosestObject = closestObject;

            int index = gameObjectsWithMeshes.IndexOf(currentClosestObject);
            if (index >= 0 && index < collectibles.Count)
            {
                var selectedCollectible = collectibles[index];
                InitializeGemini(selectedCollectible.Title, selectedCollectible.Description);
            }
        }
    }
}
