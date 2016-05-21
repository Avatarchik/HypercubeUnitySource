using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;

//this class:
// -connects to osc
// -ferries the input from osc directly to any touchscreenTargets that it knows about.


[Serializable]
public class touch
{
    public touch(int _id, Vector2 _pos)
    {
        id = _id;
        position = _pos;
        size = new Vector2(1f, 1f);
    }

    public int id;
    public Vector2 position;
    public Vector2 size;

    public void set(Vector2 _pos, Vector2 _size) //having this method avoids an extra hash lookup
    {
        position = _pos;
        size = _size;
    }
}

public class TouchscreenServer : MonoBehaviour
{
    public int multicastPort = 7500;
    public int directPort = 8000;
    public Boolean debug;
    public Boolean multicast = false;
    private Boolean lastMulticastSetting;

    public List<touchscreenTarget> targets;  //these will get updates on every event coming from osc

    UdpClient udpClient;
    IPEndPoint endPoint;
    Osc.Parser osc = new Osc.Parser();
    Vector2 position = new Vector2(); //scratch variables
    Vector2 difference = new Vector2();
    Vector2 size = new Vector2();

    public Dictionary<int, touch> touches; //convenience, if you simply want to access touches as an array.

    void Start()
    {
        touches = new Dictionary<int, touch>();

        targets.AddRange(GameObject.FindObjectsOfType<touchscreenTarget>()); //automatically add any touch screen targets in the scene so that they will get notified of touch events
       
        setListenPort();
    }

    void setListenPort()
    {     
        if (multicast)
        {
            endPoint = new IPEndPoint(IPAddress.Any, multicastPort);
            udpClient = new UdpClient(endPoint);
        }
        else
        {
            endPoint = new IPEndPoint(IPAddress.Any, directPort);
            udpClient = new UdpClient(endPoint);
        }
        lastMulticastSetting = multicast;
    }


    void Update()
    {

        if (multicast != lastMulticastSetting)
            setListenPort();

        if (udpClient == null)
            return;

        while (udpClient.Available > 0)
        {
            osc.FeedData(udpClient.Receive(ref endPoint));
        }

        while (osc.MessageCount > 0)
        {
            Osc.Message msg = osc.PopMessage();
            if (msg.path == "/touchDown")
            {

                int id = (int)msg.data[0];
                position.x = float.Parse(msg.data[1].ToString());
                position.y = float.Parse(msg.data[2].ToString());

                touches.Add(id, new touch(id, position));

                foreach (touchscreenTarget t in targets)
                {
                    t.onTouchDown(id, position);
                }            
            }
            else if (msg.path == "/touchUp")
            {
                int id = (int)msg.data[0];
                position.x = float.Parse(msg.data[1].ToString());
                position.y = float.Parse(msg.data[2].ToString());

                touches.Remove(id);

                foreach (touchscreenTarget t in targets)
                {
                    t.onTouchUp(id, position);
                }              
            }
            else if (msg.path == "/touchMove")
            {
                int id = (int)msg.data[0];
                position.x = float.Parse(msg.data[1].ToString());
                position.y = float.Parse(msg.data[2].ToString());
                size.x = float.Parse(msg.data[3].ToString());
                size.y = float.Parse(msg.data[4].ToString());

                difference.x = touches[id].position.x - position.x;
                difference.y = touches[id].position.y - position.y;

                touches[id].set(position, size);

                foreach (touchscreenTarget t in targets)
                {
                    t.onTouchRelativeMoved(id, difference);
                    t.onTouchMoved(id, position, size);
                }              
            }

            if (debug)
            {
                string outstring = "";
                foreach (var v in msg.data)
                    outstring += v.ToString() + "\t";
                Debug.Log("OSC: " + outstring );
            }
        }
    }


}



