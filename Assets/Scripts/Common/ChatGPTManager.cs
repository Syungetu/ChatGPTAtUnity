using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace com.studiocross.ChatGPT
{
    /// <summary>
    /// ChatGPTの通信のデータを管理する
    /// </summary>
    public class  ChatGPTData
    {
        /// <summary> ChatGPTへ送信するデータの構造体(Json) </summary>
        [Serializable]
        public struct SendingData
        {
            /// <summary> ChatGPTのモデル名 </summary>
            public string model;
            /// <summary> 送信するテキスト </summary>
            public List<MessageData> messages;
        }

        /// <summary> 送信するテキスト </summary>
        [Serializable]
        public struct MessageData
        {
            /// <summary> ロール（出力される設定：system=ChatGPTに対しての設定 user=ユーザーからの会話 assistant=ChatGPTからの返答） </summary>
            public string role;
            /// <summary> 送信するテキスト </summary>
            public string content;
        }

        /// <summary>
        /// ロールの種類
        /// </summary>
        public enum RoleType
        {
            /// <summary> ChatGPTに対しての設定 </summary>
            system,
            /// <summary> ユーザーからの会話 </summary>
            user,
            /// <summary> ChatGPTからの返答 </summary>
            assistant
        }

        /// <summary>
        /// ロールの種類をテキストに変換する
        /// </summary>
        /// <returns></returns>
        public static string GetRoleName(RoleType roleType)
        {
            string name = "";

            switch(roleType)
            {
                case RoleType.system:
                    name = "system";
                    break;
                case RoleType.user:
                    name = "user";
                    break;
                case RoleType.assistant:
                    name = "assistant";
                    break;
            }

            return name;
        }

        /// <summary> 受信した表示するテキスト </summary>
        [Serializable]
        public struct ShowData
        {
            /// <summary> ChatGPTの返答 </summary>
            public string content;
            /// <summary> ChatGPTの表情 </summary>
            public Emotion emotion;
        }

        /// <summary> 感情値 </summary>
        [Serializable]
        public struct Emotion
        {
            /// <summary> 嬉しい </summary>
            public int Joy;
            /// <summary> 楽しい </summary>
            public int Fun;
            /// <summary> 怒り </summary>
            public int Anger;
            /// <summary> 悲しみ </summary>
            public int Sad;
        }

        /// <summary> ChatGPTから受信するデータの構造体 </summary>
        [Serializable]
        public struct ReceivingData
        {
            /// <summary> ID </summary>
            public string id;
            /// <summary> オブジェクト（型不定 </summary>
            public string @object;
            /// <summary> 生成時間 </summary>
            public long created;
            /// <summary> 生成されたメッセージ </summary>
            public Choice[] choices;
            /// <summary> 通信するときのトークン </summary>
            public Usage usage;
        }

        /// <summary> 生成されたメッセージが保存される場所 </summary>
        [Serializable]
        public struct Choice
        {
            /// <summary> 会話番号 </summary>
            public int index;
            /// <summary> 受け取ったテキスト </summary>
            public ReactionMessage message;
            /// <summary> 終了理由 </summary>
            public string finish_reason;
        }

        /// <summary>
        /// 受け取ったテキスト
        /// </summary>
        [Serializable]
        public struct ReactionMessage
        {
            /// <summary> ロール（出力される設定：system=ChatGPTに対しての設定 user=ユーザーからの会話 assistant=ChatGPTからの返答） </summary>
            public string role;
            /// <summary> ChatGPTの返答 </summary>
            public string content;
            /// <summary> ChatGPTの表情 </summary>
            public Emotion emotion;
        }

        /// <summary> 通信するときのトークン </summary>
        public struct Usage
        {
            public int prompt_tokens;
            public int completion_tokens;
            public int total_tokens;
        }
    }

    /// <summary>
    /// ChatGPTとUnityを通信を管理する
    /// </summary>
    public class ChatGPTManager
    {
        /// <summary> APIキー </summary>
        public string _ApiKey = "";
        /// <summary> OrganizationID </summary>
        public string _OrganizationID = "";
        /// <summary> CharGPTのモデル設定 </summary>
        public string _ApiModel = "gpt-3.5-turbo";
        /// <summary> CharGPT送信先URL </summary>
        public string _ApiUrl = "https://api.openai.com/v1/chat/completions";

        /// <summary> 送信するデータを保持しておく </summary>
        private List<ChatGPTData.MessageData> _messageDataList = new List<ChatGPTData.MessageData>();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ChatGPTManager()
        {
            ChatGPTData.MessageData initMessage = new ChatGPTData.MessageData();

            if(_messageDataList.Count > 0)
            {
                // すでに初期化済みだったら処理しない
                return;
            }

            // ChatGPTに初期設定を送信する
            initMessage.role = ChatGPTData.GetRoleName(ChatGPTData.RoleType.system);
            initMessage.content =
                "以下の条件に従って、疑似的な感情をもつチャットボットとしてロールプレイをします。" + 
                "以後の会話では、あなたは下記の4つの感情パラメーターを持つかのように、振る舞うものとします。" +
                "各感情パラメーターは会話を通じて変動するものとします。" +
                "現在の感情パラメーターの値を反映するように、あなたの返答のトーンや発言は変化します。" +
                "以後の会話ではまず現在の感情パラメータを出力し、その後に会話を出力してください。" +
                "出力形式は以下のjsonフォーマットとします。このフォーマット以外で会話しないでください。" +
                "{" +
                    "\"emotion\": {" +
                        "\"Joy\": 0~100," +
                        "\"Fun\": 0~100," +
                        "\"Anger\": 0~100," +
                        "\"Sad\": 0~100," +
                    "}" +
                    "\"content\": \"会話の文章\"" +
                "} ";
            _messageDataList.Add(initMessage);
        }

        /// <summary>
        /// データを送信する
        /// </summary>
        /// <returns></returns>
        public IEnumerator SetSendData(Action<ChatGPTData.ShowData> callBack)
        {
            // ヘッダー設定
            Dictionary<string, string> headers = new Dictionary<string, string>
            {
                {"Authorization", "Bearer " + _ApiKey},
                {"Content-type", "application/json"},
                {"X-Slack-No-Retry", "1"}
            };

            // 送信データ作成
            ChatGPTData.SendingData sendingData = new ChatGPTData.SendingData();
            sendingData.model = _ApiModel;
            sendingData.messages = _messageDataList;

            // Jsonに変換
            string jsonData = JsonUtility.ToJson(sendingData);

            // 送信
            using var request = new UnityWebRequest(_ApiUrl, "POST")
            {
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonData)),
                downloadHandler = new DownloadHandlerBuffer()
            };
            foreach(var header in headers)
            {
                request.SetRequestHeader(header.Key, header.Value);
            }

            // リクエストが来るまで待つ
            yield return request.SendWebRequest();

            // エラー処理
            if(request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(request.error);
                Debug.LogError(jsonData);
                yield break;
            }

            // 受信したデータをUnityで使えるように変換
            ChatGPTData.ReceivingData receivingData = JsonUtility.FromJson<ChatGPTData.ReceivingData>(request.downloadHandler.text);
            Debug.Log("ChatGPT:" + receivingData.choices[0].message.content);
            ChatGPTData.MessageData messageData = new ChatGPTData.MessageData();
            messageData.content = receivingData.choices[0].message.content;
            messageData.role = receivingData.choices[0].message.role;

            // リストに追加
            _messageDataList.Add(messageData);

            // 結果を返す
            if (callBack != null)
            {
                // Unityで使うテキストに変換
                ChatGPTData.ShowData showData = JsonUtility.FromJson<ChatGPTData.ShowData>(receivingData.choices[0].message.content);
                callBack(showData);
            }
        }

        /// <summary>
        /// 送信するテキストを登録する
        /// </summary>
        /// <param name="message"> 登録するテキスト </param>
        public void SetRecordText(string message)
        {
            // いたずら防止のために空白のみのテキストを判定する
            string checkText = message;
            checkText = checkText.Replace(" ", "");
            checkText = checkText.Replace("　", "");

            if (checkText.Length <= 0)
            {
                // テキストがなければ送らない
                return;
            }

            // ユーザーからのテキスト設定をする
            ChatGPTData.MessageData messageData = new ChatGPTData.MessageData();
            messageData.role = ChatGPTData.GetRoleName(ChatGPTData.RoleType.user);
            messageData.content = message;
            _messageDataList.Add(messageData);
        }
    }
}
