using UnityEngine;
using UnityEngine.UI;
using System;

namespace SuperUltra.Container
{

    [Serializable]
    public static class UserData
    {
        public static string WalletAddress;
        public static string ObjectId;
        public static string Token;
        public static string AdsCount;
        public static string KOL;
        public static string playFabId;
        public static string userName;
        public static Texture2D profilePic;
        /// <summary> a texture ready to send or sent to update user profile. </summary>
        public static Texture2D pendingProfilePic;
        public static string email;
        public static string playFabSessionTicket;
        public static int totalTokenNumber;
        public static int pointsInCurrentRank;
        public static int pointsToNextRank;
        public static int rankLevel;
        public static string rankTitle;
        public static NFTItem[] nftItemList;

        static UserData()
        {
            ContainerInterface.OnGetNFTItemList += OnGetNFTItemList;
        }

        static NFTItem[] OnGetNFTItemList()
        {
            NFTItem[] list = new NFTItem[nftItemList.Length];
            nftItemList.CopyTo(list, 0);
            return list;
        }

        public static void ActivateNFTItem(NFTItem item)
        {
            if (item.type == NFTItem.ItemType.Cosmetic)
            {
                for (int i = 0; i < nftItemList.Length; i++)
                {
                    bool isCosmetic = nftItemList[i].type == NFTItem.ItemType.Cosmetic;
                    bool isTarget = item.id == nftItemList[i].id;
                    if (isCosmetic)
                    {
                        nftItemList[i].isActive = isTarget;
                    }
                    if (isTarget)
                    {
                        PlayerPrefs.SetInt(Config.KEY_NFT_ITEM, item.id);
                    }
                }
            }
            // TODO : Non cosmetic items
        }

        public static NFTItem GetNFTItemById(int id)
        {
            if (id < 0)
                return null;
            for (int i = 0; i < nftItemList.Length; i++)
            {
                bool isTarget = id == nftItemList[i].id;
                if (isTarget)
                {
                    return nftItemList[i];
                }
            }
            return null;
        }

        public static void DeactivateNFTItem(NFTItem item)
        {
            if (item.type == NFTItem.ItemType.Cosmetic)
            {
                for (int i = 0; i < nftItemList.Length; i++)
                {
                    bool isTarget = item.id == nftItemList[i].id;
                    if (isTarget)
                    {
                        nftItemList[i].isActive = false;
                    }
                }
            }
        }

        public static void ClearData()
        {
            playFabId = "";
            userName = "";
            profilePic = null;
            email = "";
            totalTokenNumber = -1;
            WalletAddress = "";
            pointsInCurrentRank = -1;
            pointsToNextRank = -1;
            rankLevel = -1;
            rankTitle = "";
        }

        
    }

}
