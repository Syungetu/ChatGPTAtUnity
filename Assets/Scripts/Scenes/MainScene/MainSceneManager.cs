using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using com.studiocross.InputTextManager;
using com.studiocross.ValueTextManager;
using com.studiocross.ChatGPT;

/// <summary>
/// メインシーンの管理
/// </summary>
public class MainSceneManager : MonoBehaviour
{
    [Header("表情")]
    /// <summary> 喜び数値 </summary>
    public ValueTextManager _JoyText;
    /// <summary> 楽しい数値 </summary>
    public ValueTextManager _FunText;
    /// <summary> 怒り数値 </summary>
    public ValueTextManager _AngerText;
    /// <summary> 悲しみ数値 </summary>
    public ValueTextManager _SedText;

    [Header("会話")]
    /// <summary> 会話テキスト </summary>
    public ValueTextManager _TalkText;

    [Header("入力")]
    /// <summary> 入力テキスト </summary>
    public InputTextManager _SpeakText;

    /// <summary> ChatGPTを管理しているクラス </summary>
    private ChatGPTManager _chatGPTManager = null;

    // Start is called before the first frame update
    void Start()
    {
        if (_chatGPTManager == null)
        {
            // 初期化時にChatGPTに送るデータも初期化する
            _chatGPTManager = new ChatGPTManager();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// テキストを送信する
    /// </summary>
    /// <param name="text"></param>
    public void SetSendText(string text)
    {
        StartCoroutine(CoSendData(text));
    }

    /// <summary>
    /// 非同期でデータを送信する
    /// </summary>
    /// <returns></returns>
    private IEnumerator CoSendData(string text)
    {
        // 送信するテキストを設定
        _chatGPTManager.SetRecordText(text);
        // テキストを送信する
        yield return _chatGPTManager.SetSendData(GetReceiveDataCallback);
    }

    /// <summary>
    /// 受信後のコールバック処理
    /// </summary>
    private void GetReceiveDataCallback(ChatGPTData.ShowData showData)
    {
        SetFacialExpressionValue(showData.emotion);
        SetConversationText(showData.content);
    }

    /// <summary>
    /// 受信した表情の数値を表示する
    /// </summary>
    public void SetFacialExpressionValue(ChatGPTData.Emotion emotion)
    {
        _JoyText.SetText(emotion.Joy.ToString());
        _FunText.SetText(emotion.Fun.ToString());
        _AngerText.SetText(emotion.Anger.ToString());
        _SedText.SetText(emotion.Sad.ToString());
    }

    /// <summary>
    /// 受信したテキストを表示する
    /// </summary>
    public void SetConversationText(string content)
    {
        _TalkText.SetText(content);
    }

}
