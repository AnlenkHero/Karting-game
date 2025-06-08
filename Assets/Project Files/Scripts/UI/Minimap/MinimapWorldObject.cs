using UnityEngine;

public class MinimapWorldObject : MonoBehaviour
{
    [SerializeField] private bool followObject = false;
    [SerializeField] private Sprite minimapIcon;
    [SerializeField] string nameText;

    public Sprite MinimapIcon => minimapIcon;
    public string NameText => nameText;

    private void Start()
    {
        MinimapController.Instance.RegisterMinimapWorldObject(this, followObject);
    }

    private void OnEnable()
    {
        MinimapController.Instance?.RegisterMinimapWorldObject(this, followObject);
    }

    private void OnDisable()
    {
        MinimapController.Instance?.RemoveMinimapWorldObject(this);
    }

    private void OnDestroy()
    {
        MinimapController.Instance.RemoveMinimapWorldObject(this);
    }
}