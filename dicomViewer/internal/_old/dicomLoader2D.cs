using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;



public class dicomLoader2D: MonoBehaviour 
{
    public Material sliceMaterial;
    public float size = 10f;
    public float valueMod = 1f;
    public int stretchTextureOverlap = 2; //how many pixels to represent each slice on the dynamic stretched textures

    public string dicomPath;

    //List<MeshRenderer> frames = new List<MeshRenderer>();
    List<Texture2D> textures = new List<Texture2D>(); //a list of every texture created so they can all be destroyed easily from one list
    List<Material> mats = new List<Material>(); //a list of every material for easy modification

    void Start()
    {
        if (stretchTextureOverlap < 1)
            stretchTextureOverlap = 1; //we need at least 1 pixel of height to represent each slice on the stretched texture
        loadFrame(0, dicomPath);
    }

    void OnValidate()
    {
        foreach (Material m in mats)
            m.SetFloat("_Mod", valueMod);
    }


    public bool loadFrame(int frame, string dirPath) //load a 3D frame, not to be confused with a slice
    {
        // find out how may slices it has
        var info = new DirectoryInfo(dirPath);
        FileInfo[] fileInfo = info.GetFiles();
        List<FileInfo> slices = new List<FileInfo>();
        foreach (FileInfo file in fileInfo)
        {
            if (file.Name.EndsWith(".png") ||
                        file.Name.EndsWith(".jpg") ||
                        file.Name.EndsWith(".jpeg") ||
                        file.Name.EndsWith(".tif") ||
                        file.Name.EndsWith(".bmp")
                )
               slices.Add(file);
        }

        //load all the slice textures
        List<Texture2D> sliceTextures = new List<Texture2D>();
        foreach (FileInfo s in slices)
        {
            //load each texture.
            Texture2D tex = new Texture2D(2, 2);
            tex.wrapMode = TextureWrapMode.Clamp;

            byte[] pngBytes = File.ReadAllBytes(s.FullName);
            if (!tex.LoadImage(pngBytes))        // Load data into the texture.
                return false;
            sliceTextures.Add(tex);
            textures.Add(tex);  //keep track of all textures so we can destroy them easily if we want to load a different dicom
        }

        //load the frame - which is a model containing the slices on each face
        createFrameMesh("frame_" + frame.ToString(), sliceTextures).transform.Rotate(180,0,0) ;

        valueMod = .1f; //start off kind of transparent
        OnValidate();

        return true;
    }



    GameObject createFrameMesh(string objectName, List<Texture2D> sliceTextures)
    {
        int dimensions = 3;
        int sliceCount = sliceTextures.Count;
        int faceCount = (sliceCount * dimensions) ;// +dimensions - 1; //the + dimensions - 1 is the end cap
        float sliceSize = 1f / (float)sliceCount;
        float subSize = size * sliceSize * (float)(sliceCount -1) ;  //these are slightly shortened because the original y slices doesn't use an endcap
        Vector3[] verts = new Vector3[4 * faceCount ]; //4 verts in a quad * slices * dimensions  
        Vector2[] uvs = new Vector2[4 * faceCount];
        Vector3[] normals = new Vector3[4 * faceCount]; //normals are necessary for the transparency shader to work (since it uses it to calculate camera facing)
        List<int[]> submeshes = new List<int[]>(); //the triangle list(s)
        Material[] materials = new Material[faceCount];

        //create the slices along y , the main slices
        for (int y = 0; y < sliceCount; y++ )
        {
            int v = y * 4;
            float yPos = sliceSize * (float)y * size;
            //yPos += 10; //TEMP!!
            verts[v + 0] = new Vector3(0, yPos , 0); //top left
            verts[v + 1] = new Vector3(0, yPos, size); //top right
            verts[v + 2] = new Vector3(size, yPos, size); //bottom right
            verts[v + 3] = new Vector3(size, yPos, 0); //bottom left
            normals[v + 0] = new Vector3(0, 1, 0); 
            normals[v + 1] = new Vector3(0, 1, 0);
            normals[v + 2] = new Vector3(0, 1, 0); 
            normals[v + 3] = new Vector3(0, 1, 0);
            uvs[v + 0] = new Vector2(0, 0);
            uvs[v + 1] = new Vector2(0, 1);
            uvs[v + 2] = new Vector2(1, 1);
            uvs[v + 3] = new Vector2(1, 0);
      
            int[] tris = new int[6];
            tris[0] = v + 0; //1st tri starts at top left
            tris[1] = v + 1;
            tris[2] = v + 2;
            tris[3] = v + 2; //2nd triangle begins here
            tris[4] = v + 3;
            tris[5] = v + 0; //ends at bottom right       
            submeshes.Add(tris);

            //every face has a separate material/texture     
            materials[y] = new Material(sliceMaterial);
            materials[y].SetTexture("_MainTex",sliceTextures[y]);
            mats.Add(materials[y]); //store for any modifications later
        }

        //create the slices along z, the side slices      
        for (int z = 0; z < sliceCount ; z++) //the <= accounts for the end cap
        {
            int v = (z * 4) + (sliceCount * 4); //the slicecount * 4 here must take into account the previous verts set for the existing slices                
            float zPos = sliceSize * (float)z * size;
            //zPos += 10; //temp
            verts[v + 0] = new Vector3(0, subSize, zPos); //top left
            verts[v + 1] = new Vector3(size, subSize, zPos); //top right
            verts[v + 2] = new Vector3(size, 0, zPos); //bottom right
            verts[v + 3] = new Vector3(0, 0, zPos); //bottom left
            normals[v + 0] = new Vector3(0, 0, 1);
            normals[v + 1] = new Vector3(0, 0, 1);
            normals[v + 2] = new Vector3(0, 0, 1);
            normals[v + 3] = new Vector3(0, 0, 1);
            uvs[v + 3] = new Vector2(0, 0);
            uvs[v + 0] = new Vector2(0, 1);
            uvs[v + 1] = new Vector2(1, 1);
            uvs[v + 2] = new Vector2(1, 0);

            int[] tris = new int[6];
            tris[0] = v + 0; //1st tri starts at top left
            tris[1] = v + 1;
            tris[2] = v + 2;
            tris[3] = v + 2; //2nd triangle begins here
            tris[4] = v + 3;
            tris[5] = v + 0; //ends at bottom right       
            submeshes.Add(tris);

            //every face has a separate material/texture     
            materials[z + sliceCount] = new Material(sliceMaterial);
            materials[z + sliceCount].SetTexture("_MainTex", createInterpolatedTexture(sliceTextures, sliceSize * (float)z, true));
            mats.Add(materials[z + sliceCount]); //store for any modifications later
        }

        //create the slices along x, the depth slices      
        for (int x = 0; x < sliceCount; x++) //the <= accounts for the end cap
        {
            int v = (x * 4) + (sliceCount * 2 * 4); //the slicecount * 4 here must take into account the previous verts set for the existing slices, and the 2 dimensions                
            float xPos = sliceSize * (float)x* size;
            //xPos += 10f; //TEMP
            verts[v + 0] = new Vector3(xPos, subSize, 0); //top left
            verts[v + 1] = new Vector3(xPos, subSize, size); //top right
            verts[v + 2] = new Vector3(xPos, 0, size); //bottom right
            verts[v + 3] = new Vector3(xPos, 0, 0); //bottom left
            normals[v + 0] = new Vector3(1, 0, 0);
            normals[v + 1] = new Vector3(1, 0, 0);
            normals[v + 2] = new Vector3(1, 0, 0);
            normals[v + 3] = new Vector3(1, 0, 0);
            uvs[v + 3] = new Vector2(0, 0);
            uvs[v + 0] = new Vector2(0, 1);
            uvs[v + 1] = new Vector2(1, 1);
            uvs[v + 2] = new Vector2(1, 0);

            int[] tris = new int[6];
            tris[0] = v + 0; //1st tri starts at top left
            tris[1] = v + 1;
            tris[2] = v + 2;
            tris[3] = v + 2; //2nd triangle begins here
            tris[4] = v + 3;
            tris[5] = v + 0; //ends at bottom right       
            submeshes.Add(tris);

            //every face has a separate material/texture     
            materials[x + sliceCount + sliceCount] = new Material(sliceMaterial);
            materials[x + sliceCount + sliceCount].SetTexture("_MainTex", createInterpolatedTexture(sliceTextures, sliceSize * (float)x, false));
            mats.Add(materials[x + sliceCount + sliceCount]); //store for any modifications later
        }

        GameObject g = new GameObject(objectName);

        MeshRenderer r = g.AddComponent<MeshRenderer>();
        Mesh m = g.AddComponent<MeshFilter>().mesh;
        m.vertices = verts;
        m.uv = uvs;
        m.normals = normals;

        m.subMeshCount = faceCount;        
        for(int s = 0; s < faceCount; s++)
        {
            m.SetTriangles(submeshes[s], s);
        }

 
        r.materials = materials;

        m.RecalculateBounds();
        return g;
    }


    Texture2D createInterpolatedTexture(List<Texture2D> source, float slicePos, bool XYorZY) //slicePos here is a % along the texture
    {
        if (source.Count == 0)
            return null;
      
        //gather the pixels
        int w = source[0].width;
        int h = source.Count;
        int pixelSlicePos = Mathf.RoundToInt(slicePos * (float)w);
        Color[] colors = new Color[ w* h * stretchTextureOverlap];

        for (int y = 0; y < h; y++) //textures hold the data as LEFT to RIGHT, BOTTOM to TOP, we compensate for this in UV's to keep things simple in code
        {
            for (int s = 0; s < stretchTextureOverlap; s++ )
            {
                for (int x = 0; x < w; x++)
                {
                    if (XYorZY)
                        colors[ (s * w)+(y * w) + x] = source[y].GetPixel(x, pixelSlicePos);  //translate the color from the horizontal slice into the XY slice
                    else
                        colors[(s * w) + (y * w) + x] = source[y].GetPixel(pixelSlicePos, x);  
                }
            }
        }

        Texture2D t = new Texture2D(source[0].width, source.Count);
        t.wrapMode = TextureWrapMode.Clamp;
        //t.filterMode = FilterMode.Point;

        t.SetPixels(colors);
        t.Apply(); //send the data to the gpu

        return t;
    }



}
