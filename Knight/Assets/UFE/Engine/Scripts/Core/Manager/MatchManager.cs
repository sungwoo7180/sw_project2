using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FPLibrary;

namespace UFE3D
{
	public class MatchManager : MonoBehaviour
	{
		protected Text debugger;

        private void Start()
        {
			debugger = UFE.UIManager.debugger;
		}

        public void UpdateMatchState(long currentFrame,
			IDictionary<InputReferences, InputEvents> player1PreviousInputs, 
			IDictionary<InputReferences, InputEvents> player1CurrentInputs, 
			IDictionary<InputReferences, InputEvents> player2PreviousInputs, 
			IDictionary<InputReferences, InputEvents> player2CurrentInputs)
		{
			//-------------------------------------------------------------------------------------------------------------
			// Update the game state
			//-------------------------------------------------------------------------------------------------------------
			if (!UFE.IsPaused())
			{
				List<ControlsScript> allScripts = UFE.GetAllControlsScripts();

				// 1 - Update All ControlsScripts
				foreach (ControlsScript cScript in allScripts)
					this.UpdateCharacter(cScript, currentFrame, cScript.playerNum == 1 ? player1PreviousInputs : player2PreviousInputs, cScript.playerNum == 1 ? player1CurrentInputs : player2CurrentInputs);

				// 2 - Update Active Instantiated Objects
				this.UpdateInstantiatedObjects(currentFrame);

				// 3 - Check Collisions
				this.CheckCollision(allScripts);

				// 4 - Update Animations
				this.UpdateAnimation(allScripts);

				// 5 - Update Camera
				if (UFE.CameraScript != null) UFE.CameraScript.DoFixedUpdate();

				// 6 - Clear Instance Buffer
				this.MemoryCleaner(allScripts);

				// 7 - Update Timer
				this.UpdateTimer();

				// 8 - Check End Round Conditions
				if (UFE.gameRunning && !UFE.IsTimerPaused()) CheckEndRoundConditions();
			}


			if (debugger != null)
			{
				if (UFE.config.debugOptions.debugMode)
				{
					debugger.enabled = true;
					debugger.text = "";
					if (UFE.config.debugOptions.currentLocalFrame) debugger.text += "Current Frame:" + currentFrame + "\n";
					if (UFE.IsConnected)
					{
						if (UFE.config.debugOptions.ping) debugger.text += "Ping:" + UFE.MultiplayerAPI.GetLastPing() + " ms\n";
						if (UFE.config.debugOptions.frameDelay) debugger.text += "Frame Delay:" + UFE.FluxCapacitor.NetworkFrameDelay + "\n";
					}
				}
				else
				{
					debugger.enabled = false;
				}
			}
		}

		protected void UpdateCharacter(ControlsScript controlsScript, long currentFrame, IDictionary<InputReferences, InputEvents> previousInputs, IDictionary<InputReferences, InputEvents> currentInputs)
		{
			if (controlsScript != null && controlsScript.GetActive())
			{
				// .1 - Update Character
				controlsScript.DoFixedUpdate(previousInputs, currentInputs);
			}
		}

		protected void UpdateAnimation(List<ControlsScript> allScripts)
        {
			foreach (ControlsScript controlsScript in allScripts)
			{
				if (controlsScript != null && controlsScript.GetActive())
				{
					// .1 - Update Animations
					if (controlsScript.MoveSet != null && controlsScript.MoveSet.MecanimControl != null)
						controlsScript.MoveSet.MecanimControl.DoFixedUpdate();

					if (controlsScript.MoveSet != null && controlsScript.MoveSet.LegacyControl != null)
						controlsScript.MoveSet.LegacyControl.DoFixedUpdate();

					// .2 - Update Character Maps
					controlsScript.HitBoxes.UpdateMap();
				}
			}
        }

		protected void CheckCollision(List<ControlsScript> allScripts)
		{
			if (UFE.freezePhysics) return;

			foreach (ControlsScript controlsScript in allScripts)
			{
				if (controlsScript == null || !controlsScript.GetActive()) continue;

				// .1 - Check Blockable Area
				bool canBlock = false;

				if (UFE.config.blockOptions.blockType == BlockType.HoldBack || UFE.config.blockOptions.blockType == BlockType.AutoBlock)
				{
					ControlsScript opponent = controlsScript.opControlsScript;

					// Test contact with physical blockable areas
					bool isColliding = opponent.CheckBlockableAreaContact(controlsScript.HitBoxes.blockableArea, controlsScript.mirror > 0);
					if (!isColliding)
					{
						foreach (ProjectileMoveScript projectile in controlsScript.projectiles)
						{
							// Test projectile blocking
							if (projectile == null || !projectile.IsActive() || projectile.isHit > 0) continue;
							isColliding = projectile.blockableAreaIntersect;
							if (isColliding) break;
						}
					}

					if (isColliding) canBlock = true;

					if (canBlock && !opponent.blockStunned && opponent.currentSubState != SubStates.Stunned)
						opponent.CheckBlocking(true);
					else if (!canBlock && opponent.isBlocking)
						opponent.CheckBlocking(false);
				}

				// .2 - Check Physical Hits
				if (controlsScript.GetActive())
				{
					controlsScript.CheckHits(controlsScript.currentMove, controlsScript.opControlsScript);
					foreach (ControlsScript opAssist in controlsScript.opControlsScript.assists)
					{
						controlsScript.CheckHits(controlsScript.currentMove, opAssist);
					}
				}

				// .3 - Push other characters away from collision mass and body colliders
				if (UFE.config.selectedMatchType != MatchType.Singles)
				{
					foreach (ControlsScript cScript in UFE.GetControlsScriptTeam(controlsScript.opControlsScript.playerNum))
					{
						PushOpponentsAway(controlsScript, cScript);
						if (!controlsScript.isAssist) foreach (ControlsScript assist in cScript.assists) PushOpponentsAway(controlsScript, assist);
					}
				}
				else
				{
					PushOpponentsAway(controlsScript, controlsScript.opControlsScript);
					if (!controlsScript.isAssist) foreach (ControlsScript assist in controlsScript.opControlsScript.assists) PushOpponentsAway(controlsScript, assist);
				}
			}
		}


		private void PushOpponentsAway(ControlsScript controlsScript, ControlsScript opControlsScript)
		{
			if (opControlsScript == null
				|| !opControlsScript.GetActive()
				|| opControlsScript.HitBoxes == null
				|| controlsScript.ignoreCollisionMass
				|| opControlsScript.ignoreCollisionMass) return;


			// Set target in case its a 3D fighter
			FPVector target;
			if (!controlsScript.Physics.IsMoving())
				target = opControlsScript.worldTransform.position + opControlsScript.worldTransform.forward * -1;
			else
				target = controlsScript.worldTransform.position + controlsScript.worldTransform.forward * -1;

			ControlsScript cornerChar = opControlsScript;
			if (UFE.config.characterRotationOptions.allowCornerStealing)
				cornerChar = controlsScript;

			// Test collision between hitboxes
			Fix64 pushForce = CollisionManager.TestCollision(controlsScript.HitBoxes.hitBoxes, opControlsScript.HitBoxes.hitBoxes, false, false);
			if (pushForce > 0)
			{
				if (UFE.config.gameplayType == GameplayType._2DFighter)
				{
					if (controlsScript.worldTransform.position.x < opControlsScript.worldTransform.position.x)
					{
						controlsScript.worldTransform.Translate(new FPVector(.1 * -pushForce, 0, 0));
					}
					else if (controlsScript.worldTransform.position.x > UFE.config.selectedStage._leftBoundary)
					{
						controlsScript.worldTransform.Translate(new FPVector(.1 * pushForce, 0, 0));
					}

					if (opControlsScript.worldTransform.position.x >= UFE.config.selectedStage._rightBoundary)
						cornerChar.worldTransform.Translate(new FPVector(.1 * -pushForce, 0, 0));
					else if (opControlsScript.worldTransform.position.x <= UFE.config.selectedStage._leftBoundary)
						cornerChar.worldTransform.Translate(new FPVector(.1 * pushForce, 0, 0));
				}
#if !UFE_LITE && !UFE_BASIC
				else
				{
					if (!controlsScript.Physics.IsMoving()) pushForce *= -1;
					controlsScript.worldTransform.position = FPVector.MoveTowards(controlsScript.worldTransform.position, target, .1 * pushForce);
				}
#endif
			}

			pushForce = controlsScript.opControlsScript.myInfo.physics._groundCollisionMass + controlsScript.myInfo.physics._groundCollisionMass - FPVector.Distance(opControlsScript.worldTransform.position, controlsScript.worldTransform.position);
			if (pushForce > 0)
			{
				if (UFE.config.gameplayType == GameplayType._2DFighter)
				{
					if (controlsScript.worldTransform.position.x < opControlsScript.worldTransform.position.x)
					{
						controlsScript.worldTransform.Translate(new FPVector(.5 * -pushForce, 0, 0));
					}
					else if (controlsScript.worldTransform.position.x > UFE.config.selectedStage._leftBoundary)
					{
						controlsScript.worldTransform.Translate(new FPVector(.5 * pushForce, 0, 0));
					}

					if (opControlsScript.worldTransform.position.x >= UFE.config.selectedStage._rightBoundary)
						cornerChar.worldTransform.Translate(new FPVector(.5 * -pushForce, 0, 0));
					else if (opControlsScript.worldTransform.position.x <= UFE.config.selectedStage._leftBoundary)
						cornerChar.worldTransform.Translate(new FPVector(.5 * pushForce, 0, 0));
				}
				else
				{
					if (!controlsScript.Physics.IsMoving()) pushForce *= -1;
					controlsScript.worldTransform.position = FPVector.MoveTowards(controlsScript.worldTransform.position, target, .5 * pushForce);
				}
			}
		}

		protected void UpdateInstantiatedObjects(long currentFrame)
		{
			foreach (InstantiatedGameObject entry in UFE.instantiatedObjects.ToArray())
			{
				if (entry.gameObject == null) continue;

				if (entry.gameObject.activeInHierarchy)
				{
					if (entry.mrFusion != null)
					{
						if (currentFrame == entry.creationFrame)
						{
							entry.mrFusion.AssignComponents();
							entry.mrFusion.StartBehaviours();
						}

						if (currentFrame > entry.creationFrame)
						{
							entry.mrFusion.UpdateBehaviours();
						}

						if (currentFrame == entry.destructionFrame)
						{
							entry.mrFusion.DestroyEvent();
						}
					}

					if (entry.particles != null && currentFrame >= entry.creationFrame)
					{
						foreach (KeyValuePair<ParticleSystem,float> particleData in entry.particles)
						{
							if (UFE.config.networkOptions.particleRandomSeed)
							{
								var mainModule = particleData.Key.main;
								mainModule.simulationSpeed = (float)UFE.timeScale * particleData.Value;
							}

							float time = (currentFrame - entry.creationFrame) / (float)UFE.fps;
							particleData.Key.Simulate(time, true, true, true);
						}
					}
				}

				if (entry.destructionFrame != null)
				{
					entry.gameObject.SetActive(currentFrame >= entry.creationFrame && currentFrame < entry.destructionFrame);
				}
				else
				{
					entry.gameObject.SetActive(currentFrame >= entry.creationFrame);
				}
			}
		}

		protected void MemoryCleaner(List<ControlsScript> allScripts)
		{
			// Clean Spawn Pool
			if (UFE.instantiatedObjects.Count > 0 && UFE.instantiatedObjects.Count > UFE.config.networkOptions.spawnBuffer)
			{
				UnityEngine.Object.Destroy(UFE.instantiatedObjects[0].gameObject);
				UFE.instantiatedObjects.RemoveAt(0);
			}

			// Remove Destroyed Projectiles
			foreach (ControlsScript controlsScript in allScripts)
			{
				if (controlsScript.projectiles.Count > 0)
					controlsScript.projectiles.RemoveAll(item => item == null);
			}
		}

		protected List<ControlsScript> UpdateCSGroup(int playerNum, long currentFrame, IDictionary<InputReferences, InputEvents> playerPreviousInputs, IDictionary<InputReferences, InputEvents> playerCurrentInputs)
		{
			List<ControlsScript> cSList = new List<ControlsScript>();
			if (UFE.config.selectedMatchType != MatchType.Singles)
			{
				cSList = UFE.GetControlsScriptTeam(playerNum);
			}
			else
			{
				cSList.Add(UFE.GetControlsScript(playerNum));
			}

			List<ControlsScript> activeCScripts = new List<ControlsScript>();
			foreach (ControlsScript i_cScript in cSList)
			{
				activeCScripts.Add(i_cScript);
				this.UpdateCharacter(i_cScript, currentFrame, playerPreviousInputs, playerCurrentInputs);

				foreach (ControlsScript csAssist in i_cScript.assists)
				{
					activeCScripts.Add(csAssist);
					this.UpdateCharacter(csAssist, currentFrame, playerPreviousInputs, playerCurrentInputs);
				}
			}

			return activeCScripts;
		}

		protected void UpdateTimer()
		{
			if (!UFE.gameRunning) return;

			if (UFE.config.roundOptions.hasTimer && UFE.timer > 0 && !UFE.IsTimerPaused())
			{
				if (UFE.gameMode != GameMode.ChallengeMode && (UFE.gameMode != GameMode.TrainingRoom || (UFE.gameMode == GameMode.TrainingRoom && !UFE.config.trainingModeOptions.freezeTime)))
				{
					UFE.timer -= UFE.fixedDeltaTime * (UFE.config.roundOptions._timerSpeed * .01);
				}
				if (UFE.timer < UFE.intTimer)
				{
					UFE.intTimer--;
					UFE.FireTimer((float)UFE.timer);
				}
			}

			if (UFE.timer < 0)
			{
				UFE.timer = 0;
			}
			if (UFE.intTimer < 0)
			{
				UFE.intTimer = 0;
			}

			if (UFE.timer == 0 && !UFE.config.lockMovements)
			{
				UFE.config.lockMovements = true;
				UFE.config.lockInputs = true;
				EndRound();
			}
		}

		protected void CheckEndRoundConditions()
		{
			if (UFE.GetControlsScript(1).currentLifePoints == 0 || UFE.GetControlsScript(2).currentLifePoints == 0)
			{
				UFE.FireAlert(UFE.config.selectedLanguage.ko, null);

				if (UFE.GetControlsScript(1).currentLifePoints == 0) UFE.PlaySound(UFE.GetControlsScript(1).myInfo.deathSound);
				if (UFE.GetControlsScript(2).currentLifePoints == 0) UFE.PlaySound(UFE.GetControlsScript(2).myInfo.deathSound);

				UFE.PauseTimer();
				if (!UFE.config.roundOptions.allowMovementEnd)
				{
					UFE.config.lockMovements = true;
					UFE.config.lockInputs = true;
				}

				if (UFE.config.roundOptions.slowMotionKO)
				{
					UFE.DelaySynchronizedAction(this.ReturnTimeScale, UFE.config.roundOptions._slowMoTimer);
					UFE.DelaySynchronizedAction(this.EndRound, 1 / UFE.config.roundOptions._slowMoSpeed);
					UFE.timeScale *= UFE.config.roundOptions._slowMoSpeed;
				}
				else
				{
					UFE.DelaySynchronizedAction(this.EndRound, (Fix64)1);
				}
			}
		}

		public void ReturnTimeScale()
		{
			UFE.timeScale = UFE.config._gameSpeed;
		}

		public void EndRound()
		{
			ControlsScript p1ControlsScript = UFE.GetControlsScript(1);
			ControlsScript p2ControlsScript = UFE.GetControlsScript(2);

			// Make sure both characters are grounded
			if (!p1ControlsScript.Physics.IsGrounded() || !p2ControlsScript.Physics.IsGrounded())
			{
				UFE.DelaySynchronizedAction(this.EndRound, .5);
				return;
			}

			UFE.config.lockMovements = true;
			UFE.config.lockInputs = true;

			// Reset Stats
			p1ControlsScript.KillCurrentMove();
			p2ControlsScript.KillCurrentMove();

			p1ControlsScript.ResetDrainStatus(true);
			p2ControlsScript.ResetDrainStatus(true);

			// Clear All Projectiles
			foreach (ProjectileMoveScript projectile in p1ControlsScript.projectiles)
			{
				if (projectile != null) UFE.DestroyGameObject(projectile.gameObject);
			}
			foreach (ProjectileMoveScript projectile in p2ControlsScript.projectiles)
			{
				if (projectile != null) UFE.DestroyGameObject(projectile.gameObject);
			}

			// Deactivate All Assists
			foreach (ControlsScript cScript in UFE.GetAllControlsScripts())
			{
				foreach (ControlsScript assist in cScript.assists)
				{
					assist.SetActive(false);
				}
			}

			// Check Winner
			ControlsScript winner = null;
			ControlsScript loser = null;
			if (UFE.timer == 0)
			{
				Fix64 p1LifePercentage = p1ControlsScript.currentLifePoints / p1ControlsScript.myInfo.lifePoints;
				Fix64 p2LifePercentage = p2ControlsScript.currentLifePoints / p2ControlsScript.myInfo.lifePoints;
				UFE.PauseTimer();
				UFE.config.lockMovements = true;
				UFE.config.lockInputs = true;

				UFE.FireTimeOver();


				// Check Winner
				if (p1LifePercentage == p2LifePercentage)
				{
					p1ControlsScript.SetMoveToOutro(3);
					p2ControlsScript.SetMoveToOutro(3);
					UFE.FireAlert(UFE.config.selectedLanguage.draw, null);
					UFE.DelaySynchronizedAction(this.NewRound, UFE.config.roundOptions._newRoundDelay);
				}
				else
				{
					winner = (p1LifePercentage > p2LifePercentage) ? p1ControlsScript : p2ControlsScript;
					loser = (winner == p1ControlsScript) ? p2ControlsScript : p1ControlsScript;

					loser.SetMoveToOutro(3);
				}
			}
			else
			{
				if (p1ControlsScript.currentLifePoints == 0 && p2ControlsScript.currentLifePoints == 0)
				{
					UFE.FireAlert(UFE.config.selectedLanguage.draw, null);
					UFE.DelaySynchronizedAction(this.NewRound, UFE.config.roundOptions._newRoundDelay);
				}
				else
				{
					if (p1ControlsScript.currentLifePoints == 0)
					{
						winner = p2ControlsScript;
					}
					else if (p2ControlsScript.currentLifePoints == 0 || UFE.gameMode == GameMode.ChallengeMode)
					{
						winner = p1ControlsScript;
					}
				}
			}

			// Start New Round or End Game
			if (winner)
			{
				++winner.roundsWon;
				if (winner.roundsWon > Mathf.Ceil(UFE.config.roundOptions.totalRounds / 2) || UFE.challengeMode != null)
				{
					winner.SetMoveToOutro(1);
					UFE.DelaySynchronizedAction(this.EndMatch, UFE.config.roundOptions._endGameDelay);
					UFE.FireGameEnds(winner, winner.opControlsScript);
				}
				else
				{
					winner.SetMoveToOutro(2);
					UFE.DelaySynchronizedAction(this.NewRound, UFE.config.roundOptions._newRoundDelay);
				}
			}

			UFE.FireRoundEnds(winner, winner == null ? null : winner.opControlsScript);
		}

		public void EndMatch()
		{
			UFE.EndGame(false);
			UFE.CameraScript.killCamMove = true;
		}

		public void NewRound()
		{
			ControlsScript p1ControlScript = UFE.GetControlsScript(1);
			ControlsScript p2ControlScript = UFE.GetControlsScript(2);

			p1ControlScript.potentialBlock = false;
			p2ControlScript.potentialBlock = false;
			if (UFE.config.roundOptions.resetPositions)
			{
				CameraFade.StartAlphaFade(UFE.config.gameGUI.roundFadeColor, false, (float)UFE.config.gameGUI.roundFadeDuration / 2);
				UFE.DelaySynchronizedAction(this.StartNewRound, UFE.config.gameGUI.roundFadeDuration / 2);
			}
			else
			{
				UFE.DelaySynchronizedAction(this.StartNewRound, (Fix64)2);
			}

			//if (UFE.challengeMode != null) UFE.challengeMode.Run();
		}

		protected void StartNewRound()
		{
			ControlsScript p1ControlScript = UFE.GetControlsScript(1);
			ControlsScript p2ControlScript = UFE.GetControlsScript(2);

			UFE.config.currentRound++;
			UFE.ResetTimer();

			p1ControlScript.ResetData(false); // Set it to true in case its challenge mode
			p2ControlScript.ResetData(false);
			if (UFE.config.roundOptions.resetPositions)
			{
				p1ControlScript.worldTransform.position = UFE.config.roundOptions._p1XPosition;
				p2ControlScript.worldTransform.position = UFE.config.roundOptions._p2XPosition;

				CameraFade.StartAlphaFade(UFE.config.gameGUI.roundFadeColor, true, (float)UFE.config.gameGUI.roundFadeDuration / 2);
				UFE.CameraScript.ResetCam();

#if !UFE_LITE && !UFE_BASIC
				if (UFE.config.gameplayType == GameplayType._3DFighter)
				{
					p1ControlScript.LookAtTarget();
					p2ControlScript.LookAtTarget();
				}
				else if (UFE.config.gameplayType == GameplayType._3DArena)
				{
					p1ControlScript.worldTransform.rotation = FPQuaternion.Euler(UFE.config.roundOptions._p1XRotation);
					p2ControlScript.worldTransform.rotation = FPQuaternion.Euler(UFE.config.roundOptions._p2XRotation);
				}
#endif
			}

			UFE.config.lockInputs = true;

			UFE.CastNewRound();

			if (UFE.config.roundOptions.allowMovementStart)
			{
				UFE.config.lockMovements = false;
			}
			else
			{
				UFE.config.lockMovements = true;
			}
		}

		public virtual void StartReplay(FluxGameReplay replay)
		{
			if (replay != null && replay.Player1InputBuffer != null && replay.Player2InputBuffer != null)
			{
				FluxStateTracker.LoadGameState(replay.InitialState);
				UFE.FluxCapacitor.PlayerManager.GetPlayer(1)._inputBuffer = replay.Player1InputBuffer;
				UFE.FluxCapacitor.PlayerManager.GetPlayer(2)._inputBuffer = replay.Player2InputBuffer;
			}
		}

		public virtual void LoadReplayBuffer(List<FluxStates> replayData, int frame)
		{
			UFE.currentFrame = replayData[frame].NetworkFrame;
			FluxStateTracker.LoadGameState(replayData[frame]);


			if (UFE.config.debugOptions.playbackPhysics)
			{
				UFE.GameManager.UpdateGameState(UFE.currentFrame);
				UFE.FluxCapacitor.PlayerManager.Initialize(UFE.currentFrame);
			}

			UFE.UIManager.UpdateUIState();
		}
	}
}