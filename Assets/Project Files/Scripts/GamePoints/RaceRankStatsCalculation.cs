using System;
using System.Collections.Generic;
using System.Linq;
using Kart.Project_Files.Scripts.Managers.Game;
using UnityEngine;

namespace Kart.Project_Files.Scripts.GamePoints
{
    public enum RaceRank
    {
        D,
        C,
        B,
        A,
        S,
        SS,
        SSS
    }

    public class RaceRankStatsCalculation : MonoBehaviour
    {
        [Header("Rank Sprites")] public Sprite spriteD;
        public Sprite spriteC;
        public Sprite spriteB;
        public Sprite spriteA;
        public Sprite spriteS;
        public Sprite spriteSS;
        public Sprite spriteSSS;

        [Header("Threshold Distribution")] [Tooltip("If true → thresholds will be spread equally from 0% to 100%")]
        public bool autoDistributeThresholds = true;

        [Tooltip("If not auto, define C,B,A,S,SS,SSS here (in %):")] [Range(0f, 100f)]
        public float[] manualThresholds = new float[6]
            { 20f, 40f, 60f, 80f, 90f, 95f };

        private Dictionary<RaceRank, float> _thresholds;
        private Dictionary<RaceRank, Sprite> _rankSprites;

        private void Awake()
        {
            InitThresholds();
            InitSprites();
        }

        private void InitThresholds()
        {
            _thresholds = new Dictionary<RaceRank, float>();
            var ranks = (RaceRank[])Enum.GetValues(typeof(RaceRank));
            int count = ranks.Length;

            if (autoDistributeThresholds)
            {
                float step = 100f / count;
                for (int i = 1; i < count; i++)
                    _thresholds[ranks[i]] = step * i;
            }
            else
            {
                for (int i = 1; i < count; i++)
                    _thresholds[ranks[i]] = manualThresholds[i - 1];
            }
        }

        private void InitSprites()
        {
            _rankSprites = new Dictionary<RaceRank, Sprite>
            {
                { RaceRank.D, spriteD },
                { RaceRank.C, spriteC },
                { RaceRank.B, spriteB },
                { RaceRank.A, spriteA },
                { RaceRank.S, spriteS },
                { RaceRank.SS, spriteSS },
                { RaceRank.SSS, spriteSSS }
            };
        }

        public RaceRank CalculateRaceRank(float playerPoints)
        {
            float percent = playerPoints / GameManager.Instance.PointsTable.MaxPointsForAllRaces * 100f;
            return (from kv in _thresholds.OrderByDescending(kv => kv.Value) where percent >= kv.Value select kv.Key)
                .FirstOrDefault();
        }

        public Sprite GetRankSprite(RaceRank rank)
        {
            return _rankSprites.GetValueOrDefault(rank);
        }


        public void CalculateRaceRankStats(float playerPoints, out RaceRank rank, out Sprite rankSprite)
        {
            rank = CalculateRaceRank(playerPoints);
            rankSprite = GetRankSprite(rank);
        }
    }
}