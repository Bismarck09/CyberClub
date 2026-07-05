using UnityEngine;
using YG;

public class CyberClubYG2StickyBannerService : MonoBehaviour
{
    [SerializeField] private bool _showOnStart = true;

    private void Start()
    {
        if (_showOnStart)
            ShowStickyBanner();
    }

    public void ShowStickyBanner() => YG2.StickyAdActivity(true);
    public void HideStickyBanner() => YG2.StickyAdActivity(false);
}