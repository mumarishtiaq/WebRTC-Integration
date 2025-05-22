
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Games.CoinRush
{
    public class GameSceneView : SceneViewBase
    {
        [field: SerializeField]
        public ArenaUIOverlayPanelView arenaUiOverlayPanelView { get; private set; }
        public GameResultsPanelView gameResultPanelView ;

        public void ShowArenaPanel()
        {
            //ShowPanel(arenaUiOverlayPanelView);
            arenaUiOverlayPanelView.gameObject.SetActive(true);
        }

        public void UpdateScores()
        {
            arenaUiOverlayPanelView.UpdateScores();
        }

        public void ShowGameResults(GameResultsData results)
        {
            gameResultPanelView.Show();
            gameResultPanelView.ShowResults(results);
        }


    }
}
