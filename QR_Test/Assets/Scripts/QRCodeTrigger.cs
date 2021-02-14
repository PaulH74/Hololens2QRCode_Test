using System.Collections.Generic;
using UnityEngine;

namespace Hydac_QR
{
    public class QRCodeTrigger : MonoBehaviour
    {
        // Public Attributes
        public GameObject[] panelUIs;

        // Private Attributes
        private Dictionary<string, int> _MenuLookUp;

        private void Start()
        {
            PopulateMenuLookupTable();
            DisableUIMenus();
        }

        private void DisableUIMenus()
        {
            foreach (GameObject panel in panelUIs)
            {
                panel.SetActive(false);
            }    
        }

        private void PopulateMenuLookupTable()
        {
            const string PREFACE = "Menu_";                     // NOTE: This preface may need to be changed / updated according to Hydac
            string keyName = "";

            _MenuLookUp = new Dictionary<string, int>();
            
            for (int i = 0; i < panelUIs.Length; i++)
            {
                keyName = PREFACE + "0" + (i + 1).ToString();   // NOTE: This numbering system may need to be changed / updated according to Hydac
                _MenuLookUp.Add(keyName, i);
            }
        }
        
        /// <summary>
        /// Triggers the appropriate UI Panel (GameObject) and syncs its transform to the QR Code associated with the trigger.
        /// </summary>
        /// <param name="menuName"></param>
        /// <param name="qrCodeTF"></param>
        public void TriggerMenu(string menuName, Transform qrCodeTF)
        {
            int index = _MenuLookUp[menuName];
            
            panelUIs[index].SetActive(true);
            //panelUIs[index].transform.SetParent(qrCodeTF);
            //panelUIs[index].transform.localPosition = Vector3.zero;
            //panelUIs[index].transform.localRotation = Quaternion.identity;
            // Offset rotation
            //panelUIs[index].transform.Rotate(Vector3.forward * 180f);
            //panelUIs[index].transform.Translate(Vector3.forward * 0.1f);
        }
    }
}
