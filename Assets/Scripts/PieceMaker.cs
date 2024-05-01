using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.CoreModule;

public class PieceMaker : MonoBehaviour
{
    [SerializeField] private Texture2D rawImage;
    [SerializeField] private GameObject piece;

    private Image _image;

    [SerializeField] private int width;
    [SerializeField] private int height;

    [SerializeField] private int pieceRow;
    [SerializeField] private int pieceCol;

    [SerializeField] private Camera mainCamera;

    // Start is called before the first frame update
    void Start()
    {
        // resize image
        Texture2D imageResize = ResizeImage(rawImage);
        
        //GameObject obj = Instantiate(piece);
        //SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        //sr.sprite = Sprite.Create(imageRes, new UnityEngine.Rect(0,0,rawImage.width,rawImage.height),Vector2.zero);


        // make image for piece
        Texture2D[] images = TrimImage(imageResize);

        // make piece
        //Texture2D[] images = {testImage1, testImage2, testImage3, imageResize};
        CreatePiece(images);
    }

    // function for resizing raw image
    private Texture2D ResizeImage(Texture2D image)
    {
        // change texture2D to mat
        Mat imageMat = new Mat(image.height,image.width,CvType.CV_8UC4);
        Utils.texture2DToMat(image,imageMat);
        
        // resize mat
        Mat matResize = new Mat(height,width,CvType.CV_8UC4);
        Imgproc.resize(imageMat,matResize,matResize.size());
        
        // change mat to texture2D
        Texture2D imageResize = new Texture2D(matResize.cols(),matResize.rows(),TextureFormat.RGBA32,false);
        Utils.matToTexture2D(matResize, imageResize);

        //return imageResize;
        return imageResize;
    }
    

    private Texture2D[] TrimImage(Texture2D image)
    {
        int pieceHeight = height / pieceRow;
        int pieceWidth = width / pieceCol;
        Texture2D[] images = new Texture2D[pieceRow * pieceCol];

        // change texture2D to mat
        Mat imageMat = new Mat(height, width, CvType.CV_8UC4);
        Utils.texture2DToMat(image, imageMat);
        
        // create images for pieces
        for (int i = 0; i < pieceCol; i++)
        {
            for (int j = 0; j < pieceRow; j++)
            {
                // get submatrix
                Mat pieceMat = new Mat(pieceHeight,pieceWidth,CvType.CV_8UC4);
                imageMat.rowRange(new Range(j*pieceHeight,(j+1)*pieceHeight)).colRange(new Range(i*pieceWidth,(i+1)*pieceWidth)).copyTo(pieceMat);
                
                // change mat to texture2D
                Texture2D imagePiece = new Texture2D(pieceMat.cols(), pieceMat.rows(), TextureFormat.RGBA32, false);
                Utils.matToTexture2D(pieceMat, imagePiece);
                images[i + j * pieceRow] = imagePiece;
            }
        }
        
        // return images for pieces
        return images;
    }

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
