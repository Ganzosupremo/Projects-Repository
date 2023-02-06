using UnityEngine;
using TMPro;

public class CreditsPrefab : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(nameText), nameText);
        HelperUtilities.ValidateCheckNullValue(this, nameof(descriptionText), descriptionText);
    }
#endif
    #endregion
}
