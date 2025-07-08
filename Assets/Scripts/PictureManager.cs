using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using SFB;

public class PictureManager : MonoBehaviour
{
    [SerializeField] private GameObject pieceMaker, selectFolderButton;
    private List<Texture2D> _texture2Ds = new List<Texture2D>();

    void Start()
    {
        selectFolderButton.SetActive(true);
    }

    // フォルダ選択ボタンから呼び出すメソッド
    public void OpenFolder()
    {
        // フォルダ選択ダイアログを表示
        var paths = StandaloneFileBrowser.OpenFolderPanel("画像フォルダを選択", "", false);

        // パスが選択されたかチェック
        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            // 画像を読み込む
            LoadImagesFromDirectory(paths[0]);
            
            // ピースを作成
            pieceMaker.GetComponent<PieceMaker>().CreateAllPieces(_texture2Ds.ToArray());
            
            // ボタンを無効化
            selectFolderButton.SetActive(false);
        }
    }

    private void LoadImagesFromDirectory(string directoryPath)
    {
        // 指定可能な画像形式の拡張子
        string[] extensions = { ".png", ".jpg", ".jpeg" };

        foreach (var ext in extensions)
        {
            // 各拡張子のファイルを取得
            string[] files = Directory.GetFiles(directoryPath, "*" + ext);

            foreach (var file in files)
            {
                // 画像ファイルをバイト配列として読み込む
                byte[] bytes = File.ReadAllBytes(file);

                // Texture2Dを作成し、画像データをロード
                // サイズは仮で2x2。LoadImageで自動的にリサイズされる。
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(bytes);
                _texture2Ds.Add(texture);
            }
        }
    }
}