// using System;

using System;
using UnityEngine;
using System.Collections;
using System.IO;
// using System.Text;
using System.Threading;
using Microsoft.Graph;
using File = System.IO.File;

#if WINDOWS_UWP && !UNITY_EDITOR
// using Windows.Networking.Sockets;
// using Windows.Storage.Streams;
// using Windows.Storage;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using System.Linq;

#else
// using System.Net;
#endif


public class ImageReceiver : MonoBehaviour
{
    private bool isRunning = false;

    private int originalCullingMask;

// #if WINDOWS_UWP && !UNITY_EDITOR
//      private StreamSocketListener listener;
// #else
//     private HttpListener listener;
// #endif
    public Material targetMaterial;

    public GameObject rightEyePrefab;

    public GameObject rightEyeCamera;

    private Camera rightCamera;
    private string prevImage = "none";
    private string cameraTransformFilePath = "CameraTransform.txt"; // File path for camera transform
    // public GameObject byldGameObject = null;
    // [SerializeField] private GameObject otherObjects;
    private CancellationTokenSource cts;

    private void Start()
    {
        isRunning = true;
        StartCoroutine(SetCamera(readFromFile: true));
        StartCoroutine(CheckForFolderImage());
    }

    private IEnumerator SetCamera(bool readFromFile = false)
    {
        
        // if (readFromFile && File.Exists(cameraTransformFilePath))
        // {
        //     // Read and set camera transform from file
        //     string[] lines = File.ReadAllLines(cameraTransformFilePath);
        //     if (lines.Length >= 2)
        //     {
        //         transform.localPosition = StringToVector3(lines[0]);
        //         transform.localRotation = Quaternion.Euler(StringToVector3(lines[1]));
        //     }
        // }
        // else
        // {
        //     // Save current camera transform to file
        //     File.WriteAllLines(cameraTransformFilePath, new string[] { 
        //         Vector3ToString(transform.localPosition), 
        //         Vector3ToString(transform.localRotation.eulerAngles) 
        //     });
        // }
        
        bool needsCameraClear = false;
        yield return new WaitForSeconds(0.1f);
        while (Camera.main != null && transform.parent != Camera.main.transform)
        {
            yield return null;
            if (Camera.main == null)
                continue;
            needsCameraClear = true;
            transform.parent = Camera.main.transform;

            transform.localRotation = UnityEngine.Quaternion.Euler(-0.3f, 0.69f, -0.12f);
            transform.localPosition = new Vector3(3.29f, 1.64f, 184.8f);
            transform.localScale = new Vector3(0.1017f, 0.1f, 0.1f);
        }

        transform.parent = Camera.main.transform;


        transform.localRotation = UnityEngine.Quaternion.Euler(-0.3f, 0.69f, -0.12f);
        transform.localPosition = new Vector3(3.29f, 1.64f, 184.8f);
        transform.localScale = new Vector3(0.1017f, 0.1f, 0.1f);

        if (rightEyeCamera == null)
        {
            yield return new WaitForSeconds(0.5f);
            rightEyeCamera = Instantiate(rightEyePrefab, transform.parent.parent);
            rightCamera = rightEyeCamera.GetComponent<Camera>();
            // needsCameraClear = true;
        }


        rightCamera.stereoTargetEye = StereoTargetEyeMask.Right;
        rightCamera.cullingMask = 1 << LayerMask.NameToLayer("RightEye");
        rightCamera.orthographic = true;
        rightCamera.orthographicSize = 468.1612f;
        rightCamera.stereoSeparation = 0;
        rightCamera.farClipPlane = 10000;
        rightCamera.clearFlags = CameraClearFlags.SolidColor;
        rightCamera.backgroundColor = Color.black;
        Camera mainCamera = Camera.main;
        mainCamera.clearFlags = CameraClearFlags.SolidColor;
        mainCamera.backgroundColor = Color.black;
        originalCullingMask = mainCamera.cullingMask;
        int leftEyeLayer = LayerMask.NameToLayer("LeftEye");
        int rightEyeLayer = LayerMask.NameToLayer("RightEye");

        originalCullingMask &= ~(1 << leftEyeLayer);
        originalCullingMask &= ~(1 << rightEyeLayer);

        // if (needsCameraClear)
        // {
        //     rightEyeCamera.SetActive(false);
        //     ClearTexture();
        //     yield break;
        // }

        rightEyeCamera.SetActive(true);

        mainCamera.stereoTargetEye = StereoTargetEyeMask.Left;

        mainCamera.cullingMask = 1 << LayerMask.NameToLayer("LeftEye");
        mainCamera.orthographic = true;
        mainCamera.orthographicSize = 468.1612f;
        mainCamera.stereoSeparation = 0;
        mainCamera.farClipPlane = 10000000000000;
    }
// Utility method to convert a Vector3 to a string
    private string Vector3ToString(Vector3 vector)
    {
        return $"{vector.x},{vector.y},{vector.z}";
    }

    // Utility method to convert a string to a Vector3
    private Vector3 StringToVector3(string sVector)
    {
        string[] sArray = sVector.Split(',');
        return new Vector3(
            float.Parse(sArray[0]),
            float.Parse(sArray[1]),
            float.Parse(sArray[2]));
    }

private IEnumerator CheckForFolderImage()
{
    isRunning = true;
    DebugLog.Log("CheckForFolderImage coroutine started.");
    while (isRunning)
    {
        #if WINDOWS_UWP && !UNITY_EDITOR
        DebugLog.Log("UWP Platform detected.");
        // UWP-specific code to access the Pictures library
        var picturesFolder = Windows.Storage.KnownFolders.PicturesLibrary;
        var piseoFolderOperation = picturesFolder.CreateFolderAsync("Piseo", Windows.Storage.CreationCollisionOption.OpenIfExists);
        DebugLog.Log("Attempting to open or create Piseo folder in Pictures.");

        // Wait for the folder creation operation to complete
        DateTime startTime = DateTime.Now;
        while (piseoFolderOperation.Status != AsyncStatus.Completed && DateTime.Now - startTime < TimeSpan.FromSeconds(3f))
        {
            yield return null;
        }
        if(DateTime.Now - startTime >= TimeSpan.FromSeconds(3f))
            DebugLog.LogError("PiseoFolderOperation timed out.");
        DebugLog.Log("Piseo folder access status: " + piseoFolderOperation.Status);

        if (piseoFolderOperation.Status == AsyncStatus.Completed)
        {
            var piseoFolder = piseoFolderOperation.GetResults();
            var filesOperation = piseoFolder.GetFilesAsync();

            // Wait for the file retrieval operation to complete
            startTime = DateTime.Now;
            while (filesOperation.Status != AsyncStatus.Completed && DateTime.Now - startTime < TimeSpan.FromSeconds(3f))
            {
                yield return null;
            }
            if(DateTime.Now - startTime >= TimeSpan.FromSeconds(3f))
                DebugLog.LogError("filesOperation timed out.");
            if (filesOperation.Status == AsyncStatus.Completed)
            {
                var files = filesOperation.GetResults();
                // Convert IReadOnlyList to List for LINQ support
                var fileList = files.ToList();
                var imageFiles = fileList.Where(file => file.FileType == ".png").ToList();

                DebugLog.Log($"Found {imageFiles.Count} PNG files in Piseo folder.");

                if (imageFiles.Count > 0)
                {
                    var selectedImageFile = imageFiles[0]; // Assuming you want to process the first image
                    if (selectedImageFile.Name != prevImage)
                    {
                        prevImage = selectedImageFile.Name;
                        DebugLog.Log("Selected image file: " + selectedImageFile.Name);
                        yield return ProcessSelectedImageFile(selectedImageFile);
                    }
                }
            }
        }
        else
        {
            DebugLog.LogError("Failed to open or create Piseo folder in Pictures.");
        }
        // #else
        // ... [fallback code for other platforms]
        #endif
        yield return new WaitForSeconds(1f); // Check for new images every second
    }
}
#if WINDOWS_UWP && !UNITY_EDITOR
private IEnumerator ProcessSelectedImageFile(Windows.Storage.StorageFile imageFile)
{
    DebugLog.Log("Processing image file: " + imageFile.Path);

    var bufferOperation = Windows.Storage.FileIO.ReadBufferAsync(imageFile);

    // Wait for the read buffer operation to complete
    DateTime startTime = DateTime.Now;
    while (bufferOperation.Status != AsyncStatus.Completed && DateTime.Now - startTime < TimeSpan.FromSeconds(3f))
    {
        yield return null;
    }
    if(DateTime.Now - startTime >= TimeSpan.FromSeconds(3f))
        DebugLog.LogError("bufferOperation timed out.");
    
    if (bufferOperation.Status == AsyncStatus.Completed)
    {
        var buffer = bufferOperation.GetResults();
        byte[] imageData = buffer.ToArray();  // Using WindowsRuntimeBufferExtensions
        
        // Texture2D newTexture = new Texture2D(2, 2); // Create a small Texture2D with a dummy size, LoadImage will resize it properly.
        // if (newTexture.LoadImage(imageData))
        // {
            DebugLog.LogError("Load image data into texture. Success");
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                DebugLog.Log($"Received image data of size: {imageData.Length} bytes");
                        // System.IO.File.WriteAllBytes("Assets/DebugReceivedImage.png", imageData);
                        StartCoroutine(SetCamera());
                        Texture2D receivedTexture = new Texture2D(1440, 963);
                        bool isLoaded = receivedTexture.LoadImage(imageData);
                        Sprite sprite = Sprite.Create(receivedTexture,
                            new UnityEngine.Rect(0.0f, 0.0f, receivedTexture.width, receivedTexture.height),
                            new Vector2(0.5f, 0.5f), 100.0f);
                        DebugLog.Log("Texture loaded: " + isLoaded);
                        if (isLoaded)
                        {
                            targetMaterial.SetTexture("_MainTex", sprite.texture);
                        }
                        else
                        {
                            DebugLog.LogError("Failed to load image onto texture.");
                        }


                // ApplyTexture(newTexture, imageFile.Name);
                // DebugLog.LogError("Apply image data into texture. Success");
            });
        // }
        
        
    }
    else
    {
        DebugLog.LogError("Failed to read image file.");
    }

    yield break;
}
#endif


    private void ApplyTexture(Texture2D texture, string textureName)
    {
        // Ensure the Unity API calls are here since this is called on the main thread.
        texture.name = textureName;
        targetMaterial.SetTexture("_MainTex", texture);
    }


// Method to clear the texture
    private void ClearTexture()
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            Camera mainCamera = Camera.main;
            mainCamera.stereoTargetEye = StereoTargetEyeMask.Both;
            mainCamera.cullingMask = originalCullingMask;
            mainCamera.orthographic = false;
            mainCamera.orthographicSize = 1f;
            mainCamera.stereoSeparation = 0.02f;
            mainCamera.farClipPlane = 1000;

            rightEyeCamera.SetActive(false);
            if (targetMaterial && targetMaterial.mainTexture)
            {
                Texture2D clearTexture = new Texture2D(1, 1);
                clearTexture.SetPixel(0, 0, Color.clear);
                clearTexture.Apply();
                targetMaterial.mainTexture = clearTexture;
                transform.localPosition = new Vector3(100000, 0, transform.localPosition.z);
            }
        });
    }

    private void OnApplicationQuit()
    {
        isRunning = false;

        // If you have started any threads, make sure to join them here.
    }
}