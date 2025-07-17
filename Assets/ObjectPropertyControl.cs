using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ObjectPropertyControl : MonoBehaviour
{
    private MassSpringSystem targetSystem;


    public TextMeshProUGUI systemNameText;


    public Slider restitutionSlider;
    public TextMeshProUGUI restitutionValueText;
    public Slider pointRadiusSlider;
    public TextMeshProUGUI pointRadiusValueText;
    public Slider collisionPointRadiusSlider;
    public TextMeshProUGUI collisionPointRadiusValueText;


    public Slider stiffnessSlider;
    public TextMeshProUGUI stiffnessValueText;
    public Slider bendingStiffnessSlider;
    public TextMeshProUGUI bendingStiffnessValueText;
    public Slider volumeStiffnessSlider;
    public TextMeshProUGUI volumeStiffnessValueText;
    public Slider dampingSlider;
    public TextMeshProUGUI dampingValueText;
    public Slider gravitySlider;
    public TextMeshProUGUI gravityValueText;
    public Toggle isFixedTopToggle;
    public Slider fixedTopRatioSlider;
    public TextMeshProUGUI fixedTopRatioValueText;


    public Slider collisionColorIntensitySlider;
    public TextMeshProUGUI collisionColorIntensityValueText;
    public Slider pointSizeSlider;
    public TextMeshProUGUI pointSizeValueText;


    public Slider solverIterationsSlider;
    public TextMeshProUGUI solverIterationsValueText;
    public Slider groundLevelSlider;
    public TextMeshProUGUI groundLevelValueText;
    public Slider groundStiffnessSlider;
    public TextMeshProUGUI groundStiffnessValueText;


    public Slider resolutionSlider;
    public TextMeshProUGUI resolutionValueText;



    public void SetMassSpringSystem(MassSpringSystem system)
    {
        targetSystem = system;


        if (systemNameText != null)
        {
            systemNameText.text = system.gameObject.name;
        }


        if (restitutionSlider != null)
        {
            restitutionSlider.value = targetSystem.restitution;
            restitutionSlider.onValueChanged.AddListener(OnRestitutionChanged);
            OnRestitutionChanged(targetSystem.restitution);
        }
        if (pointRadiusSlider != null)
        {
            pointRadiusSlider.value = targetSystem.pointRadius;
            pointRadiusSlider.onValueChanged.AddListener(OnPointRadiusChanged);
            OnPointRadiusChanged(targetSystem.pointRadius);
        }
        if (collisionPointRadiusSlider != null)
        {
            collisionPointRadiusSlider.value = targetSystem.collisionPointRadius;
            collisionPointRadiusSlider.onValueChanged.AddListener(OnCollisionPointRadiusChanged);
            OnCollisionPointRadiusChanged(targetSystem.collisionPointRadius);
        }


        if (stiffnessSlider != null)
        {
            stiffnessSlider.value = targetSystem.stiffness;
            stiffnessSlider.onValueChanged.AddListener(OnStiffnessChanged);
            OnStiffnessChanged(targetSystem.stiffness);
        }
        if (bendingStiffnessSlider != null)
        {
            bendingStiffnessSlider.value = targetSystem.bendingStiffness;
            bendingStiffnessSlider.onValueChanged.AddListener(OnBendingStiffnessChanged);
            OnBendingStiffnessChanged(targetSystem.bendingStiffness);
        }
        if (volumeStiffnessSlider != null)
        {
            volumeStiffnessSlider.value = targetSystem.volumeStiffness;
            volumeStiffnessSlider.onValueChanged.AddListener(OnVolumeStiffnessChanged);
            OnVolumeStiffnessChanged(targetSystem.volumeStiffness);
        }
        if (dampingSlider != null)
        {
            dampingSlider.value = targetSystem.damping;
            dampingSlider.onValueChanged.AddListener(OnDampingChanged);
            OnDampingChanged(targetSystem.damping);
        }
        if (gravitySlider != null)
        {
            gravitySlider.value = targetSystem.gravity;
            gravitySlider.onValueChanged.AddListener(OnGravityChanged);
            OnGravityChanged(targetSystem.gravity);
        }
        if (isFixedTopToggle != null)
        {
            isFixedTopToggle.isOn = targetSystem.isFixedTop;
            isFixedTopToggle.onValueChanged.AddListener(OnIsFixedTopChanged);
        }
        if (fixedTopRatioSlider != null)
        {
            fixedTopRatioSlider.value = targetSystem.fixedTopRatio;
            fixedTopRatioSlider.onValueChanged.AddListener(OnFixedTopRatioChanged);
            OnFixedTopRatioChanged(targetSystem.fixedTopRatio);
        }


        if (collisionColorIntensitySlider != null)
        {
            collisionColorIntensitySlider.value = targetSystem.collisionColorIntensity;
            collisionColorIntensitySlider.onValueChanged.AddListener(OnCollisionColorIntensityChanged);
            OnCollisionColorIntensityChanged(targetSystem.collisionColorIntensity);
        }
        if (pointSizeSlider != null)
        {
            pointSizeSlider.value = targetSystem.pointSize;
            pointSizeSlider.onValueChanged.AddListener(OnPointSizeChanged);
            OnPointSizeChanged(targetSystem.pointSize);
        }


        if (solverIterationsSlider != null)
        {
            solverIterationsSlider.value = targetSystem.solverIterations;
            solverIterationsSlider.onValueChanged.AddListener(OnSolverIterationsChanged);
            OnSolverIterationsChanged(targetSystem.solverIterations);
        }
        if (groundLevelSlider != null)
        {
            groundLevelSlider.value = targetSystem.groundLevel;
            groundLevelSlider.onValueChanged.AddListener(OnGroundLevelChanged);
            OnGroundLevelChanged(targetSystem.groundLevel);
        }
        if (groundStiffnessSlider != null)
        {
            groundStiffnessSlider.value = targetSystem.groundStiffness;
            groundStiffnessSlider.onValueChanged.AddListener(OnGroundStiffnessChanged);
            OnGroundStiffnessChanged(targetSystem.groundStiffness);
        }


        if (resolutionSlider != null)
        {
            resolutionSlider.value = targetSystem.resolution;
            resolutionSlider.onValueChanged.AddListener(OnResolutionChanged);
            OnResolutionChanged(targetSystem.resolution);
        }
    }


    public void OnStiffnessChanged(float value)
    {
        if (targetSystem != null)
        {
            targetSystem.stiffness = value;
            if (stiffnessValueText != null)
            {
                stiffnessValueText.text = value.ToString("F2");
            }
        }
    }

    public void OnDampingChanged(float value)
    {
        if (targetSystem != null)
        {
            targetSystem.damping = value;
            if (dampingValueText != null)
            {
                dampingValueText.text = value.ToString("F2");
            }
        }
    }

    public void OnIsFixedTopChanged(bool value)
    {
        if (targetSystem != null)
        {
            targetSystem.isFixedTop = value;


        }
    }

    public void OnGravityChanged(float value)
    {
        if (targetSystem != null)
        {
            targetSystem.gravity = value;
            if (gravityValueText != null)
            {
                gravityValueText.text = value.ToString("F2");
            }
        }
    }

    public void OnRestitutionChanged(float value)
    {
        if (targetSystem != null)
        {
            targetSystem.restitution = value;
            if (restitutionValueText != null)
            {
                restitutionValueText.text = value.ToString("F2");
            }
        }
    }

    public void OnPointRadiusChanged(float value)
    {
        if (targetSystem != null)
        {
            targetSystem.pointRadius = value;

            if (pointRadiusValueText != null)
            {
                pointRadiusValueText.text = value.ToString("F2");
            }
        }
    }


    public void OnCollisionPointRadiusChanged(float value)
    {
        if (targetSystem != null)
        {
            targetSystem.collisionPointRadius = value;
            if (collisionPointRadiusValueText != null)
            {
                collisionPointRadiusValueText.text = value.ToString("F2");
            }
        }
    }

    public void OnBendingStiffnessChanged(float value)
    {
        if (targetSystem != null)
        {
            targetSystem.bendingStiffness = value;
            if (bendingStiffnessValueText != null)
            {
                bendingStiffnessValueText.text = value.ToString("F2");
            }
        }
    }

    public void OnVolumeStiffnessChanged(float value)
    {
        if (targetSystem != null)
        {
            targetSystem.volumeStiffness = value;
            if (volumeStiffnessValueText != null)
            {
                volumeStiffnessValueText.text = value.ToString("F2");
            }
        }
    }

    public void OnFixedTopRatioChanged(float value)
    {
        if (targetSystem != null)
        {
            targetSystem.fixedTopRatio = value;
            if (fixedTopRatioValueText != null)
            {
                fixedTopRatioValueText.text = value.ToString("F2");
            }
        }
    }

    public void OnCollisionColorIntensityChanged(float value)
    {
        if (targetSystem != null)
        {
            targetSystem.collisionColorIntensity = value;
            if (collisionColorIntensityValueText != null)
            {
                collisionColorIntensityValueText.text = value.ToString("F0"); // Integer value
            }
        }
    }

    public void OnPointSizeChanged(float value)
    {
        if (targetSystem != null)
        {
            targetSystem.pointSize = value;

            if (pointSizeValueText != null)
            {
                pointSizeValueText.text = value.ToString("F2");
            }
        }
    }

    public void OnSolverIterationsChanged(float value)
    {
        if (targetSystem != null)
        {
            targetSystem.solverIterations = Mathf.RoundToInt(value);
            if (solverIterationsValueText != null)
            {
                solverIterationsValueText.text = targetSystem.solverIterations.ToString();
            }
        }
    }

    public void OnGroundLevelChanged(float value)
    {
        if (targetSystem != null)
        {
            targetSystem.groundLevel = value;
            if (groundLevelValueText != null)
            {
                groundLevelValueText.text = value.ToString("F2");
            }
        }
    }

    public void OnGroundStiffnessChanged(float value)
    {
        if (targetSystem != null)
        {
            targetSystem.groundStiffness = value;
            if (groundStiffnessValueText != null)
            {
                groundStiffnessValueText.text = value.ToString("F0");
            }
        }
    }

    public void OnResolutionChanged(float value)
    {
        if (targetSystem != null)
        {
            targetSystem.resolution = Mathf.RoundToInt(value);
            if (resolutionValueText != null)
            {
                resolutionValueText.text = targetSystem.resolution.ToString();
            }

        }
    }
}