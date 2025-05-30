using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Games.RockPaperScissors
{
    public class PlayerChoicesHandler : MonoBehaviour
    {
        [Header("Sprites")]
        [SerializeField] private Sprite _rockSprite;
        [SerializeField] private Sprite _paperSprite;
        [SerializeField] private Sprite _scissorsSprite; 
        
        
        [Header("Choice Colors")]
        [SerializeField] private Color _rockColor;
        [SerializeField] private Color _paperColor;
        [SerializeField] private Color _scissorsColor;


        [Header("PLayer Choice Data")]
        [SerializeField] private PlayerChoiceData _local;
        [SerializeField] private PlayerChoiceData _remote;
        
        
       



        public void SetPlayerChoice(ChoiceType choice, bool isLocal,Action onComplete = null)
        {
            GetSpriteAndColor(choice, out Sprite sprite, out Color color);

            if (sprite == null || color == null) return;

            var playerChoiceData = isLocal ? _local : _remote;
            float xpos  = isLocal ? -559 : 559;

            playerChoiceData.Background.color = color;
            playerChoiceData.SpriteHolder.sprite = sprite;
            playerChoiceData.ChoiceTxt.text = choice.ToString();
            
            playerChoiceData.Transform.Scale(Vector3.one, 0.4f, Ease.OutBack,onComplete);
            //playerChoiceData.Transform.MoveX(xpos, 0.6f, onComplete);
        }

        private void GetSpriteAndColor(ChoiceType choice, out Sprite sprite, out Color color)
        {
            sprite = null;
            color = Color.white;
            switch (choice)
            {
                case ChoiceType.Rock:
                    sprite = _rockSprite;
                    color = _rockColor;
                    break;
                case ChoiceType.Paper:
                    sprite = _paperSprite;
                    color = _paperColor;
                    break;
                case ChoiceType.Scissors:
                    sprite = _scissorsSprite;
                    color = _scissorsColor;
                    break;
                default:
                    Debug.Log("UnHandled Choice Type");
                    break;
            }
        }

       
        public void ResetToDefault()
        {
            _local.Transform.Scale(Vector3.zero);
            _remote.Transform.Scale(Vector3.zero);
        }
    }

    [System.Serializable]
    public struct PlayerChoiceData
    {
        public Transform Transform;
        public Image Background;
        public Image SpriteHolder;
        public TextMeshProUGUI ChoiceTxt;
    }

}
