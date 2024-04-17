using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRM;

namespace com.studiocross.ChatGPT
{
    /// <summary>
    /// ChatGPTの表情をモデル（VRM）に適応する
    /// </summary>
    public class ChatGPTFacialExpressionManager : MonoBehaviour
    {
        [Header("モデルデータ")]
        /// <summary> 表情を変化するモデルデータ </summary>
        public GameObject _ModelObject;

        [Header("表情管理")]
        /// <summary> VRMで表情を変化させるスクリプト </summary>
        public VRMBlendShapeProxy _VRMBlendShapeProxy;

        [Header("モーション管理")]
        /// <summary> モーションの管理 </summary>
        public Animator _Animator;

        [Header("表情成分")]
        /// <summary> 嬉しい成分 </summary>
        public float _Joy = 0.0f;
        /// <summary> 楽しい成分 </summary>
        public float _Fun = 0.0f;
        /// <summary> 怒り成分 </summary>
        public float _Anger = 0.0f;
        /// <summary> 悲しみ成分 </summary>
        public float _Sad = 0.0f;

        /// <summary> 口パク継続時間 </summary>
        public float _LipSyncTime = 2.0f;
        /// <summary> 口パク速さ(1~10) </summary>
        public float _LipSpeed = 2.5f;
        /// <summary> 現在の表情の変化時間 </summary>
        public float _FacialExpressionTime = 3.0f;

        /// <summary> 現在の口パク時間 </summary>
        private float _nowLipSyncCount = 0.0f;
        /// <summary> 口の開き具合 </summary>
        private float _lipOpenRatio = 0.0f;
        /// <summary> 現在の口の開き具合 </summary>
        private float _nowLipOpenRatio = 0.0f;
        /// <summary> 口の形（0:あ 1:い 2:う 3:え 4:お） </summary>
        private int _lipType = 0;
        /// <summary> クチパク中の口の開きフラグ </summary>
        private bool _isLipOpen = false;
        /// <summary> 現在の表情の変化時間 </summary>
        private float _nowFacialExpressionCount = 0.0f;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            // 表情のスクリプトが見つかれるまで処理させない
            if (_VRMBlendShapeProxy == null)
            {
                if (_ModelObject != null)
                {
                    // VRMで表情を変化させるスクリプトを探す
                    VRMBlendShapeProxy vrmBlend = _ModelObject.GetComponent<VRMBlendShapeProxy>();
                    if (vrmBlend != null)
                    {
                        _VRMBlendShapeProxy = vrmBlend;
                    }
                }
                return;
            }

            // モーフィングを管理
            if (_nowFacialExpressionCount > 0.0f)
            {
                _nowFacialExpressionCount -= Time.deltaTime;
            }
            else
            {
                _Joy = 0.0f;
                _Fun = 0.0f;
                _Anger = 0.0f;
                _Sad = 0.0f;
            }
            // 嬉しい成分
            _Joy = _Joy > 1.0f ? 1.0f : _Joy;
            _VRMBlendShapeProxy.ImmediatelySetValue(
                BlendShapeKey.CreateFromPreset(BlendShapePreset.Joy),
                _Joy
            );
            // 楽しい成分
            _Fun = _Fun > 1.0f ? 1.0f : _Fun;
            _VRMBlendShapeProxy.ImmediatelySetValue(
                BlendShapeKey.CreateFromPreset(BlendShapePreset.Fun),
                _Fun
            );
            // 怒り成分
            _Anger = _Anger > 1.0f ? 1.0f : _Anger;
            _VRMBlendShapeProxy.ImmediatelySetValue(
                BlendShapeKey.CreateFromPreset(BlendShapePreset.Angry),
                _Anger
            );
            // 悲しみ成分
            _Sad = _Sad > 1.0f ? 1.0f : _Sad;
            _VRMBlendShapeProxy.ImmediatelySetValue(
                BlendShapeKey.CreateFromPreset(BlendShapePreset.Sorrow),
                _Sad
            );

            // モーションを設定する
            if(_Joy == _Fun && _Fun == _Anger && _Anger == _Sad)
            {
                // 通常モーション
                _Animator.SetInteger("animBaseInt", 1);
            }
            else if(_Joy > _Fun && _Joy > _Anger && _Joy > _Sad)
            {
                // 嬉しいモーション
                _Animator.SetInteger("animBaseInt", 5);
            }
            else if (_Fun > _Joy && _Fun > _Anger && _Fun > _Sad)
            {
                // 楽しいモーション
                _Animator.SetInteger("animBaseInt", 4);
            }
            else if (_Anger > _Joy && _Anger > _Fun && _Anger > _Sad)
            {
                // 怒りモーション
                _Animator.SetInteger("animBaseInt", 2);
            }
            else if (_Sad > _Joy && _Sad > _Fun && _Sad > _Anger)
            {
                // 悲しみモーション
                _Animator.SetInteger("animBaseInt", 9);
            }

            // 口パク時間を経過する
            float speed = 0.01f * _LipSpeed;
            if (_nowLipSyncCount > 0.0f)
            {
                // 口パク中
                _nowLipSyncCount -= Time.deltaTime;
                if (_isLipOpen == true)
                {
                    // 口を開く
                    _nowLipOpenRatio += _lipOpenRatio * speed;
                    if (_nowLipOpenRatio >= _lipOpenRatio)
                    {
                        // 口を開ききった
                        _isLipOpen = false;
                    }
                }
                else
                {
                    // 口を閉じる
                    _nowLipOpenRatio -= _lipOpenRatio * speed;
                    if(_nowLipOpenRatio <= 0.0f)
                    {
                        // 口を閉じきった
                        _nowLipOpenRatio = 0.0f;
                        _isLipOpen = true;
                        // 次の口の形へ
                        SetLipChange();
                    }
                }
            }
            else
            {
                // 口パクしていない
                _nowLipOpenRatio -= _lipOpenRatio * speed;
                if (_nowLipOpenRatio <= 0.0f)
                {
                    _nowLipOpenRatio = 0.0f;
                }
            }

            // 口の形
            switch (_lipType)
            {
                case 0:
                    // あ
                    _VRMBlendShapeProxy.ImmediatelySetValue(
                        BlendShapeKey.CreateFromPreset(BlendShapePreset.A),
                        _nowLipOpenRatio
                    );
                    break;
                case 1:
                    // い
                    _VRMBlendShapeProxy.ImmediatelySetValue(
                        BlendShapeKey.CreateFromPreset(BlendShapePreset.I),
                        _nowLipOpenRatio
                    );
                    break;
                case 2:
                    // う
                    _VRMBlendShapeProxy.ImmediatelySetValue(
                        BlendShapeKey.CreateFromPreset(BlendShapePreset.U),
                        _nowLipOpenRatio
                    );
                    break;
                case 3:
                    // え
                    _VRMBlendShapeProxy.ImmediatelySetValue(
                        BlendShapeKey.CreateFromPreset(BlendShapePreset.E),
                        _nowLipOpenRatio
                    );
                    break;
                case 4:
                    // お
                    _VRMBlendShapeProxy.ImmediatelySetValue(
                        BlendShapeKey.CreateFromPreset(BlendShapePreset.O),
                        _nowLipOpenRatio
                    );
                    break;
            }
        }

        /// <summary>
        /// テキスト変更を監視する
        /// </summary>
        public void GetTextChange()
        {
            _isLipOpen = true;
            _nowLipSyncCount = _LipSyncTime;
            _nowFacialExpressionCount = _FacialExpressionTime;
            SetLipChange();
        }

        /// <summary>
        /// 口の形を変える
        /// </summary>
        private void SetLipChange()
        {
            _lipOpenRatio = UnityEngine.Random.Range(0.0f, 1.0f);
            _lipType = UnityEngine.Random.Range(0, 5);
        }
    }
}