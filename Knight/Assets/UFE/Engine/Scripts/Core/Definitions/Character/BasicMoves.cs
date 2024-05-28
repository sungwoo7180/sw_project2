using System;
using System.Collections.Generic;

namespace UFE3D
{
    [System.Serializable]
    public class BasicMoves : ICloneable
    {
        public BasicMoveInfo idle = new();
        public BasicMoveInfo moveForward = new();
        public BasicMoveInfo moveBack = new();
        public BasicMoveInfo moveSideways = new();
        public BasicMoveInfo crouching = new();
        public BasicMoveInfo takeOff = new();
        public BasicMoveInfo jumpStraight = new();
        public BasicMoveInfo jumpBack = new();
        public BasicMoveInfo jumpForward = new();
        public BasicMoveInfo fallStraight = new();
        public BasicMoveInfo fallBack = new();
        public BasicMoveInfo fallForward = new();
        public BasicMoveInfo landing = new();
        public BasicMoveInfo blockingCrouchingPose = new();
        public BasicMoveInfo blockingCrouchingHit = new();
        public BasicMoveInfo blockingHighPose = new();
        public BasicMoveInfo blockingHighHit = new();
        public BasicMoveInfo blockingLowHit = new();
        public BasicMoveInfo blockingAirPose = new();
        public BasicMoveInfo blockingAirHit = new();
        public BasicMoveInfo parryCrouching = new();
        public BasicMoveInfo parryHigh = new();
        public BasicMoveInfo parryLow = new();
        public BasicMoveInfo parryAir = new();
        public BasicMoveInfo groundBounce = new();
        public BasicMoveInfo standingWallBounce = new();
        public BasicMoveInfo standingWallBounceKnockdown = new();
        public BasicMoveInfo airWallBounce = new();
        public BasicMoveInfo fallingFromGroundBounce = new();
        public BasicMoveInfo fallingFromAirHit = new();
        public BasicMoveInfo fallDown = new();
        public BasicMoveInfo airRecovery = new();
        public BasicMoveInfo getHitCrouching = new();
        public BasicMoveInfo getHitHigh = new();
        public BasicMoveInfo getHitLow = new();
        public BasicMoveInfo getHitHighKnockdown = new();
        public BasicMoveInfo getHitMidKnockdown = new();
        public BasicMoveInfo getHitAir = new();
        public BasicMoveInfo getHitCrumple = new();
        public BasicMoveInfo getHitKnockBack = new();
        public BasicMoveInfo getHitSweep = new();
        public BasicMoveInfo standUp = new();
        public BasicMoveInfo standUpFromAirHit = new();
        public BasicMoveInfo standUpFromKnockBack = new();
        public BasicMoveInfo standUpFromStandingHighHit = new();
        public BasicMoveInfo standUpFromStandingMidHit = new();
        public BasicMoveInfo standUpFromCrumple = new();
        public BasicMoveInfo standUpFromSweep = new();
        public BasicMoveInfo standUpFromStandingWallBounce = new();
        public BasicMoveInfo standUpFromAirWallBounce = new();
        public BasicMoveInfo standUpFromGroundBounce = new();
        public BasicMoveInfo intro = new();
        public BasicMoveInfo roundWon = new();
        public BasicMoveInfo timeOut = new();
        public BasicMoveInfo gameWon = new();


        public bool moveEnabled = true;
        public bool jumpEnabled = true;
        public bool crouchEnabled = true;
        public bool blockEnabled = true;
        public bool parryEnabled = true;

        public Dictionary<BasicMoveInfo, BasicMoveReference> basicMoveDictionary;

        public BasicMoves()
        {
            UpdateDictionary();
        }

        public void UpdateDictionary()
        {
            basicMoveDictionary = new Dictionary<BasicMoveInfo, BasicMoveReference>
            {
                { idle, BasicMoveReference.Idle },
                { moveForward, BasicMoveReference.MoveForward },
                { moveBack, BasicMoveReference.MoveBack },
                { moveSideways, BasicMoveReference.MoveSideways },
                { crouching, BasicMoveReference.Crouching },
                { takeOff, BasicMoveReference.TakeOff },
                { jumpStraight, BasicMoveReference.JumpStraight },
                { jumpBack, BasicMoveReference.JumpBack },
                { jumpForward, BasicMoveReference.JumpForward },
                { fallStraight, BasicMoveReference.FallStraight },
                { fallBack, BasicMoveReference.FallBack },
                { fallForward, BasicMoveReference.FallForward },
                { landing, BasicMoveReference.Landing },
                { blockingCrouchingPose, BasicMoveReference.BlockingCrouchingPose },
                { blockingCrouchingHit, BasicMoveReference.BlockingCrouchingHit },
                { blockingHighPose, BasicMoveReference.BlockingHighPose },
                { blockingHighHit, BasicMoveReference.BlockingHighHit },
                { blockingLowHit, BasicMoveReference.BlockingLowHit },
                { blockingAirPose, BasicMoveReference.BlockingAirPose },
                { blockingAirHit, BasicMoveReference.BlockingAirHit },
                { parryCrouching, BasicMoveReference.ParryCrouching },
                { parryHigh, BasicMoveReference.ParryHigh },
                { parryLow, BasicMoveReference.ParryLow },
                { parryAir, BasicMoveReference.ParryAir },
                { groundBounce, BasicMoveReference.StageGroundBounce },
                { standingWallBounce, BasicMoveReference.StageStandingWallBounce },
                { standingWallBounceKnockdown, BasicMoveReference.StageStandingWallBounceKnockdown },
                { airWallBounce, BasicMoveReference.StageAirWallBounce },
                { fallingFromGroundBounce, BasicMoveReference.FallDownDefault },
                { fallingFromAirHit, BasicMoveReference.FallDownFromAirJuggle },
                { fallDown, BasicMoveReference.FallDownFromGroundBounce },
                { airRecovery, BasicMoveReference.AirRecovery },
                { getHitCrouching, BasicMoveReference.HitStandingCrouching },
                { getHitHigh, BasicMoveReference.HitStandingHigh },
                { getHitLow, BasicMoveReference.HitStandingLow },
                { getHitHighKnockdown, BasicMoveReference.HitStandingHighKnockdown },
                { getHitMidKnockdown, BasicMoveReference.HitStandingMidKnockdown },
                { getHitAir, BasicMoveReference.HitAirJuggle },
                { getHitCrumple, BasicMoveReference.HitCrumple },
                { getHitKnockBack, BasicMoveReference.HitKnockBack },
                { getHitSweep, BasicMoveReference.HitSweep },
                { standUp, BasicMoveReference.StandUpDefault },
                { standUpFromAirHit, BasicMoveReference.StandUpFromAirJuggle },
                { standUpFromKnockBack, BasicMoveReference.StandUpFromKnockBack },
                { standUpFromStandingHighHit, BasicMoveReference.StandUpFromStandingHighHit },
                { standUpFromStandingMidHit, BasicMoveReference.StandUpFromStandingMidHit },
                { standUpFromCrumple, BasicMoveReference.StandUpFromCrumple },
                { standUpFromSweep, BasicMoveReference.StandUpFromSweep },
                { standUpFromStandingWallBounce, BasicMoveReference.StandUpFromStandingWallBounce },
                { standUpFromAirWallBounce, BasicMoveReference.StandUpFromAirWallBounce },
                { standUpFromGroundBounce, BasicMoveReference.StandUpFromGroundBounce },
                { intro, BasicMoveReference.Intro },
                { roundWon, BasicMoveReference.RoundWon },
                { timeOut, BasicMoveReference.TimeOut },
                { gameWon, BasicMoveReference.GameWon }
            };

            foreach(KeyValuePair<BasicMoveInfo, BasicMoveReference> dic in basicMoveDictionary)
            {
                dic.Key.reference = dic.Value;
            }
        }

        public BasicMoveReference? GetBasicAnimationReference(string id)
        {
            foreach (BasicMoveInfo basicMove in basicMoveDictionary.Keys)
            {
                if (basicMove.id == id)
                {
                    return basicMoveDictionary[basicMove];
                }
            }

            return null;
        }

        public BasicMoveInfo GetBasicMoveInfo(BasicMoveReference basicMoveReference)
        {
            foreach (var dic in basicMoveDictionary)
            {
                if (dic.Value == basicMoveReference)
                    return dic.Key;
            }
            return null;
        }

        public object Clone()
        {
            return CloneObject.Clone(this);
        }
    }
}