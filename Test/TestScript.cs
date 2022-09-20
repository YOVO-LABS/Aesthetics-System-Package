using UnityEngine;
using Zionverse.AestheticsSystem;

public class TestScript : MonoBehaviour
{
   
    // Start is called before the first frame update
    void Start()
    {
       AestheticManager.Init("default", OnDownloadComplete, OnDownloadFail,OnDownloading);
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void OnDownloadComplete()
    {
        Object obj = (Object)AestheticManager.GetAsset("5");
        Debug.Log(obj.name+ " testAsset");
    }

    public void OnDownloadFail()
    {
        Debug.LogError("Download Failed. Please make sure you have a stable internet connection");
    }

    public void OnDownloading(float progress)
    {
         Debug.Log("percent complete " + progress);
    }
}
