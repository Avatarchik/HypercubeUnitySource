using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//this script does nothing.
//It's purpose is so that the hypercube can query if an appropriate canvas exists in the scene.
//If it doesn't exist, it will create one.

[ExecuteInEditMode]
public class hypercubeCanvas : MonoBehaviour 
{

    public bool flipX = false;
    public float sliceOffsetX = 0;
    public float sliceOffsetY = 0;
    int sliceCount = 12; //this is given by the attached hypercube
    public float sliceWidth = 600;
    public float sliceHeight = 53;
    public float zPos = .01f;
    public GameObject sliceMesh;

    public List<Material> canvasMaterials = new List<Material>(); 



    void OnValidate()
    {
        if (Screen.width < 1 || Screen.height < 1)
            return; //wtf.

        if (sliceCount < 1)
            sliceCount = 1;

        if (!sliceMesh)
            return;

        updateMesh(sliceCount);
        resetTransform();       
    }

    void Update()
    {
        if (transform.hasChanged)
        {
            resetTransform();
        }
    }

 
    public void flip()
    {
        flipX = !flipX;
        updateMesh(sliceCount);
    }
    public void sliceHeightUp()
    {
        sliceHeight += .2f;
        updateMesh(sliceCount);
    }
    public void sliceHeightDown()
    {
        sliceHeight -= .2f;
        updateMesh(sliceCount);
    }
    public void nudgeUp()
    {
        sliceOffsetY += .2f;
        updateMesh(sliceCount);
    }
    public void nudgeDown()
    {
        sliceOffsetY -= .2f;
        updateMesh(sliceCount);
    }
    public void nudgeLeft()
    {
        sliceOffsetX -= 1f;
        updateMesh(sliceCount);
    }
    public void nudgeRight()
    {
        sliceOffsetX += 1f;
        updateMesh(sliceCount);
    }
    public void widthUp()
    {
        sliceWidth += 1f;
        updateMesh(sliceCount);
    }
    public void widthDown()
    {
        sliceWidth -= 1f;
        updateMesh(sliceCount);
    }
    public void setPreset1()
    {
        sliceHeight = 120f;
        updateMesh(sliceCount);
    }
    public void setPreset2()
    {
        sliceHeight = 68f;
        updateMesh(sliceCount);
    }


    void resetTransform() //size the mesh appropriately to the screen
    {
        if (!sliceMesh)
            return;

        float aspectRatio = (float)Screen.width / (float)Screen.height;
        float xPixel = 1f / (float)Screen.width;
        float yPixel = 1f / (float)Screen.height;
        sliceMesh.transform.localScale = new Vector3(sliceWidth * xPixel * aspectRatio, (float)sliceCount * sliceHeight * 2f * yPixel, 1f); //the *2 is because the view is 2 units tall

        sliceMesh.transform.localPosition = new Vector3(xPixel * sliceOffsetX, (yPixel * sliceOffsetY * 2f) - 1f, zPos); //the 1f is the center vertical on the screen, the *2 is because the view is 2 units tall

    }

    //this is part of the code that tries to map the player to a particular screen (this appears to be very flaky in Unity)
    public void setToDisplay(int displayNum)
    {
        if (displayNum == 0 || displayNum >= Display.displays.Length)
            return;

        GetComponent<Camera>().targetDisplay = displayNum;
        Display.displays[displayNum].Activate();
    }


    public void setTone(float value)
    {   
        if (!sliceMesh)
            return;

        MeshRenderer r = sliceMesh.GetComponent<MeshRenderer>();
        if (!r)
            return;
        foreach (Material m in r.sharedMaterials)
        {
            m.SetFloat("_Mod", value);
        }
    }

    
    public void updateMesh(int _sliceCount)
    {
        sliceCount = _sliceCount;
        if (canvasMaterials.Count == 0)
        {
            Debug.LogError("Canvas materials have not been set!  Please define what materials you want to apply to each slice in the hypercubeCanvas component.");
            return;
        }

        if (sliceCount < 1 )
        {
            sliceCount = 1;
            return;
        }
        if (sliceHeight < 1)
        {
            sliceHeight = 1;
            return;
        }
        if (sliceWidth < 1)
        {
            sliceWidth = 1;
            return;
        }

        if (sliceCount > canvasMaterials.Count)
        {
            Debug.LogWarning("Can't add more than " + canvasMaterials.Count + " slices, because only " + canvasMaterials.Count + " canvas materials are defined.");
            sliceCount = canvasMaterials.Count;
            return;
        }

        Vector3[] verts = new Vector3[4 * sliceCount]; //4 verts in a quad * slices * dimensions  
        Vector2[] uvs = new Vector2[4 * sliceCount];
        Vector3[] normals = new Vector3[4 * sliceCount]; //normals are necessary for the transparency shader to work (since it uses it to calculate camera facing)
        List<int[]> submeshes = new List<int[]>(); //the triangle list(s)
        Material[] faceMaterials = new Material[sliceCount];

        //create the mesh
        float size = 1f / (float)sliceCount;
        for (int y = 0; y < sliceCount; y++)
        {
            int v = y * 4;
            float yPos =  (float)y * size;

            verts[v + 0] = new Vector3(-1f, yPos + size, 0f); //top left
            verts[v + 1] = new Vector3(1f, yPos + size, 0f); //top right
            verts[v + 2] = new Vector3(1f, yPos, 0f); //bottom right
            verts[v + 3] = new Vector3(-1f, yPos, 0f); //bottom left
            normals[v + 0] = new Vector3(0, 0, 1);
            normals[v + 1] = new Vector3(0, 0, 1);     
            normals[v + 2] = new Vector3(0, 0, 1);
            normals[v + 3] = new Vector3(0, 0, 1);

            if (!flipX)
            {
                uvs[v + 0] = new Vector2(1, 0);
                uvs[v + 1] = new Vector2(0, 0);
                uvs[v + 2] = new Vector2(0, 1);
                uvs[v + 3] = new Vector2(1, 1);
            }
            else
            {
                uvs[v + 0] = new Vector2(0, 0);
                uvs[v + 1] = new Vector2(1, 0);
                uvs[v + 2] = new Vector2(1, 1);
                uvs[v + 3] = new Vector2(0, 1);
            }



            int[] tris = new int[6];
            tris[0] = v + 0; //1st tri starts at top left
            tris[1] = v + 1;
            tris[2] = v + 2;
            tris[3] = v + 2; //2nd triangle begins here
            tris[4] = v + 3;
            tris[5] = v + 0; //ends at bottom right       
            submeshes.Add(tris);

            //every face has a separate material/texture     
            faceMaterials[y] = canvasMaterials[y];
        }


        MeshRenderer r = sliceMesh.GetComponent<MeshRenderer>();
        if (!r)
             r = sliceMesh.AddComponent<MeshRenderer>();

        MeshFilter mf = sliceMesh.GetComponent<MeshFilter>();
        if (!mf)
            mf = sliceMesh.AddComponent<MeshFilter>();

        Mesh m = mf.sharedMesh;
        if (!m)
            return; //probably some in-editor state where things aren't init.
        m.Clear();
        m.vertices = verts;
        m.uv = uvs;
        m.normals = normals;

        m.subMeshCount = sliceCount;
        for (int s = 0; s < sliceCount; s++)
        {
            m.SetTriangles(submeshes[s], s);
        }

        r.materials = faceMaterials;

        m.RecalculateBounds();
    }
	
}
