using UnityEngine;
using TMPro;
using Hashneer.BitcoinMining;
using System;
using System.Collections.Generic;

namespace Hashneer.Tests 
{
    public class Test : MonoBehaviour
    {
        public TextMeshProUGUI nonce;
        public TextMeshProUGUI latestBlock;
        public GameObject panel;

        public TextMeshProUGUI displayBlockText;
        public GameObject blockPrefab;
        public Transform contentTransform;

        private void Start()
        {
            SetTextes();
            panel.SetActive(false);
            ShowBlocks();
        }

        private void SetTextes()
        {
            nonce.text = Blockchain.Instance.Difficulty.ToString();
            latestBlock.text = Blockchain.Instance.GetLatestBlock().ToString();
        }

        public void ActivatePanel(bool isActive)
        {
            panel.SetActive(isActive);

            if (isActive)
            {
                ShowBlocks();
            }
        }

        public void ShowBlocks()
        {
            GameObject prefab;
            foreach (Block block in Blockchain.Instance.GetBlockList())
            {
                prefab = Instantiate(blockPrefab, contentTransform);

                BlockUI blockUI = prefab.GetComponent<BlockUI>();

                blockUI.currentHash.text = BitConverter.ToString(block.Hash).Replace("-", "");
                blockUI.previousHash.text = BitConverter.ToString(block.PreviousHash).Replace("-", "");
                blockUI.timeStamp.text = block.Timestamp.ToString();
                blockUI.nonce.text = block.Nonce.ToString();
            }

            prefab = Instantiate(blockPrefab, contentTransform);
        }
    }
}