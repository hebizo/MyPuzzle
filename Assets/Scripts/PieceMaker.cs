using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.CoreModule;

public class PieceMaker : MonoBehaviour
{
    [SerializeField] private int pieceNum;
    
    [SerializeField] private Texture2D rawImage;
    [SerializeField] private GameObject piece;

    private Image _image;
    private Texture2D[] _images;

    private int _width = 810;
    private int _height = 540;

    [SerializeField] private int pieceRow;
    [SerializeField] private int pieceCol;

    [SerializeField] private Camera mainCamera;

    // Start is called before the first frame update
    void Start()
    {
        // resize image
        //Texture2D imageResize = ResizeImage(rawImage, _height, _width);
        
        //GameObject obj = Instantiate(piece);
        //SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        //sr.sprite = Sprite.Create(imageRes, new UnityEngine.Rect(0,0,rawImage.width,rawImage.height),Vector2.zero);


        // make image for piece
        //Texture2D[] images = TrimImage(imageResize);

        // make piece
        //Texture2D[] images = {testImage1, testImage2, testImage3, imageResize};
        _images = LoadPieceImage();
        CreatePiece(_images);
    }

    // function for resizing raw image
    private Texture2D ResizeImage(Texture2D image, int height, int width)
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
        int pieceHeight = _height / pieceRow;
        int pieceWidth = _width / pieceCol;
        Texture2D[] images = new Texture2D[pieceRow * pieceCol];

        // change texture2D to mat
        Mat imageMat = new Mat(_height, _width, CvType.CV_8UC4);
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
        float posX = -6.5f;
        float intervalX = 13f / (images.Length - 1);
        foreach (Texture2D img in images)
        {
            Vector3 pos = new Vector3(posX, -4.0f, 0f);
            posX += intervalX;
            GameObject pieceIns = Instantiate(piece, pos, Quaternion.identity);
            
            // attach each image to piece
            SpriteRenderer sr = pieceIns.GetComponent<SpriteRenderer>();
            sr.sprite = Sprite.Create(img, new UnityEngine.Rect(0,0,img.width,img.height), new Vector2(0.5f,0.5f));
            
            // attach main camera to piece
            pieceIns.GetComponent<Piece>().cam = mainCamera;
        }
    }

    private Texture2D[] LoadPieceImage()
    {
        // calculate piece size
        int pieceHeight = _height / pieceRow;
        int pieceWidth = _width / pieceCol;
        
        Texture2D[] images = Resources.LoadAll<Texture2D>("Images/Pieces");
        images = Black2Transparent(images);
        
        // resize piece
        for (int i = 0; i < images.Length; i++)
        {
            images[i] = ResizeImage(images[i], pieceHeight, pieceWidth);
        }
        
        return images;
    }

    private Texture2D[] Black2Transparent(Texture2D[] images)
    {
        Texture2D[] imageRet = new Texture2D[images.Length];
        for (int i = 0; i < images.Length; i++)
        {
            Texture2D image = images[i];
            
            Mat imageMat = new Mat(image.height, image.width, CvType.CV_8UC4);
            Utils.texture2DToMat(image, imageMat);

            for (int y = 0; y < imageMat.rows(); y++)
            {
                for (int x = 0; x < imageMat.cols(); x++)
                {
                    double[] pixel = imageMat.get(y, x);
                    
                    // change black pixel to transparent
                    if (pixel[0] == 0 && pixel[1] == 0 && pixel[2] == 0)
                    {
                        pixel[3] = 0;
                        imageMat.put(y, x, pixel);
                    }
                }
            }
            
            Texture2D imagePiece = new Texture2D(imageMat.cols(), imageMat.rows(), TextureFormat.RGBA32, false);
            Utils.matToTexture2D(imageMat, imagePiece);
            imageRet[i] = imagePiece;
        }

        return imageRet;
    }
}
