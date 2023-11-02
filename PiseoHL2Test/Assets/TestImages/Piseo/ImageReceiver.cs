using System;
using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Quaternion = System.Numerics.Quaternion;
#if WINDOWS_UWP && !UNITY_EDITOR
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
// using Windows.Storage;
#else
using System.Net;
#endif


public class ImageReceiver : MonoBehaviour
{
    private bool isRunning = false;
    private int originalCullingMask;
#if WINDOWS_UWP && !UNITY_EDITOR
     private StreamSocketListener listener;
#else
    private HttpListener listener;
#endif
    public Material targetMaterial;

    public GameObject rightEyePrefab;

    public GameObject rightEyeCamera;

    private Camera rightCamera;

    // public GameObject byldGameObject = null;
    // [SerializeField] private GameObject otherObjects;
    private CancellationTokenSource cts;

    private void Start()
    {
        StartCoroutine(SetCamera());
        StartCoroutine(CheckForFolderImage());

#if WINDOWS_UWP && !UNITY_EDITOR
         // cts = new CancellationTokenSource();
         // listener = new StreamSocketListener();
         // listener.ConnectionReceived += ListenerConnectionReceived;
         // StartCoroutine(BindServiceNameAsync("8000"));
         // isRunning = true;
#else
        listener = new HttpListener();
        listener.Prefixes.Add("http://*:8000/"); // Listen on port 8000. "*" means any available IP.
        listener.Start();
        isRunning = true;
        Thread listenerThread = new Thread(new ThreadStart(ListenerCallback));
        listenerThread.Start();
#endif
    }
#if WINDOWS_UWP && !UNITY_EDITOR
// private IEnumerator BindServiceNameAsync(string port)
// {
//     var bindOperation = listener.BindServiceNameAsync(port).AsTask(cts.Token);
//
//     // Wait until the bind operation is complete
//     while (!bindOperation.IsCompleted)
//     {
//         yield return null; // Wait until next frame
//     }
//
//     try
//     {
//         // This will rethrow any exceptions that were caught during the bind operation
//         bindOperation.GetAwaiter().GetResult();
//         isRunning = true; // Only set to true if binding is successful
//     }
//     catch (Exception ex)
//     {
//         // Handle exceptions (e.g., unable to bind to the port)
//         Debug.LogError($"Exception when trying to bind listener: {ex}");
//         // Consider how to handle this error. Do you want to retry, alert the user, etc.?
//     }
// }
#endif

    private IEnumerator SetCamera()
    {
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
            needsCameraClear = true;
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

        if (needsCameraClear)
        {
            rightEyeCamera.SetActive(false);
            ClearTexture();
            yield break;
        }

        rightEyeCamera.SetActive(true);

        mainCamera.stereoTargetEye = StereoTargetEyeMask.Left;

        mainCamera.cullingMask = 1 << LayerMask.NameToLayer("LeftEye");
        mainCamera.orthographic = true;
        mainCamera.orthographicSize = 468.1612f;
        mainCamera.stereoSeparation = 0;
        mainCamera.farClipPlane = 10000000000000;
    }

   private IEnumerator CheckForFolderImage()
{
#if UNITY_UWP && !UNITY_EDITOR
    while (isRunning)
    { 
        StorageFolder picturesFolder = KnownFolders.PicturesLibrary;
        StorageFolder testImagesFolder = null;

        // Check if the TestImages folder exists or create it if it doesn't
        yield return CheckOrCreateTestImagesFolder(picturesFolder, (folder) => testImagesFolder = folder);

        // Get the files in the TestImages folder
        if (testImagesFolder != null)
        {
            IReadOnlyList<StorageFile> files = null;
            yield return GetFilesInFolder(testImagesFolder, (f) => files = f);

            if (files != null && files.Count > 0)
            {
                StorageFile selectedImageFile = files[0];
                yield return ProcessSelectedImageFile(selectedImageFile, testImagesFolder);
            }
            else
            {
                ClearTexture();
            }
        }

        yield return new WaitForSeconds(1f);
    }
#endif
    yield break;
}
#if UNITY_UWP && !UNITY_EDITOR
private IEnumerator CheckOrCreateTestImagesFolder(StorageFolder picturesFolder, Action<StorageFolder> callback)
{

    StorageFolder testImagesFolder = null;
    try
    {
        testImagesFolder = await picturesFolder.GetFolderAsync("TestImages");
    }
    catch (Exception)
    {
        testImagesFolder = await picturesFolder.CreateFolderAsync("TestImages", CreationCollisionOption.OpenIfExists);
    }
    callback?.Invoke(testImagesFolder);

    yield break;
}
#endif
    
#if UNITY_UWP && !UNITY_EDITOR
private IEnumerator GetFilesInFolder(StorageFolder folder, Action<IReadOnlyList<StorageFile>> callback)
{

    IReadOnlyList<StorageFile> files = null;
    try
    {
        files = await folder.GetFilesAsync();
    }
    catch (Exception e)
    {
        Debug.LogError("Error accessing folder: " + e.Message);
    }
    callback?.Invoke(files);

    yield break;
}
#endif
#if UNITY_UWP && !UNITY_EDITOR
private IEnumerator ProcessSelectedImageFile(StorageFile selectedImageFile, StorageFolder testImagesFolder)
{

    byte[] imageData = null;
    try
    {
        imageData = await FileIO.ReadBufferAsync(selectedImageFile).AsTask();
        if (imageData != null)
        {
            Texture2D newTexture = new Texture2D(2, 2); // Create a small Texture2D with a dummy size, LoadImage will resize it properly.
            if (newTexture.LoadImage(imageData))
            {
                // Use UnityMainThreadDispatcher to execute Unity API calls on the main thread.
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    ApplyTexture(newTexture, selectedImageFile.Name);
                });

                // Delete other images.
                // yield return DeleteOtherFiles(testImagesFolder, selectedImageFile.Name);
            }
            else
            {
                Debug.LogError("Failed to load image data into texture.");
            }
        }
    }
    catch (Exception e)
    {
        Debug.LogError("Error reading file: " + e.Message);
    }

    yield break;
}
#endif
#if UNITY_UWP && !UNITY_EDITOR
// private IEnumerator DeleteOtherFiles(StorageFolder folder, string keepFileName)
// {
//
//     IReadOnlyList<StorageFile> files = null;
//     try
//     {
//         files = await folder.GetFilesAsync();
//         foreach (var file in files)
//         {
//             if (file.Name != keepFileName)
//             {
//                 await file.DeleteAsync();
//             }
//         }
//     }
//     catch (Exception e)
//     {
//         Debug.LogError("Error deleting files: " + e.Message);
//     }
//
//     yield break;
// }
#endif
private void ApplyTexture(Texture2D texture, string textureName)
{
    // Ensure the Unity API calls are here since this is called on the main thread.
    texture.name = textureName;
    targetMaterial.SetTexture("_MainTex", texture);
}

    
#if WINDOWS_UWP && !UNITY_EDITOR
//     private async void ListenerConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
//     {
//         try
//         {
//             // Read the HTTP request.
//             using (var streamReader = new StreamReader(args.Socket.InputStream.AsStreamForRead()))
//             {
//                 string request = await streamReader.ReadLineAsync();
// if (request.Contains("/clearTexture")) // Check for the clearTexture endpoint
//             {
//                 ClearTexture(); // Clear the texture
//
//                 // Send 200 OK response to acknowledge the action
//                 using (Windows.Storage.Streams.DataWriter writer =
//  new Windows.Storage.Streams.DataWriter(args.Socket.OutputStream))
//                 {
//                     writer.WriteString("HTTP/1.1 200 OK\r\nContent-Length: 0\r\n\r\n");
//                     await writer.StoreAsync();
//                 }
//             }
//                else if (request.StartsWith("POST"))
//                 {
//                     await Process(args.Socket);
//                 }
//                 else
//                 {
//                     // Handle non-POST requests here (e.g., send 400 Bad Request response)
//                 }
//             }
//         }
//         catch (Exception e)
//         {
//             Debug.LogError("Error in ListenerConnectionReceived: " + e.Message);
//         }
//     }
//
//     private async Task Process(StreamSocket socket)
//     {
//     try
//     {
//         using (DataReader reader = new DataReader(socket.InputStream))
//         {
//             reader.InputStreamOptions = InputStreamOptions.Partial; // Allow partial reads
//
//             // Parse headers to find content length and content type. (This needs more logic to be robust.)
//             uint contentLength = 0; // TODO: Parse this from the headers
//             string contentType = ""; // TODO: Parse this too
//
//             // Prepare a buffer for the data
//             byte[] buffer = new byte[contentLength];
//
//             await reader.LoadAsync(contentLength);
//             reader.ReadBytes(buffer);
//
//             // Immediately save the raw buffer to see if it's received correctly
//             // System.IO.File.WriteAllBytes("Assets/RawReceivedImage.png", buffer);
//
//             string boundary = "--" + contentType.Split('=')[1];
//             string[] parts =
//  System.Text.Encoding.UTF8.GetString(buffer).Split(new string[] { boundary }, StringSplitOptions.RemoveEmptyEntries);
//             byte[] imageData = null;
//
//             foreach (var part in parts)
//             {
//                 if (part.Contains("filename"))
//                 {
//                     byte[] doubleNewline = Encoding.UTF8.GetBytes("\r\n\r\n");
//
//                     int startOfImageDataInBuffer = IndexOf(buffer, doubleNewline);
//                     if (startOfImageDataInBuffer != -1)
//                     {
//                         startOfImageDataInBuffer += 4; // Skip over the double newline
//
//                         int lengthOfImageData = buffer.Length - startOfImageDataInBuffer;
//                         imageData = new byte[lengthOfImageData];
//                         Array.Copy(buffer, startOfImageDataInBuffer, imageData, 0, lengthOfImageData);
//                     }
//                 }
//             }
//
//             Debug.LogError($"for loop finished, {imageData==null}");
//             if (imageData != null)
//             {
//                 // Unity actions have to be run in the main thread
//                 UnityMainThreadDispatcher.Instance().Enqueue(() =>
//                 {
//                     Debug.Log($"Received image data of size: {imageData.Length} bytes");
//                     //System.IO.File.WriteAllBytes("Assets/DebugReceivedImage.png", imageData);
//                     StartCoroutine(SetCamera());
//                     #if !UNITY_EDITOR
//                     Texture2D receivedTexture = new Texture2D(1440, 963);
//                     bool isLoaded = receivedTexture.LoadImage(imageData);
//                     Sprite sprite =
//  Sprite.Create(receivedTexture, new Rect(0.0f, 0.0f, receivedTexture.width, receivedTexture.height), new Vector2(0.5f, 0.5f), 100.0f);
//                     Debug.Log("Texture loaded: " + isLoaded);
//                     if (isLoaded)
//                     {
//                         targetMaterial.SetTexture("_MainTex", sprite.texture);
//                     }
//                     else
//                     {
//                         Debug.LogError("Failed to load image onto texture.");
//                     }
//                     #endif
//
//                 });
//
//                 // Send 200 OK response
//                 using (Windows.Storage.Streams.DataWriter writer =
//  new Windows.Storage.Streams.DataWriter(socket.OutputStream))
//                 {
//                     writer.WriteString("HTTP/1.1 200 OK\r\nContent-Length: 0\r\n\r\n");
//                     await writer.StoreAsync();
//                 }
//             }
//             else
//             {
//                 Debug.LogError("Failed to extract image data.");
//                 // Send 400 Bad Request response
//                 using (Windows.Storage.Streams.DataWriter writer =
//  new Windows.Storage.Streams.DataWriter(socket.OutputStream))
//                 {
//                     writer.WriteString("HTTP/1.1 400 Bad Request\r\nContent-Length: 0\r\n\r\n");
//                     await writer.StoreAsync();
//                 }
//             }
//         }
//     }
//     catch (Exception e)
//     {
//         Debug.LogError("Error processing request: " + e.Message);
//         // Send 500 Internal Server Error response
//         using (Windows.Storage.Streams.DataWriter writer = new Windows.Storage.Streams.DataWriter(socket.OutputStream))
//         {
//             writer.WriteString("HTTP/1.1 500 Internal Server Error\r\nContent-Length: 0\r\n\r\n");
//             await writer.StoreAsync();
//         }
//     }
// }
#else
    private void ListenerCallback()
    {
        while (isRunning)
        {
            try
            {
                var context = listener.GetContext(); // Block until a connection comes in.
                if (context != null)
                    Process(context);
            }
            catch (HttpListenerException e)
            {
                // This exception is thrown when listener is stopped while waiting in GetContext.
                // We can safely ignore it if we're shutting down; otherwise, you might want to handle or log it.
                if (isRunning)
                {
                    Debug.LogError("HttpListenerException: " + e.Message + e.StackTrace);
                }
            }
            catch (Exception e)
            {
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    Debug.LogError("Exception in ListenerCallback: " + e.Message + e.StackTrace);
                });
            }
        }
    }

    private void Process(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;
        Debug.Log($"We received a request!");

        try
        {
            if (request.Url.AbsolutePath.Equals("/clearTexture", StringComparison.OrdinalIgnoreCase))
            {
                ClearTexture();

                response.StatusCode = 200;
                response.StatusDescription = "OK";
                response.Close();
                return; // Important: Return here to avoid processing the request further.
            }


            if (request.HttpMethod == "POST" && request.ContentLength64 > 0)
            {
                byte[] buffer;

                // Reading the entire stream into buffer
                using (MemoryStream ms = new MemoryStream())
                {
                    byte[] tempBuffer = new byte[4096];
                    int bytesRead;
                    while ((bytesRead = request.InputStream.Read(tempBuffer, 0, tempBuffer.Length)) > 0)
                    {
                        ms.Write(tempBuffer, 0, bytesRead);
                    }

                    buffer = ms.ToArray(); // Now 'buffer' contains the full request payload.
                }

                // Immediately save the raw buffer to see if it's received correctly

                // System.IO.File.WriteAllBytes("Assets/RawReceivedImage.png", buffer);

                string boundary = "--" + request.ContentType.Split('=')[1];
                string[] parts = System.Text.Encoding.UTF8.GetString(buffer)
                    .Split(new string[] { boundary }, StringSplitOptions.RemoveEmptyEntries);
                byte[] imageData = null;

                foreach (var part in parts)
                {
                    if (part.Contains("filename"))
                    {
                        byte[] doubleNewline = Encoding.UTF8.GetBytes("\r\n\r\n");

                        int startOfImageDataInBuffer = IndexOf(buffer, doubleNewline);
                        if (startOfImageDataInBuffer != -1)
                        {
                            startOfImageDataInBuffer += 4; // Skip over the double newline

                            int lengthOfImageData = buffer.Length - startOfImageDataInBuffer;
                            imageData = new byte[lengthOfImageData];
                            Array.Copy(buffer, startOfImageDataInBuffer, imageData, 0, lengthOfImageData);
                        }
                    }
                }

                Debug.LogError($"for loop finished, {imageData == null}");
                if (imageData != null)
                {
                    // Unity actions have to be run in the main thread
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        Debug.Log($"Received image data of size: {imageData.Length} bytes");
                        // System.IO.File.WriteAllBytes("Assets/DebugReceivedImage.png", imageData);
                        StartCoroutine(SetCamera());
                        Texture2D receivedTexture = new Texture2D(1440, 963);
                        bool isLoaded = receivedTexture.LoadImage(imageData);
                        Sprite sprite = Sprite.Create(receivedTexture,
                            new Rect(0.0f, 0.0f, receivedTexture.width, receivedTexture.height),
                            new Vector2(0.5f, 0.5f), 100.0f);
                        Debug.Log("Texture loaded: " + isLoaded);
                        if (isLoaded)
                        {
                            targetMaterial.SetTexture("_MainTex", sprite.texture);
                        }
                        else
                        {
                            Debug.LogError("Failed to load image onto texture.");
                        }
                    });

                    response.StatusCode = 200;
                    response.StatusDescription = "OK";
                }
                else
                {
                    Debug.LogError("Failed to extract image data.");
                    response.StatusCode = 400;
                    response.StatusDescription = "Bad Request";
                }
            }
            else
            {
                response.StatusCode = 400;
                response.StatusDescription = "Bad Request";
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error processing request: " + e.Message);

            response.StatusCode = 500;
            response.StatusDescription = "Internal Server Error";
        }
        finally
        {
            // Close the input stream and response to free up resources.
            request.InputStream.Close();
            response.Close();
        }
    }

#endif

    private static int IndexOf(byte[] haystack, byte[] needle)
    {
        for (int i = 0; i <= haystack.Length - needle.Length; i++)
        {
            bool found = true;
            for (int j = 0; j < needle.Length; j++)
            {
                if (haystack[i + j] != needle[j])
                {
                    found = false;
                    break;
                }
            }

            if (found) return i;
        }

        return -1;
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
#if WINDOWS_UWP && !UNITY_EDITOR
    listener?.Dispose();
    cts?.Cancel();
#else
        listener?.Stop();
#endif
        // If you have started any threads, make sure to join them here.
    }
}