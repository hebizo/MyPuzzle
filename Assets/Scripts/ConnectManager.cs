using UnityEngine;
using System;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class ConnectManager : MonoBehaviour
{
    public string host = "127.0.0.1";
    public int port = 49152;
    public Renderer targetRenderer; // 最新の画像を表示するRenderer
    [SerializeField] private GameObject statusTextObject;
    
    private TextMeshProUGUI statusText; // 状態を表示するUI Text (任意)
    private TcpClient client;
    private NetworkStream stream;
    private Thread clientReceiveThread;

    private List<Texture2D> receivedTextures = new List<Texture2D>();
    private Queue<Action> mainThreadActions = new Queue<Action>();

    void Start()
    {
        statusText = statusTextObject.GetComponent<TextMeshProUGUI>();
        ConnectToServer();
    }

    void ConnectToServer()
    {
        try
        {
            clientReceiveThread = new Thread(new ThreadStart(ListenForData));
            clientReceiveThread.IsBackground = true;
            clientReceiveThread.Start();
        }
        catch (Exception e)
        {
            Debug.LogError("On client connect error: " + e);
            UpdateStatus("接続エラー");
        }
    }

    private void ListenForData()
    {
        try
        {
            client = new TcpClient(host, port);
            stream = client.GetStream();
            UpdateStatus("サーバーに接続しました。");

            // 画像枚数を受信 (Python側で送信する場合)
            byte[] numImagesBytes = ReadBytes(4);
            int totalImages = BitConverter.ToInt32(numImagesBytes, 0);
            UpdateStatus($"受信開始: {totalImages} 枚の画像");

            // ヘッダーサイズ (IIIQ = 4+4+4+8 = 20 bytes)
            int headerSize = 20;
            byte[] headerBytes = new byte[headerSize];

            for (int i = 0; i < totalImages; i++)
            {
                // 1. ヘッダーを受信
                byte[] receivedHeader = ReadBytes(headerSize);
                if (receivedHeader == null) break; // 接続切断

                // ヘッダーをデコード
                int width = BitConverter.ToInt32(receivedHeader, 0);
                int height = BitConverter.ToInt32(receivedHeader, 4);
                int channels = BitConverter.ToInt32(receivedHeader, 8);
                long dataSize = BitConverter.ToInt64(receivedHeader, 12);

                // 2. 画像データを受信
                byte[] dataBytes = ReadBytes((int)dataSize);
                if (dataBytes == null) break; // 接続切断

                UpdateStatus($"受信中: {i + 1}/{totalImages} ({width}x{height} Ch:{channels})");

                // メインスレッドでテクスチャ処理を実行するようにキューに追加
                lock (mainThreadActions)
                {
                    mainThreadActions.Enqueue(() => ProcessReceivedImage(width, height, channels, dataBytes));
                }
            }
            UpdateStatus("すべての画像を受信しました。");
        }
        catch (Exception e)
        {
            Debug.LogError("Receive error: " + e);
            UpdateStatus("受信エラー: " + e.Message);
        }
        finally
        {
            CloseConnection();
        }
    }

    // 指定されたバイト数をストリームから読み取るヘルパー関数
    private byte[] ReadBytes(int bytesToRead)
    {
        byte[] buffer = new byte[bytesToRead];
        int bytesRead = 0;
        try
        {
            while (bytesRead < bytesToRead)
            {
                int read = stream.Read(buffer, bytesRead, bytesToRead - bytesRead);
                if (read == 0)
                {
                    Debug.LogWarning("サーバーが切断されました。");
                    return null; // 接続が切断された
                }
                bytesRead += read;
            }
        }
        catch (IOException ex) {
            Debug.LogError($"IO Exception: {ex.Message}");
            return null;
        }
        catch (ObjectDisposedException ex) {
            Debug.LogWarning($"Stream closed: {ex.Message}");
            return null;
        }

        return buffer;
    }


    // メインスレッドで実行される画像処理
    private void ProcessReceivedImage(int width, int height, int channels, byte[] data)
    {
        TextureFormat format;
        switch (channels)
        {
            case 1:
                format = TextureFormat.R8; // グレースケール
                break;
            case 3:
                format = TextureFormat.RGB24; // RGB
                break;
            case 4:
                format = TextureFormat.RGBA32; // RGBA
                break;
            default:
                Debug.LogError($"サポートされていないチャンネル数: {channels}");
                return;
        }

        // 新しいテクスチャを作成するか、既存のものを再利用
        Texture2D tex = new Texture2D(width, height, format, false);
        tex.LoadRawTextureData(data);
        tex.Apply();

        receivedTextures.Add(tex); // リストに追加

        // 最新の画像をRendererに表示
        if (targetRenderer != null)
        {
            targetRenderer.material.mainTexture = tex;
        }

        // 必要に応じてリストに追加したテクスチャを利用する処理をここに書く
        Debug.Log($"テクスチャを生成/更新しました。合計: {receivedTextures.Count} 枚");
    }

    // メインスレッドでUIを更新するためのヘルパー
    private void UpdateStatus(string message)
    {
        Debug.Log(message);
        if (statusText != null)
        {
            lock (mainThreadActions)
            {
                mainThreadActions.Enqueue(() => { statusText.text = message; });
            }
        }
    }


    void Update()
    {
        // メインスレッドで実行すべきアクションを処理
        lock (mainThreadActions)
        {
            while (mainThreadActions.Count > 0)
            {
                mainThreadActions.Dequeue().Invoke();
            }
        }
    }

    void CloseConnection()
    {
         if (stream != null) stream.Close();
         if (client != null) client.Close();
         stream = null;
         client = null;
         Debug.Log("接続を閉じました。");
    }

    void OnApplicationQuit()
    {
        CloseConnection();
        if (clientReceiveThread != null && clientReceiveThread.IsAlive)
        {
             clientReceiveThread.Abort();
        }
    }
}
