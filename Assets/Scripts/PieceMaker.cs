using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCvSharp;

public class PieceMaker : MonoBehaviour
{
    [SerializeField] private Texture2D rawImage;
    [SerializeField] private GameObject piece;

    private Image _image;

    [SerializeField] private int width;
    [SerializeField] private int height;

    [SerializeField] private Camera mainCamera;

    [SerializeField] private Texture2D testImage1;
    [SerializeField] private Texture2D testImage2;
    [SerializeField] private Texture2D testImage3;
    
    // Start is called before the first frame update
    void Start()
    {
        // preprocess
        // get SpriteRenderer of piece

        // resize image
        //Texture2D imageRes = ResizeImage(rawImage);
        
        //GameObject obj = Instantiate(piece);
        //SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        //sr.sprite = Sprite.Create(imageRes, new UnityEngine.Rect(0,0,rawImage.width,rawImage.height),Vector2.zero);


        // make image for piece

        // make piece
        Texture2D[] images = {testImage1, testImage2, testImage3};
        CreatePiece(images);
    }

    // function for resizing raw image
    private Texture2D ResizeImage(Texture2D image)
    {
        Mat imageMat = OpenCvSharp.Unity.TextureToMat(image);
        Cv2.Resize(imageMat, imageMat, new OpenCvSharp.Size(width, height));
        Texture2D imageRes = OpenCvSharp.Unity.MatToTexture(imageMat);
        return imageRes;
    }

    //private Image[] TrimImage(Texture2D image)
    //{
        
    //}

    private void CreatePiece(Texture2D[] images)
    {
        foreach (Texture2D img in images)
        {
            Vector3 pos = new Vector3(0f, 0f, 0f);
            GameObject pieceIns = Instantiate(piece, pos, Quaternion.identity);
            
            // attach each image to piece
            SpriteRenderer sr = pieceIns.GetComponent<SpriteRenderer>();
            sr.sprite = Sprite.Create(img, new UnityEngine.Rect(0,0,img.width,img.height), new Vector2(0.5f,0.5f));
            
            // attach main camera to piece
            pieceIns.GetComponent<Piece>().cam = mainCamera;
        }
    }
}
