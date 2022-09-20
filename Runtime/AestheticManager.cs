using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading.Tasks;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace Zionverse.AestheticsSystem
{
    public class AestheticManager 
    {
        #region Static Variables
        private static Dictionary<string, Object> objectsDictionary = new Dictionary<string, Object>();  
        private static UnityAction OnDownloadSuccessAction;
        private static UnityAction OnDownloadFailedAction;
        private static UnityAction<float> OnDownloadingAction;
        #endregion

        #region Private Variables
        private AsyncOperationHandle<IList<Object>> assetsList;
        private AsyncOperationHandle<long> currentAddressableFileDownloadSize;
        private string addressablePath;
        #endregion

        //Initializes the aesthetic system 
        public static void Init(string aestheticJsonData, UnityAction onDownloadSuccessAction,UnityAction onDownloadFailAction, UnityAction<float> onDownloadingAction)
        {
            OnDownloadSuccessAction = onDownloadSuccessAction;
            OnDownloadFailedAction = onDownloadFailAction;
            OnDownloadingAction = onDownloadingAction;
            AestheticManager aestheticManager = new AestheticManager(); //Find more optimized solution
            aestheticManager.ParseAestheticJSONData(aestheticJsonData);
        }

        //Parse the json data received from the communication module
        private void ParseAestheticJSONData(string aestheticJsonData)
        {
            addressablePath = aestheticJsonData;
            CheckForDownload();
        }

        private async void CheckForDownload()
        {
            Addressables.ClearDependencyCacheAsync("default");
            currentAddressableFileDownloadSize = Addressables.GetDownloadSizeAsync(addressablePath);
            while(!currentAddressableFileDownloadSize.IsDone)
                await Task.Yield();
            DownloadAssetsBundle();
            // float assetSize = currentAddressableFileDownloadSize.Result / 1000000f;
            // double size = Math.Round((float)assetSize, 2);
            //Debug.Log("size b4 download: " + assetSize + " MB");
        }


        //Downloads the asset bundle from remote location
        private async void DownloadAssetsBundle()
        {
            if (currentAddressableFileDownloadSize.Result > 0)
            {
                var assets = Addressables.DownloadDependenciesAsync(addressablePath);
                assets.Completed += OnDownloadCompleted;
                //assets.Destroyed += OnDownloadFail;
                while (!assets.IsDone)
                {
                    var status = assets.GetDownloadStatus();
                    float progressPercentage = status.Percent * 100;
                    OnDownloadingAction?.Invoke(progressPercentage);
                    if(Application.internetReachability == NetworkReachability.NotReachable)
                    {
                        OnDownloadFailedAction?.Invoke();
                        //OnDownloadFailed();
                        break;
                    }
                    //Debug.Log("percent complete " + status.DownloadedBytes /1000000);
                    await Task.Yield();

                }
            }
            else
                StoreAssets();
        }

        //Called on asset download completed
        private void OnDownloadCompleted(AsyncOperationHandle assets)
        {
            if (assets.IsDone)
                 StoreAssets();
        }


        //Called on download failed
        private void OnDownloadFailed()
        {
           
        }

        //Stores all the assets inside a dictionary 
        private async void StoreAssets()
        {
            assetsList = Addressables.LoadAssetsAsync<Object>(addressablePath, null);
            while(!assetsList.IsDone)
                await Task.Yield();
            foreach (Object asset in assetsList.Result)
            {
                //Debug.Log(asset.name + "  " + asset.GetType());
                if (!objectsDictionary.ContainsValue(asset))
                    objectsDictionary.Add(asset.name, asset);
            }
            OnDownloadSuccessAction?.Invoke();
        }

        /// <summary>
        /// Returns the object for the specified asset from the bundle.You need to do an explicit cast to convert the object
        /// </summary>
        /// <returns>Returns an object based on the asset name if it exists.</returns>
        public static Object GetAsset(string assetName)
        {
            if(objectsDictionary.ContainsKey(assetName))
                return objectsDictionary[assetName];
            else
                Debug.LogError("The given key was not found in the dictionary.Please make sure that the asset exist");
            return null;
        }
    }
}

