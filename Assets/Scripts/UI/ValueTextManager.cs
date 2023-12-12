using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace com.studiocross.ValueTextManager
{
    /// <summary>
    /// テキスト表示を管理する
    /// </summary>
    public class ValueTextManager : MonoBehaviour
    {
        /// <summary> 表示するテキストオブジェクト </summary>
        public TMP_Text _ValteText;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        /// <summary>
        /// テキストを設定する
        /// </summary>
        /// <param name="text"> 設定するテキスト </param>
        public void SetText(string text)
        {
            _ValteText.text = text;
        }
    }
}