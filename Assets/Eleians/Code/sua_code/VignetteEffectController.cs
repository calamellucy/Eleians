using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VignetteEffectController : MonoBehaviour
{
    public static VignetteEffectController Instance;

    [Header("연결 필요")]
    public Volume globalVolume;

    [Header("속도 효과 조절 (여기서 수치 바꾸세요!)")]
    [Range(-1f, 1f)] public float distortionPower = -0.1f; // 왜곡 강도 (기본값 확 줄임)
    [Range(0.01f, 2f)] public float distortionScale = 0.99f; // 화면 크기 (검은 테두리 방지용)
    [Range(0f, 1f)] public float chromaticPower = 0.5f;   // 색수차(무지개 번짐) 강도

    [Header("딸피/긴장감 조절")]
    [Range(0f, 1f)] public float lowHealthIntensity = 0.3f;
    [Range(0f, 1f)] public float tensionIntensity = 0.4f;

    // 내부 변수
    private Vignette _vignette;
    private LensDistortion _lensDistortion;
    private ChromaticAberration _chromaticAberration;

    private bool _isTensionOn = false;
    private bool _isLowHealthOn = false;
    private bool _isSpeedOn = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        globalVolume.profile.TryGet(out _vignette);
        globalVolume.profile.TryGet(out _lensDistortion);
        globalVolume.profile.TryGet(out _chromaticAberration);

        UpdateVisuals();
    }

    // 인스펙터에서 값을 바꿀 때 실시간으로 반영되도록 추가
    void Update()
    {
        if (GameManager.instance.health < (GameManager.instance.maxHealth*0.2f))
        {
            SetLowHealth(true);
        }
        else
        {
            SetLowHealth(false);
        }
    }

    public void SetTension(bool isActive)
    {
        _isTensionOn = isActive;
        UpdateVisuals();
    }

    public void SetLowHealth(bool isActive)
    {
        _isLowHealthOn = isActive;
        UpdateVisuals();
    }

    public void SetHighSpeed(bool isActive)
    {
        _isSpeedOn = isActive;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        // 일단 끄고 시작
        ResetAllComponents();

        if (_isSpeedOn)
        {
            ApplySpeedEffect();
            return;
        }

        if (_isLowHealthOn)
        {
            ApplyLowHealthEffect();
            return;
        }

        if (_isTensionOn)
        {
            ApplyTensionEffect();
            return;
        }
    }

    private void ApplySpeedEffect()
    {
        if (_lensDistortion != null)
        {
            _lensDistortion.active = true;
            _lensDistortion.intensity.value = distortionPower; // 인스펙터 값 사용
            _lensDistortion.scale.value = distortionScale;     // 인스펙터 값 사용
        }
        if (_chromaticAberration != null)
        {
            _chromaticAberration.active = true;
            _chromaticAberration.intensity.value = chromaticPower; // 인스펙터 값 사용
        }
    }

    private void ApplyLowHealthEffect()
    {
        if (_vignette != null)
        {
            _vignette.active = true;
            _vignette.color.value = Color.red;
            _vignette.intensity.value = lowHealthIntensity;
            _vignette.smoothness.value = 0.6f;
        }
    }

    private void ApplyTensionEffect()
    {
        if (_vignette != null)
        {
            _vignette.active = true;
            _vignette.color.value = Color.black;
            _vignette.intensity.value = tensionIntensity;
            _vignette.smoothness.value = 0.4f;
        }
    }

    private void ResetAllComponents()
    {
        if (_vignette != null) _vignette.active = false;
        if (_lensDistortion != null) _lensDistortion.active = false;
        if (_chromaticAberration != null) _chromaticAberration.active = false;
    }
}