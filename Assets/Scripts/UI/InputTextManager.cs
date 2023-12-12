using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using System;

namespace com.studiocross.InputTextManager
{
    /// <summary> 送信ボタンを押した時に処理するイベント </summary>
    [Serializable]
    public class InputTextManager_OnClickButtonEvent : UnityEvent<string>
    {
    }

    /// <summary>
    /// テキスト入力を管理する
    /// </summary>
    public class InputTextManager : MonoBehaviour
    {
        /// <summary> テキストフィールド </summary>
        public TMP_InputField _InputTextObject;
        /// <summary> 送信ボタンを押した時に処理するイベント </summary>
        public InputTextManager_OnClickButtonEvent _ButtonEvent;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        /// <summary>
        /// 送信ボタンを押した時
        /// </summary>
        public void OnClickSendButton()
        {
            if(_ButtonEvent != null)
            {
                // 入力されたテキストを送る
                _ButtonEvent.Invoke(_InputTextObject.text);
                // 入力されたテキストを削除する
                _InputTextObject.text = "";
            }
        }
    }
}