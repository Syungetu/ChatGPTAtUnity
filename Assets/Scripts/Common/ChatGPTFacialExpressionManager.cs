using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRM;

namespace com.studiocross.ChatGPT
{
    /// <summary>
    /// ChatGPT�̕\������f���iVRM�j�ɓK������
    /// </summary>
    public class ChatGPTFacialExpressionManager : MonoBehaviour
    {
        [Header("���f���f�[�^")]
        /// <summary> �\���ω����郂�f���f�[�^ </summary>
        public GameObject _ModelObject;

        [Header("�\��Ǘ�")]
        /// <summary> VRM�ŕ\���ω�������X�N���v�g </summary>
        public VRMBlendShapeProxy _VRMBlendShapeProxy;

        [Header("���[�V�����Ǘ�")]
        /// <summary> ���[�V�����̊Ǘ� </summary>
        public Animator _Animator;

        [Header("�\���")]
        /// <summary> ���������� </summary>
        public float _Joy = 0.0f;
        /// <summary> �y�������� </summary>
        public float _Fun = 0.0f;
        /// <summary> �{�萬�� </summary>
        public float _Anger = 0.0f;
        /// <summary> �߂��ݐ��� </summary>
        public float _Sad = 0.0f;

        /// <summary> ���p�N�p������ </summary>
        public float _LipSyncTime = 2.0f;
        /// <summary> ���p�N����(1~10) </summary>
        public float _LipSpeed = 2.5f;
        /// <summary> ���݂̕\��̕ω����� </summary>
        public float _FacialExpressionTime = 3.0f;

        /// <summary> ���݂̌��p�N���� </summary>
        private float _nowLipSyncCount = 0.0f;
        /// <summary> ���̊J��� </summary>
        private float _lipOpenRatio = 0.0f;
        /// <summary> ���݂̌��̊J��� </summary>
        private float _nowLipOpenRatio = 0.0f;
        /// <summary> ���̌`�i0:�� 1:�� 2:�� 3:�� 4:���j </summary>
        private int _lipType = 0;
        /// <summary> �N�`�p�N���̌��̊J���t���O </summary>
        private bool _isLipOpen = false;
        /// <summary> ���݂̕\��̕ω����� </summary>
        private float _nowFacialExpressionCount = 0.0f;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            // �\��̃X�N���v�g���������܂ŏ��������Ȃ�
            if (_VRMBlendShapeProxy == null)
            {
                if (_ModelObject != null)
                {
                    // VRM�ŕ\���ω�������X�N���v�g��T��
                    VRMBlendShapeProxy vrmBlend = _ModelObject.GetComponent<VRMBlendShapeProxy>();
                    if (vrmBlend != null)
                    {
                        _VRMBlendShapeProxy = vrmBlend;
                    }
                }
                return;
            }

            // ���[�t�B���O���Ǘ�
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
            // ����������
            _Joy = _Joy > 1.0f ? 1.0f : _Joy;
            _VRMBlendShapeProxy.ImmediatelySetValue(
                BlendShapeKey.CreateFromPreset(BlendShapePreset.Joy),
                _Joy
            );
            // �y��������
            _Fun = _Fun > 1.0f ? 1.0f : _Fun;
            _VRMBlendShapeProxy.ImmediatelySetValue(
                BlendShapeKey.CreateFromPreset(BlendShapePreset.Fun),
                _Fun
            );
            // �{�萬��
            _Anger = _Anger > 1.0f ? 1.0f : _Anger;
            _VRMBlendShapeProxy.ImmediatelySetValue(
                BlendShapeKey.CreateFromPreset(BlendShapePreset.Angry),
                _Anger
            );
            // �߂��ݐ���
            _Sad = _Sad > 1.0f ? 1.0f : _Sad;
            _VRMBlendShapeProxy.ImmediatelySetValue(
                BlendShapeKey.CreateFromPreset(BlendShapePreset.Sorrow),
                _Sad
            );

            // ���[�V������ݒ肷��
            if(_Joy == _Fun && _Fun == _Anger && _Anger == _Sad)
            {
                // �ʏ탂�[�V����
                _Animator.SetInteger("animBaseInt", 1);
            }
            else if(_Joy > _Fun && _Joy > _Anger && _Joy > _Sad)
            {
                // ���������[�V����
                _Animator.SetInteger("animBaseInt", 5);
            }
            else if (_Fun > _Joy && _Fun > _Anger && _Fun > _Sad)
            {
                // �y�������[�V����
                _Animator.SetInteger("animBaseInt", 4);
            }
            else if (_Anger > _Joy && _Anger > _Fun && _Anger > _Sad)
            {
                // �{�胂�[�V����
                _Animator.SetInteger("animBaseInt", 2);
            }
            else if (_Sad > _Joy && _Sad > _Fun && _Sad > _Anger)
            {
                // �߂��݃��[�V����
                _Animator.SetInteger("animBaseInt", 9);
            }

            // ���p�N���Ԃ��o�߂���
            float speed = 0.01f * _LipSpeed;
            if (_nowLipSyncCount > 0.0f)
            {
                // ���p�N��
                _nowLipSyncCount -= Time.deltaTime;
                if (_isLipOpen == true)
                {
                    // �����J��
                    _nowLipOpenRatio += _lipOpenRatio * speed;
                    if (_nowLipOpenRatio >= _lipOpenRatio)
                    {
                        // �����J��������
                        _isLipOpen = false;
                    }
                }
                else
                {
                    // �������
                    _nowLipOpenRatio -= _lipOpenRatio * speed;
                    if(_nowLipOpenRatio <= 0.0f)
                    {
                        // �����������
                        _nowLipOpenRatio = 0.0f;
                        _isLipOpen = true;
                        // ���̌��̌`��
                        SetLipChange();
                    }
                }
            }
            else
            {
                // ���p�N���Ă��Ȃ�
                _nowLipOpenRatio -= _lipOpenRatio * speed;
                if (_nowLipOpenRatio <= 0.0f)
                {
                    _nowLipOpenRatio = 0.0f;
                }
            }

            // ���̌`
            switch (_lipType)
            {
                case 0:
                    // ��
                    _VRMBlendShapeProxy.ImmediatelySetValue(
                        BlendShapeKey.CreateFromPreset(BlendShapePreset.A),
                        _nowLipOpenRatio
                    );
                    break;
                case 1:
                    // ��
                    _VRMBlendShapeProxy.ImmediatelySetValue(
                        BlendShapeKey.CreateFromPreset(BlendShapePreset.I),
                        _nowLipOpenRatio
                    );
                    break;
                case 2:
                    // ��
                    _VRMBlendShapeProxy.ImmediatelySetValue(
                        BlendShapeKey.CreateFromPreset(BlendShapePreset.U),
                        _nowLipOpenRatio
                    );
                    break;
                case 3:
                    // ��
                    _VRMBlendShapeProxy.ImmediatelySetValue(
                        BlendShapeKey.CreateFromPreset(BlendShapePreset.E),
                        _nowLipOpenRatio
                    );
                    break;
                case 4:
                    // ��
                    _VRMBlendShapeProxy.ImmediatelySetValue(
                        BlendShapeKey.CreateFromPreset(BlendShapePreset.O),
                        _nowLipOpenRatio
                    );
                    break;
            }
        }

        /// <summary>
        /// �e�L�X�g�ύX���Ď�����
        /// </summary>
        public void GetTextChange()
        {
            _isLipOpen = true;
            _nowLipSyncCount = _LipSyncTime;
            _nowFacialExpressionCount = _FacialExpressionTime;
            SetLipChange();
        }

        /// <summary>
        /// ���̌`��ς���
        /// </summary>
        private void SetLipChange()
        {
            _lipOpenRatio = UnityEngine.Random.Range(0.0f, 1.0f);
            _lipType = UnityEngine.Random.Range(0, 5);
        }
    }
}