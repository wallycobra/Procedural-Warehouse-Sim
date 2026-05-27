using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class WarehouseApiClient : MonoBehaviour
{
    [SerializeField] private string baseUrl = "http://127.0.0.1:5064/api/warehouses";

    public IEnumerator SaveWarehouse(
        WarehouseSaveData saveData,
        System.Action<string> onSuccess = null,
        System.Action<string> onError = null)
    {
        string json = JsonUtility.ToJson(saveData, true);

        using UnityWebRequest request = new UnityWebRequest(baseUrl, "POST");

        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();
        Debug.Log($"Request URL: {request.url}");
        if (request.result != UnityWebRequest.Result.Success)
        {
            string message =
                $"Error: {request.error}\n" +
                $"Status Code: {request.responseCode}\n" +
                $"Response: {request.downloadHandler.text}";
            onError?.Invoke(message);
            yield break;
        }

        onSuccess?.Invoke(request.downloadHandler.text);
    }

    public IEnumerator LoadWarehouse(
        string warehouseId,
        System.Action<WarehouseSaveData> onSuccess,
        System.Action<string> onError = null)
    {
        string url = $"{baseUrl}/{warehouseId}";

        using UnityWebRequest request = UnityWebRequest.Get(url);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke(request.error);
            yield break;
        }

        WarehouseSaveData saveData =
            JsonUtility.FromJson<WarehouseSaveData>(
                request.downloadHandler.text);

        onSuccess?.Invoke(saveData);
    }

    public IEnumerator GetWarehouses(
        System.Action<string> onSuccess,
        System.Action<string> onError = null)
    {
        using UnityWebRequest request = UnityWebRequest.Get(baseUrl);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke(request.error);
            yield break;
        }

        onSuccess?.Invoke(request.downloadHandler.text);
    }

    public IEnumerator DeleteWarehouse(
        string warehouseId,
        System.Action onSuccess = null,
        System.Action<string> onError = null)
    {
        string url = $"{baseUrl}/{warehouseId}";

        using UnityWebRequest request = UnityWebRequest.Delete(url);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke(request.error);
            yield break;
        }

        onSuccess?.Invoke();
    }
}