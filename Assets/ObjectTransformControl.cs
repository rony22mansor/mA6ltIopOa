using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ObjectTransformControl : MonoBehaviour
{
    private GameObject targetGameObject; 

   
    public Slider posXSlider;
    public TextMeshProUGUI posXValueText;
    public Slider posYSlider;
    public TextMeshProUGUI posYValueText;
    public Slider posZSlider;
    public TextMeshProUGUI posZValueText;

   
    public Slider rotXSlider;
    public TextMeshProUGUI rotXValueText;
    public Slider rotYSlider;
    public TextMeshProUGUI rotYValueText;
    public Slider rotZSlider;
    public TextMeshProUGUI rotZValueText;

    
    public Slider scaleXSlider;
    public TextMeshProUGUI scaleXValueText;
    public Slider scaleYSlider;
    public TextMeshProUGUI scaleYValueText;
    public Slider scaleZSlider;
    public TextMeshProUGUI scaleZValueText;

    public void SetTargetGameObject(GameObject target)
    {
        targetGameObject = target;
        InitializeSliders();
    }

    private void InitializeSliders()
    {
        if (targetGameObject == null) return;

       
        if (posXSlider != null) { posXSlider.value = targetGameObject.transform.position.x; posXSlider.onValueChanged.AddListener(OnPosXChanged); OnPosXChanged(posXSlider.value); }
        if (posYSlider != null) { posYSlider.value = targetGameObject.transform.position.y; posYSlider.onValueChanged.AddListener(OnPosYChanged); OnPosYChanged(posYSlider.value); }
        if (posZSlider != null) { posZSlider.value = targetGameObject.transform.position.z; posZSlider.onValueChanged.AddListener(OnPosZChanged); OnPosZChanged(posZSlider.value); }

        
        if (rotXSlider != null) { rotXSlider.value = targetGameObject.transform.eulerAngles.x; rotXSlider.onValueChanged.AddListener(OnRotXChanged); OnRotXChanged(rotXSlider.value); }
        if (rotYSlider != null) { rotYSlider.value = targetGameObject.transform.eulerAngles.y; rotYSlider.onValueChanged.AddListener(OnRotYChanged); OnRotYChanged(rotYSlider.value); }
        if (rotZSlider != null) { rotZSlider.value = targetGameObject.transform.eulerAngles.z; rotZSlider.onValueChanged.AddListener(OnRotZChanged); OnRotZChanged(rotZSlider.value); }

        
        if (scaleXSlider != null) { scaleXSlider.value = targetGameObject.transform.localScale.x; scaleXSlider.onValueChanged.AddListener(OnScaleXChanged); OnScaleXChanged(scaleXSlider.value); }
        if (scaleYSlider != null) { scaleYSlider.value = targetGameObject.transform.localScale.y; scaleYSlider.onValueChanged.AddListener(OnScaleYChanged); OnScaleYChanged(scaleYSlider.value); }
        if (scaleZSlider != null) { scaleZSlider.value = targetGameObject.transform.localScale.z; scaleZSlider.onValueChanged.AddListener(OnScaleZChanged); OnScaleZChanged(scaleZSlider.value); }
    }

    
    public void OnPosXChanged(float value)
    {
        if (targetGameObject != null)
        {
            targetGameObject.transform.position = new Vector3(value, targetGameObject.transform.position.y, targetGameObject.transform.position.z);
            if (posXValueText != null) posXValueText.text = value.ToString("F2");
        }
    }

    public void OnPosYChanged(float value)
    {
        if (targetGameObject != null)
        {
            targetGameObject.transform.position = new Vector3(targetGameObject.transform.position.x, value, targetGameObject.transform.position.z);
            if (posYValueText != null) posYValueText.text = value.ToString("F2");
        }
    }

    public void OnPosZChanged(float value)
    {
        if (targetGameObject != null)
        {
            targetGameObject.transform.position = new Vector3(targetGameObject.transform.position.x, targetGameObject.transform.position.y, value);
            if (posZValueText != null) posZValueText.text = value.ToString("F2");
        }
    }

    
    public void OnRotXChanged(float value)
    {
        if (targetGameObject != null)
        {
            targetGameObject.transform.rotation = Quaternion.Euler(value, targetGameObject.transform.eulerAngles.y, targetGameObject.transform.eulerAngles.z);
            if (rotXValueText != null) rotXValueText.text = value.ToString("F0");
        }
    }

    public void OnRotYChanged(float value)
    {
        if (targetGameObject != null)
        {
            targetGameObject.transform.rotation = Quaternion.Euler(targetGameObject.transform.eulerAngles.x, value, targetGameObject.transform.eulerAngles.z);
            if (rotYValueText != null) rotYValueText.text = value.ToString("F0");
        }
    }

    public void OnRotZChanged(float value)
    {
        if (targetGameObject != null)
        {
            targetGameObject.transform.rotation = Quaternion.Euler(targetGameObject.transform.eulerAngles.x, targetGameObject.transform.eulerAngles.y, value);
            if (rotZValueText != null) rotZValueText.text = value.ToString("F0");
        }
    }

    
    public void OnScaleXChanged(float value)
    {
        if (targetGameObject != null)
        {
            targetGameObject.transform.localScale = new Vector3(value, targetGameObject.transform.localScale.y, targetGameObject.transform.localScale.z);
            if (scaleXValueText != null) scaleXValueText.text = value.ToString("F2");
        }
    }

    public void OnScaleYChanged(float value)
    {
        if (targetGameObject != null)
        {
            targetGameObject.transform.localScale = new Vector3(targetGameObject.transform.localScale.x, value, targetGameObject.transform.localScale.z);
            if (scaleYValueText != null) scaleYValueText.text = value.ToString("F2");
        }
    }

    public void OnScaleZChanged(float value)
    {
        if (targetGameObject != null)
        {
            targetGameObject.transform.localScale = new Vector3(targetGameObject.transform.localScale.x, targetGameObject.transform.localScale.y, value);
            if (scaleZValueText != null) scaleZValueText.text = value.ToString("F2");
        }
    }
}
