using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 鏡面反射
/// </summary>
/// <see href="https://qiita.com/hikoalpha/items/8445109a20b8139ce7b5"/>
public class MirrorReflection : MonoBehaviour
{
    [SerializeField, Tooltip("メインになるカメラを指定します")]
    private Camera _targetCamera;
    [SerializeField, Tooltip("反射用のカメラを指定します")]
    private Camera _refCamera;
    [SerializeField, Tooltip("")]
    private Transform _reflection;
    [SerializeField, Tooltip("")]
    private Renderer _renderer;
    [SerializeField, Tooltip(""), Range(128, 1024)]
    private int _textureSize = 512;
    [SerializeField, Tooltip(""), Min(0.1f)]
    private float _size = 2.0f;

    private Material _matRefPlane;
    private RenderTexture _refTexture;

    private readonly int refBaseMapId = Shader.PropertyToID("_BaseMap");

    /// <summary>
    /// メインになるカメラ
    /// </summary>
    public Camera TargetCamera
    {
        get => _targetCamera;
        set
        {
            _targetCamera = value;
            SetupReflectionCamera();
        }
    }

    private void Awake()
    {
        _refTexture = new RenderTexture(512, 512, 24, RenderTextureFormat.ARGB32);
        SetupReflectionCamera();
    }

    private void OnDestroy()
    {
        if (_refTexture == null)
        {
            return;
        }

        Destroy(_refTexture);
        _refTexture = null;
    }

    void LateUpdate()
    {
        if (!_refCamera || !_targetCamera)
        {
            return;
        }

        UpdateReflectionCamera();
        AdjustRenderTexture();
    }

    private void SetupReflectionCamera()
    {
        if (_targetCamera == null)
        {
            Debug.LogWarning("対象となるメインカメラが設定されていません。");
        }

        if (_refCamera == null)
        {
            Debug.LogWarning("反射表現用のカメラが設定されていません。");
        }

        _refCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Mirror"));

        _refCamera.targetTexture = _refTexture;
        _matRefPlane = _renderer.sharedMaterial;
        _matRefPlane.SetTexture(refBaseMapId, _refTexture);
    }

    private void UpdateReflectionCamera()
    {
        // 鏡の法線（反射基準）
        Vector3 mirrorNormal = -transform.forward;

        // カメラ位置を鏡面対称に反転
        Vector3 camPos = _targetCamera.transform.position;
        Vector3 reflectedPos = camPos - 2 * Vector3.Dot(camPos - transform.position, mirrorNormal) * mirrorNormal;
        _refCamera.transform.position = reflectedPos;

        // 鏡面の方向に向ける
        _refCamera.transform.LookAt(_reflection.position);

        // カメラ設定の更新
        var distance = Vector3.Distance(transform.position, _refCamera.transform.position);
        _refCamera.nearClipPlane = distance * 0.9f;

        // 鏡面のサイズを調整
        _reflection.localScale = new Vector3(-_size, _size, 1);

        // 焦点距離と表示したい鏡面サイズから画角(FOV)を計算する
        _refCamera.fieldOfView = 2 * Mathf.Atan(_size / (2 * distance)) * Mathf.Rad2Deg;
    }

    private void AdjustRenderTexture()
    {
        int width = Mathf.FloorToInt(_textureSize * _size);
        int height = Mathf.FloorToInt(_textureSize * _size);
        if (_refTexture == null || _refTexture.width != width || _refTexture.height != height)
        {
            if (_refTexture != null)
            {
                Destroy(_refTexture);
            }

            _refTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            _matRefPlane.SetTexture(refBaseMapId, _refTexture);
            _refCamera.targetTexture = _refTexture;
        }
    }

}
