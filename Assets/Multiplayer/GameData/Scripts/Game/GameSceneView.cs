
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Games.CoinRush
{
    public class GameSceneView : SceneViewBase
    {
        [field: SerializeField]
        public ArenaUIOverlayPanelView arenaUiOverlayPanelView { get; private set; }

        public void ShowArenaPanel()
        {
            //ShowPanel(arenaUiOverlayPanelView);
            arenaUiOverlayPanelView.gameObject.SetActive(true);
        }

        public void UpdateScores()
        {
            arenaUiOverlayPanelView.UpdateScores();
        }
    }
}
