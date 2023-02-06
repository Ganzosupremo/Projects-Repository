using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Hashneer.BitcoinMining
{
    public class Wallet : SingletonMonobehaviour<Wallet>
    {
        public double Balance { get; set; }

        public GameObject displayBalancePanel;
        public TextMeshProUGUI balance;

        protected override void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            displayBalancePanel.SetActive(false);
            balance.text = Balance.ToString();
        }

        public void DisplayBalance(bool isActive)
        {
            displayBalancePanel.SetActive(isActive);
            balance.text = Balance.ToString();
        }

        public void UpdateBalance()
        {
            balance.text = Balance.ToString();
        }
    }
}
