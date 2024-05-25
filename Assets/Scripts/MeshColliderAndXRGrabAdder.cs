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

    private List<CollectibleDto> collectibles = new List<CollectibleDto>(); // Store the list of collectibles
    private GameObject currentClosestObject; // Track the current closest object

    private Dictionary<string, (Vector3 position, float scale)> positionScaleMap = new Dictionary<string, (Vector3 position, float scale)>()
    {
        { "Statue of Liberty", (new Vector3(13, -3.5f, 3), 0.1f) },
        { "XB-21", (new Vector3(-2, 7f, -10), 0.2f) },
        { "The Thinker", (new Vector3(-5, 0f, 5), 2f) },
        { "Car", (new Vector3(10, 0.4f, 15), 1f) },
        { "Mona Lisa", (new Vector3(5, 1.9f, 5), 1f) },
        { "Santa Claus", (new Vector3(10, 0f, 5), 0.7f) },
        { "Spongebob", (new Vector3(5, 0f, 5), 0.003f) },
        { "Pikachu", (new Vector3(10, 0f, 5), 0.03f) },
        { "Shrek", (new Vector3(15, 0f, 15), 0.01f) },
        { "Mario", (new Vector3(10, 0f, 20), 0.3f) },
        { "Eiffel Tower", (new Vector3(-6, 0, -8), 0.05f) },
        { "Starry Night", (new Vector3(-10, 2, -8), 0.3f) },
        { "American Gothic", (new Vector3(2, 2.5f, -4), 0.4f) },
        { "The Scream", (new Vector3(5, 1.5f, -8), 2f) },
        { "Terratoca Army", (new Vector3(10, 5.2f, 16), 5f) },
         { "Christ the Redeemer", (new Vector3(-5, 2f, -7), 0.4f) },
         { "Moai Head", (new Vector3(5, 1.4f, 10), 3f) },
    };

    private string collectibleArtist = "William Wang";

    private void Start()
    {
    }

    private void Update()
    {
        CheckClosestObject();
    }

    public void LoadGltfAssetsAndAddComponents(List<CollectibleDto> collectibles)
    {
        this.collectibles = collectibles;
        LoadGltfAssetsAndAddComponentsInternal();
    }

    public void LoadGltfAssetsAndAddComponents(List<ArtworkRegistryService.ArtworkDTO> artworks)
    {
        collectibleArtist = "Art";
        List<CollectibleDto> collectibles = new List<CollectibleDto>();
        foreach (var artwork in artworks)
        {
            CollectibleDto collectible = new CollectibleDto
            {
                Title = artwork.Name,
                Description = artwork.Description,
                Nft = new CollectibleDto.CollectibleNft { IpfsUrl = artwork.FileUrl },
                Versions = new List<CollectibleDto.CollectibleVersion> { new CollectibleDto.CollectibleVersion { Asset = artwork.FileUrl } }
            };
            // Add or update positionScaleMap with positions and scales
            positionScaleMap[collectible.Title] = (new Vector3((float)artwork.X, (float)artwork.Y, (float)artwork.Z), (float)artwork.Size / 100);
            collectibles.Add(collectible);
        }
        LoadGltfAssetsAndAddComponents(collectibles);
    }

    private void LoadGltfAssetsAndAddComponentsInternal()
    {
        // Ensure collectibles list is not null and contains expected items
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
            if (gameObjectsWithMeshes == null || gameObjectsWithMeshes.Count <= i)
            {
                Debug.LogError("Game objects with meshes list is null or does not contain enough items");
                continue;
            }

            var gameObject = gameObjectsWithMeshes[i];
            var collectible = collectibles[i];

            // Add title to the dropdown options
            dropdownOptions.Add(collectible.Title);

            Debug.Log($"Processing collectible: {collectible.Title}");

            // Check if the title exists in the positionScaleMap dictionary
            if (positionScaleMap.ContainsKey(collectible.Title))
            {
                // Load the GLTF asset and set properties using the position and scale from the dictionary
                LoadGltfAsset(gameObject, collectible.Title, collectible.Description, collectible.Versions[0].Asset, positionScaleMap[collectible.Title].position, positionScaleMap[collectible.Title].scale);
            }
            else
            {
                // Load the GLTF asset and set properties with default position (0, 0, 0) and scale of 1
                LoadGltfAsset(gameObject, collectible.Title, collectible.Description, collectible.Versions[0].Asset, new Vector3(Random.Range(-3f, 3f), 1, Random.Range(-10f, 10f)), 1f);
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
                StartCoroutine(MoveRandomly(gameObject));
            }
        }
    }

    private void LoadGltfAsset(GameObject gameObject, string title, string description, string assetUrl, Vector3 position, float scale)
    {
        if (gameObject != null && !string.IsNullOrEmpty(assetUrl))
        {
            Debug.Log($"{title} Loading GLTF asset: {assetUrl}");
            var gltf = gameObject.AddComponent<GltfAsset>();
            gltf.Url = assetUrl;
            gameObject.name = title;

            // Set initial position and scale based on the title
            gameObject.transform.position = position;
            gameObject.transform.localScale = Vector3.one * scale;
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
        gemini.InitializeGemini("Name: " + title + ". Description: " + description, title);
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

    private IEnumerator MoveRandomly(GameObject obj)
    {
        Vector3 originalPosition = obj.transform.position;
        float moveDistance = 2f;
        float minWaitTime = 0.5f;
        float maxWaitTime = 2f;
        float moveSpeed = 0.5f; // Adjust to control the movement speed

        while (true)
        {
            if (Vector3.Distance(obj.transform.position, playerTransform.position) <= 4f)
            {
                // Pause movement and face the player
                Vector3 directionToPlayer = (playerTransform.position - obj.transform.position).normalized;

                // Zero out the y-component to only rotate around the y-axis
                directionToPlayer.y = 0;

                // Normalize the direction vector again after modification
                directionToPlayer = directionToPlayer.normalized;

                Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
                obj.transform.rotation = Quaternion.Slerp(obj.transform.rotation, lookRotation, Time.deltaTime * 2f);

                yield return null;
            }
            else
            {
                // Determine random direction and distance within range
                Vector3 randomDirection = new Vector3(
                    Random.Range(-1f, 1f),
                    0,
                    Random.Range(-1f, 1f)
                ).normalized;

                float randomDistance = Random.Range(0.5f, moveDistance);
                Vector3 targetPosition = obj.transform.position + randomDirection * randomDistance;

                // Ensure the target position is within the allowed range from the original position
                if (Vector3.Distance(targetPosition, originalPosition) > moveDistance)
                {
                    targetPosition = originalPosition + (targetPosition - originalPosition).normalized * moveDistance;
                }

                // Rotate to face the target direction
                Quaternion targetRotation = Quaternion.LookRotation(randomDirection);
                while (Quaternion.Angle(obj.transform.rotation, targetRotation) > 0.1f)
                {
                    obj.transform.rotation = Quaternion.Slerp(obj.transform.rotation, targetRotation, Time.deltaTime * 2f);
                    yield return null;
                }

                // Move towards the target position
                while (Vector3.Distance(obj.transform.position, targetPosition) > 0.1f)
                {
                    obj.transform.position = Vector3.MoveTowards(obj.transform.position, targetPosition, moveSpeed * Time.deltaTime);
                    yield return null;
                }

                // Ensure the final position is exactly the target position
                obj.transform.position = targetPosition;

                // Wait for a random duration before the next movement
                float randomStopDuration = Random.Range(minWaitTime, maxWaitTime);
                yield return new WaitForSeconds(randomStopDuration);
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

            // Calculate the distance on the x and z coordinates only
            Vector3 objPositionXZ = new Vector3(obj.transform.position.x, 0, obj.transform.position.z);
            Vector3 playerPositionXZ = new Vector3(playerTransform.position.x, 0, playerTransform.position.z);
            float distance = Vector3.Distance(objPositionXZ, playerPositionXZ);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestObject = obj;
            }
        }

        if (closestObject != null && closestObject != currentClosestObject && closestDistance <= 4f)
        {
            Debug.Log($"Closest object: {closestObject.name} at distance: {closestDistance}");
            currentClosestObject = closestObject;

            int index = gameObjectsWithMeshes.IndexOf(currentClosestObject);
            if (index >= 0 && index < collectibles.Count)
            {
                collectiblesDropdown.value = (index);
            }
            else
            {
                Debug.LogError("Index out of range. Check if gameObjectsWithMeshes and collectibles lists are synchronized.");
            }
        }
    }

}
