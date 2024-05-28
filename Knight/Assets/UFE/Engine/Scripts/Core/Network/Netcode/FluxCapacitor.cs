using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Text;
using UFENetcode;
using FPLibrary;
using UFE3D;

public class FluxCapacitor
{
    #region public class properties
    public static string PlayerIndexOutOfRangeMessage =
    "The Player Index is {0}, but it should be in the [{1}, {2}] range.";

    public static string NetworkMessageFromUnexpectedPlayerMessage =
    "The Network Message was sent by {0}, but it was expected to be sent by {1}.";
    #endregion

    #region public instance properties
    public bool AllowRollbacks
    {
        get
        {
            //---------------------------------------------------------------------------------------------------------
            // Take into account that we will disable the remote player input prediction
            // in menu screens because we want this algorithm to behave as the frame-delay
            // algorithm in those screens (they aren't ready for dealing with rollbacks).
            //---------------------------------------------------------------------------------------------------------
            // FIXME: The current code will probably fail at "pause screen" and "after battle screens".
            //
            // Because when we try to disable rollbacks again, it's possible we already have some predicted inputs 
            // from the other player. A possible hack would be reseting the UFE.currentNetworkFrame and the input 
            // buffer when we detect one of these events, but we aren't completely sure about the undesirable 
            // side-effects which can appear.
            //---------------------------------------------------------------------------------------------------------
#if UFE_LITE || UFE_BASIC || UFE_STANDARD
            return false;
#else
            return UFE.config.rollbackEnabled && UFE.gameRunning && UFE.IsConnected;
#endif
        }
    }

    public FluxGameHistory History
    {
        get
        {
            return this._history;
        }
    }

    public int NetworkFrameDelay
    {
        get
        {
            int frameDelay;
            if (UFE.config.networkOptions.frameDelayType == UFE3D.NetworkFrameDelay.Auto)
            {
                frameDelay = this.GetOptimalFrameDelay();

                if (this.AllowRollbacks)
                {
                    //---------------------------------------------------------------------------------------------
                    // TODO: if one of the players get consistently more rollbacks than the other player, 
                    // then we should increase the frame delay for that player in 1 or 2 frames because
                    // using a greater frame-delay means having more input lag, but also less rollbacks.
                    //---------------------------------------------------------------------------------------------
                    // Another solution would be pausing the client which is receiving more rollbacks 
                    // for a single frame in order to give the other client some time to catch up.
                    //---------------------------------------------------------------------------------------------
                }
            }
            else
            {
                frameDelay = UFE.config.networkInputDelay;
            }

            return frameDelay;
        }
    }

    public FluxPlayerManager PlayerManager
    {
        get
        {
            return this._playerManager;
        }
    }
    #endregion

    #region public instance fields
    public FluxStates? savedState = null;
    #endregion

    #region protected instance fields
    protected FluxGameHistory _history = new FluxGameHistory();
    protected FluxPlayerManager _playerManager = new FluxPlayerManager();
    protected List<byte[]> _receivedNetworkMessages = new List<byte[]>();
    protected sbyte?[] _selectedOptions = new sbyte?[2];

    protected List<FluxSyncState> _localSynchronizationStates = new List<FluxSyncState>();
    protected List<FluxSyncState> _remoteSynchronizationStates = new List<FluxSyncState>();

    public long _remotePlayerNextExpectedFrame;
    protected bool _rollbackBalancingApplied;
    protected long _timeToNetworkMessage;
    protected long _lastSyncFrameSent;
    protected bool initializing;
    #endregion

    #region public instance constructors and initializers
    public FluxCapacitor(long currentFrame)
    {
        this.Initialize(currentFrame);
    }

    public virtual void Initialize(long currentFrame = 0)
    {
        int maxBufferSize = UFE.config.networkOptions.maxBufferSize;
        this.savedState = null;
        this._localSynchronizationStates.Clear();
        this._remoteSynchronizationStates.Clear();
        this._history.Initialize(currentFrame, maxBufferSize);
        this._remotePlayerNextExpectedFrame = currentFrame;
        this._rollbackBalancingApplied = false;
        this._timeToNetworkMessage = 0L;
        this._lastSyncFrameSent = 0L;
        this.PlayerManager.Initialize(currentFrame, maxBufferSize);

        UFE.currentFrame = currentFrame;
    }
    #endregion


    #region public instance methods
    public void DoFixedUpdate()
    {
        bool allowRollbacks = this.AllowRollbacks;
        long currentFrame = UFE.currentFrame;
        long frameDelay = this.NetworkFrameDelay;
        long remotePlayerLastFrameReceived = this._remotePlayerNextExpectedFrame - 1;
        long remotePlayerExpectedFrame = remotePlayerLastFrameReceived + frameDelay;

        long firstFrameWhereRollbackIsRequired = this.PlayerManager.GetFirstFrameWhereRollbackIsRequired();
        bool rollback = firstFrameWhereRollbackIsRequired >= 0 && firstFrameWhereRollbackIsRequired < UFE.currentFrame;
        long lastFrameWithConfirmedInput = this.PlayerManager.GetLastFrameWithConfirmedInput();


        //---------------------------------------------------------------------------------------------------------
        // Process the received the network messages
        //---------------------------------------------------------------------------------------------------------
        UFE.ConnectionHandler.ProcessReceivedNetworkMessages();


        //---------------------------------------------------------------------------------------------------------
        // If rollback balancing is enabled and it hasn't been applied in the current frame,
        // check if we need to apply the rollback balancing on this client.
        //
        // In order to avoid visual glitches, we want apply the rollback balancing at most one frame every second,
        // but we can become more aggressive if the desynchronization between clients is very big. If one client 
        // simulation is far ahead of the other client simulation (1 second or more), we pause that simulation
        // until the other client has time to catch up.
        //---------------------------------------------------------------------------------------------------------
        long rollbackBalancingFrameDelay = System.Math.Max(frameDelay, this.GetOptimalFrameDelay());
        if (UFE.config.networkOptions.rollbackBalancing != NetworkRollbackBalancing.Disabled &&
            (currentFrame > remotePlayerExpectedFrame + UFE.config.fps
            ||
            (
                !this._rollbackBalancingApplied
                &&
                (
                    (UFE.currentFrame % UFE.config.fps == 0 && currentFrame > remotePlayerExpectedFrame + rollbackBalancingFrameDelay / 2)
                    ||
                    (UFE.config.networkOptions.rollbackBalancing == NetworkRollbackBalancing.Aggressive &&
                    (
                    (UFE.currentFrame % (UFE.config.fps / 4) == 0 && currentFrame > remotePlayerExpectedFrame + rollbackBalancingFrameDelay * 2)
                    ||
                    (UFE.currentFrame % (UFE.config.fps / 2) == 0 && currentFrame > remotePlayerExpectedFrame + rollbackBalancingFrameDelay)
                    ))
                )
            ))
        )
        {
            //-----------------------------------------------------------------------------------------------------
            // If the game simulation on this client is far ahead in front of the simulation on the other client,
            // we will pause this client for a single frame in order to give the other simulation some time to 
            // catch up.
            //-----------------------------------------------------------------------------------------------------
            if (UFE.config.debugOptions.rollbackLog)
                Debug.Log("Game paused for one frame (Rollback Balancing Algorithm)");

            this._rollbackBalancingApplied = true;
            this.CheckOutgoingNetworkMessages(currentFrame);
            return;
        }
        else
        {
            this.ReadInputs(frameDelay, allowRollbacks);
            this.CheckOutgoingNetworkMessages(currentFrame);
        }


        //-------------------------------------------------------------------------------------------------------------
        // Clean oldest records from history and input buffers
        //-------------------------------------------------------------------------------------------------------------
        ClearBuffer(currentFrame);


        //-------------------------------------------------------------------------------------------------------------
        // Check if it's a network game and we need to apply a rollback...
        //-------------------------------------------------------------------------------------------------------------
        if (rollback)
        {
            if (allowRollbacks)
            {
            // Check if we need to rollback to a previous frame...
            this.Rollback(currentFrame, firstFrameWhereRollbackIsRequired, lastFrameWithConfirmedInput);
            }
            else
            {
                // If a desynchronization has happened and we don't allow rollbacks, 
                // show a log message and go to the "Connection Lost" screen.
                Debug.LogError("Game Desynchronized because a rollback was required, but not allowed.");
                UFE.MultiplayerAPI.LeaveMatch();
            }
        }


        //-------------------------------------------------------------------------------------------------------------
        // We need to update these values again because they may have changed during the rollback and fast-foward
        //-------------------------------------------------------------------------------------------------------------
        lastFrameWithConfirmedInput = this.PlayerManager.GetLastFrameWithConfirmedInput();
        currentFrame = UFE.currentFrame;
        long lastSyncFrame = currentFrame <= lastFrameWithConfirmedInput ? currentFrame : lastFrameWithConfirmedInput;


        //-------------------------------------------------------------------------------------------------------------
        // If the game isn't paused and all players have entered their input for the current frame...
        //-------------------------------------------------------------------------------------------------------------
        if (this.PlayerManager.TryCheckIfInputIsReady(UFE.currentFrame, out bool isInputReady) && isInputReady)
        {
            UFE.GameManager.UpdateGameState(currentFrame);
            this._rollbackBalancingApplied = false;

            //-------------------------------------------------------------------------------------------------------------
            // Sync handling tool
            //-------------------------------------------------------------------------------------------------------------
            if (UFE.gameRunning
                && !UFE.config.lockInputs
                && UFE.config.networkOptions.synchronizationAction != NetworkSynchronizationAction.Disabled
                && lastSyncFrame > _lastSyncFrameSent
                && _history.TryGetState(lastSyncFrame, out FluxStates confirmedState)
                && confirmedState.global.currentRound > 0)
            {
                // Generate positions or key log size to be compared with received state
                FluxSyncState? expectedState = new FluxSyncState(confirmedState);

                // Add state from last synchronized frame to the synchrnoization list
                AddSynchronizationState(_localSynchronizationStates, lastSyncFrame, expectedState.Value);

                // Send last synchrnozed frame along with expected state
                if (UFE.config.networkOptions.synchronizationMessageFrequency == NetworkInputMessageFrequency.EveryFrame ||
                    (UFE.config.networkOptions.synchronizationMessageFrequency == NetworkInputMessageFrequency.Every2Frames && (lastSyncFrame - _lastSyncFrameSent) >= 2) ||
                    (UFE.config.networkOptions.synchronizationMessageFrequency == NetworkInputMessageFrequency.Every3Frames && (lastSyncFrame - _lastSyncFrameSent) >= 3) ||
                    (UFE.config.networkOptions.synchronizationMessageFrequency == NetworkInputMessageFrequency.Every4Frames && (lastSyncFrame - _lastSyncFrameSent) >= 4) ||
                    (UFE.config.networkOptions.synchronizationMessageFrequency == NetworkInputMessageFrequency.Every5Frames && (lastSyncFrame - _lastSyncFrameSent) >= 5))
                {
                    UFE.MultiplayerAPI.SendNetworkMessage(new SynchronizationMessage(UFE.GetLocalPlayer(), lastSyncFrame, expectedState.Value));
                    _lastSyncFrameSent = lastSyncFrame;
                }

                // After sending the network message, check if we already have a "received state" for that frame
                FluxSyncState? receivedState = GetSimpleState(_remoteSynchronizationStates, lastSyncFrame);

                // If we do, compare states
                if (expectedState != null && receivedState != null)
                    SynchronizationCheck(expectedState.Value, receivedState.Value, lastSyncFrame);
            }
        }
    }

    public virtual int GetOptimalFrameDelay()
    {
        return this.GetOptimalFrameDelay(UFE.MultiplayerAPI.GetLastPing());
    }

    public virtual int GetOptimalFrameDelay(int ping)
    {
        //-------------------------------------------------------------------------------------------------------------
        // Measure the time that a message needs to arrive at the other client and  calculate the duration
        // of each frame in seconds, so we can calculate the number of frames that will pass before the
        // network message arrives at the other client: that value will be the frame-delay.
        //-------------------------------------------------------------------------------------------------------------
        Fix64 latency = 0.001 * 0.5 * (Fix64)ping;
        Fix64 frameDuration = 1 / (Fix64)UFE.config.fps;

        //-------------------------------------------------------------------------------------------------------------
        // Add one additional frame to the frame-delay, to compensate that messages could not being sent
        // until the next frame.
        //-------------------------------------------------------------------------------------------------------------
        int frameDelay = (int)FPMath.Ceiling(latency / frameDuration) + 1;
        return Mathf.Clamp(frameDelay, UFE.config.networkOptions.minFrameDelay, UFE.config.networkOptions.maxFrameDelay);
    }

    public virtual void RequestOptionSelection(int player, sbyte option)
    {
        if (player == 1 || player == 2)
        {
            this._selectedOptions[player - 1] = option;
        }
    }

    public void ClearBuffer(long currentFrame)
    {
        long firstFrameWhereRollbackIsRequired = this.PlayerManager.GetFirstFrameWhereRollbackIsRequired();
        long lastFrameWithConfirmedInput = this.PlayerManager.GetLastFrameWithConfirmedInput();
        long lastFrameWithSynchronizationMessage = Math.Min(this.GetFirstLocalSynchronizationFrame(), this.GetFirstRemoteSynchronizationFrame());
        long lastFrameWithSynchronizedInput = firstFrameWhereRollbackIsRequired >= 0 ? firstFrameWhereRollbackIsRequired - 1L : lastFrameWithConfirmedInput;

        //-------------------------------------------------------------------------------------------------------------
        // Remove the information which is no longer necessary:
        //-------------------------------------------------------------------------------------------------------------
        // We need to leave the confirmed information for a few extra frames
        // because we may need them later during a rollback.
        //-------------------------------------------------------------------------------------------------------------
        while (
            this.PlayerManager.player1.inputBuffer.FirstFrame < currentFrame - 1L
            &&
            this.PlayerManager.player1.inputBuffer.FirstFrame < lastFrameWithSynchronizedInput - 1L
            &&
            this.PlayerManager.player1.inputBuffer.FirstFrame < this._remotePlayerNextExpectedFrame
            &&
            (
                this.PlayerManager.player1.inputBuffer.FirstFrame < lastFrameWithSynchronizationMessage - 1L
                ||
                this.PlayerManager.player1.inputBuffer.MaxBufferSize > 0 &&
                this.PlayerManager.player1.inputBuffer.Count > this.PlayerManager.player1.inputBuffer.MaxBufferSize * 3 / 4
            )
        )
        {
            this.PlayerManager.player1.inputBuffer.RemoveNextInput();
        }

        while (
            this.PlayerManager.player2.inputBuffer.FirstFrame < currentFrame - 1L
            &&
            this.PlayerManager.player2.inputBuffer.FirstFrame < lastFrameWithSynchronizedInput - 1L
            &&
            this.PlayerManager.player2.inputBuffer.FirstFrame < this._remotePlayerNextExpectedFrame
            &&
            (
                this.PlayerManager.player2.inputBuffer.FirstFrame < lastFrameWithSynchronizationMessage - 1L
                ||
                this.PlayerManager.player2.inputBuffer.MaxBufferSize > 0 &&
                this.PlayerManager.player2.inputBuffer.Count > this.PlayerManager.player2.inputBuffer.MaxBufferSize * 3 / 4
            )
        )
        {
            this.PlayerManager.player2.inputBuffer.RemoveNextInput();
        }

        while (
            this._history.FirstStoredFrame < currentFrame - 1L
            &&
            this._history.FirstStoredFrame < lastFrameWithSynchronizedInput - 1L
            &&
            this._history.FirstStoredFrame < this._remotePlayerNextExpectedFrame
            &&
            (
                this._history.FirstStoredFrame < lastFrameWithSynchronizationMessage - 1L
                ||
                this._history.MaxBufferSize > 0 &&
                this._history.Count > this._history.MaxBufferSize * 3 / 4
            )
        )
        {
            this._history.RemoveNextFrame();
        }

        if (!UFE.IsConnected)
        {
            this._remotePlayerNextExpectedFrame = lastFrameWithSynchronizedInput + 1L;
        }
    }

    #endregion

    #region private instance mehtods
    private void CheckOutgoingNetworkMessages(long currentFrame)
    {
        //---------------------------------------------------------------------------------------------------------
        // Check if we need to send a network message
        //---------------------------------------------------------------------------------------------------------
        if (UFE.config.networkOptions.inputMessageFrequency == NetworkInputMessageFrequency.EveryFrame)
        {
            //-----------------------------------------------------------------------------------------------------
            // We may want to send a network message every frame...
            //-----------------------------------------------------------------------------------------------------
            this.SendNetworkMessages();
        }
        else
        {
            //-----------------------------------------------------------------------------------------------------
            // Or we may want to send a network message every few frames...
            //-----------------------------------------------------------------------------------------------------
            if (this._timeToNetworkMessage <= 0L)
            {
                this.SendNetworkMessages();
            }
            else
            {
                int localPlayer = UFE.GetLocalPlayer();
                if (localPlayer > 0)
                {
                    FrameInput? previousFrameInput;
                    FrameInput? currentFrameInput;

                    if (
                        this.PlayerManager.TryGetInput(localPlayer, currentFrame - 1, out previousFrameInput) &&
                        previousFrameInput != null &&
                        this.PlayerManager.TryGetInput(localPlayer, currentFrame, out currentFrameInput) &&
                        currentFrameInput != null &&
                        !previousFrameInput.Value.Equals(currentFrameInput.Value)
                    )
                    {
                        //-----------------------------------------------------------------------------------------
                        // Even if we want to send the network message every few frames, 
                        // we send the network message immediately if the local player
                        // input has changed since the previous frame.
                        //
                        // We do this to avoid "mega-rollbacks" which can kill the game
                        // performance during the "fast-forward" phase.
                        //-----------------------------------------------------------------------------------------
                        this.SendNetworkMessages();
                    }
                }
            }

            --this._timeToNetworkMessage;
        }
    }

    private FluxSyncState? GetSimpleState(List<FluxSyncState> stateList, long frame)
    {
        for (int i = 0; i < stateList.Count; ++i)
        {
            if (stateList[i].frame == frame)
            {
                return stateList[i];
            }
        }

        return null;
    }

    public long GetFirstLocalSynchronizationFrame()
    {
        long frame = -1L;

        for (int i = this._localSynchronizationStates.Count - 1; i >= 0; --i)
        {
            if (frame < 0 || frame > this._localSynchronizationStates[i].frame)
            {
                frame = this._localSynchronizationStates[i].frame;
            }
        }

        return frame;
    }

    public long GetFirstRemoteSynchronizationFrame()
    {
        long frame = -1L;

        for (int i = this._remoteSynchronizationStates.Count - 1; i >= 0; --i)
        {
            if (frame < 0 || frame > this._remoteSynchronizationStates[i].frame)
            {
                frame = this._remoteSynchronizationStates[i].frame;
            }
        }

        return frame;
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Processes the specified input network package.
    /// </summary>
    /// <param name="package">Network package.</param>
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public void ProcessInputBufferMessage(InputBufferMessage package)
    {
        // Check if the player number included in the package is valid...
        int playerIndex = package.PlayerIndex;
        if (playerIndex <= 0 || playerIndex > FluxPlayerManager.NumberOfPlayers)
        {
            throw new IndexOutOfRangeException(string.Format(
                FluxCapacitor.PlayerIndexOutOfRangeMessage,
                playerIndex,
                1,
                FluxPlayerManager.NumberOfPlayers
            ));
        }

        long previousGetLastFrameWithConfirmedInput = this.PlayerManager.GetLastFrameWithConfirmedInput();

        this._remotePlayerNextExpectedFrame = Math.Max(
            this._remotePlayerNextExpectedFrame,
            package.Data.NextExpectedFrame
        );

        // If we want to send only the input changes, we need to remove repeated inputs from the buffer...
        if (UFE.config.networkOptions.onlySendInputChanges)
        {
            int count = package.Data.InputBuffer.Count;

            if (count > 0)
            {
                // First, process the inputs of the first frame in the list...
                this.ProcessInput(playerIndex, package.Data.InputBuffer[0], previousGetLastFrameWithConfirmedInput);

                // Iterate over the rest of the items of the list except the last one...
                for (int i = 1; i < package.Data.InputBuffer.Count; ++i)
                {
                    Tuple<long, FrameInput> previousInput = package.Data.InputBuffer[i - 1];
                    Tuple<long, FrameInput> currentInput = package.Data.InputBuffer[i];

                    if (previousInput != null && currentInput != null)
                    {
                        // Repeat the previous input from the last updated frame to the frame before the new input
                        for (long j = previousInput.Item1 + 1L; j < currentInput.Item1; ++j)
                        {
                            this.ProcessInput(
                                playerIndex,
                                new Tuple<long, FrameInput>(j, new FrameInput(previousInput.Item2)),
                                previousGetLastFrameWithConfirmedInput
                            );
                        }

                        // Now process the new input
                        this.ProcessInput(playerIndex, currentInput, previousGetLastFrameWithConfirmedInput);
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < package.Data.InputBuffer.Count; ++i)
            {
                this.ProcessInput(playerIndex, package.Data.InputBuffer[i], previousGetLastFrameWithConfirmedInput);
            }
        }
    }

    private void ProcessInput(int playerIndex, Tuple<long, FrameInput> frame, long lastFrameWithConfirmedInput)
    {
        long currentFrame = frame.Item1;
        this.PlayerManager.TrySetConfirmedInput(playerIndex, currentFrame, frame.Item2);
    }

    public void ProcessSynchronizationMessage(SynchronizationMessage msg)
    {
        if (!UFE.gameRunning) return;

        FluxSyncState receivedState = msg.Data;
        AddSynchronizationState(_remoteSynchronizationStates, msg.CurrentFrame, receivedState);

        // After receiving the network message, check if we already have a "local state" for that frame
        FluxSyncState? expectedState = GetSimpleState(_localSynchronizationStates, msg.CurrentFrame);

        // If we do, compare states
        if (expectedState != null)
            this.SynchronizationCheck(expectedState.Value, receivedState, msg.CurrentFrame);
    }

    private void SendNetworkMessages()
    {
        int localPlayer = UFE.GetLocalPlayer();

        if (localPlayer > 0)
        {
            FluxPlayer local = this.PlayerManager.GetPlayer(localPlayer);

            // And send a message with their current "confirmed input" buffer.
            if (local != null && local.inputBuffer != null)
            {
                IList<Tuple<long, FrameInput>> confirmedInputBuffer =
                    local.inputBuffer.GetConfirmedInputBuffer(this._remotePlayerNextExpectedFrame);

                // If we want to send only the input changes, we need to remove repeated inputs from the buffer...
                if (UFE.config.networkOptions.onlySendInputChanges && confirmedInputBuffer.Count > 1)
                {
                    IList<Tuple<long, FrameInput>> tempInputBuffer = confirmedInputBuffer;

                    // So copy the first item of the list
                    confirmedInputBuffer = new List<Tuple<long, FrameInput>>();
                    confirmedInputBuffer.Add(tempInputBuffer[0]);

                    // Iterate over the rest of the items in the list, except the last one
                    for (int i = 1; i < tempInputBuffer.Count - 1; ++i)
                    {
                        // If the player inputs has changed since the last frame, add the item to the list
                        Tuple<long, FrameInput> currentInput = tempInputBuffer[i];
                        Tuple<long, FrameInput> lastInput = confirmedInputBuffer[confirmedInputBuffer.Count - 1];

                        if (lastInput != null && currentInput != null && !currentInput.Item2.Equals(lastInput.Item2))
                        {
                            confirmedInputBuffer.Add(currentInput);
                        }
                    }

                    // Copy the last item of the list
                    confirmedInputBuffer.Add(tempInputBuffer[tempInputBuffer.Count - 1]);
                }

                if (confirmedInputBuffer.Count > 0)
                {
                    InputBufferMessage msg = new InputBufferMessage(
                        localPlayer,
                        local.inputBuffer.FirstFrame,
                        new InputBufferMessageContent(this.PlayerManager.GetNextExpectedFrame(), confirmedInputBuffer)
                    );

                    UFE.MultiplayerAPI.SendNetworkMessage(msg);


                    if (UFE.config.networkOptions.inputMessageFrequency == NetworkInputMessageFrequency.EveryFrame)
                    {
                        this._timeToNetworkMessage = 1L;
                    }
                    else if (UFE.config.networkOptions.inputMessageFrequency == NetworkInputMessageFrequency.Every2Frames)
                    {
                        this._timeToNetworkMessage = 2L;
                    }
                    else
                    {
                        this._timeToNetworkMessage = this.NetworkFrameDelay / 4L;
                    }
                }
            }
        }
    }

    private void Rollback(long currentFrame, long rollbackFrame, long lastFrameWithConfirmedInputs)
    {
#if UFE_LITE || UFE_BASIC || UFE_STANDARD
        Debug.LogError("Rollback not installed.");
#else
        // Retrieve the first stored frame and check if we can rollback to the specified frame...
        long firstStoredFrame = Math.Max(this.PlayerManager.player1.inputBuffer.FirstFrame, this.PlayerManager.player2.inputBuffer.FirstFrame);
        if (rollbackFrame > firstStoredFrame)
        {
            // Show the debug information to help us understand what has happened
            FluxPlayerInputBuffer p1Buffer = this.PlayerManager.player1.inputBuffer;
            FluxPlayerInputBuffer p2Buffer = this.PlayerManager.player2.inputBuffer;
            FluxPlayerInput p1Input = p1Buffer[p1Buffer.GetIndex(rollbackFrame)];
            FluxPlayerInput p2Input = p2Buffer[p2Buffer.GetIndex(rollbackFrame)];

            // Update the predicted inputs with the inputs which have been already confirmed
            for (long i = rollbackFrame; i <= lastFrameWithConfirmedInputs; ++i)
            {
                this.PlayerManager.TryOverridePredictionWithConfirmedInput(1, i);
                this.PlayerManager.TryOverridePredictionWithConfirmedInput(2, i);
            }

            // Reset the game to the state it had on the last consistent frame...
            this._history = FluxStateTracker.LoadGameState(this._history, rollbackFrame);
            //this.ApplyInputs(rollbackFrame, true);

            // And simulate all the frames after that fast-forward, so we return to the previous frame again...
            long fastForwardTarget = Math.Min(UFE.currentFrame, this._remotePlayerNextExpectedFrame - 1);
            long maxFastForwards = Math.Max(UFE.config.networkOptions.maxFastForwards, (currentFrame - fastForwardTarget) / 2L);
            long currentFastForwards = 0L;

            while (UFE.currentFrame < currentFrame && currentFastForwards < maxFastForwards)
            {
                UFE.GameManager.UpdateGameState(UFE.currentFrame);
                ++currentFastForwards;
                if (UFE.config.debugOptions.rollbackLog)
                    Debug.Log("Rollback applied from frame " + UFE.currentFrame + " to frame " + lastFrameWithConfirmedInputs);
            }
        }
        else if (UFE.config.debugOptions.rollbackLog)
        {
            Debug.Log("Failed because the specified frame is no longer stored in the Game History.");
        }
#endif
    }

    public void ReadInputs(long frameDelay, bool allowRollbacks)
    {
        //-------------------------------------------------------------------------------------------------------------
        // Read the player inputs (ensuring that there aren't any "holes" created by variable frame-delay).
        //-------------------------------------------------------------------------------------------------------------
        for (int i = 0; i <= frameDelay * 2; ++i)
        {
            long frame = UFE.currentFrame + i;

            for (int j = 1; j <= FluxPlayerManager.NumberOfPlayers; ++j)
            {
                if (this.PlayerManager.ReadInputs(j, frame, this._selectedOptions[j - 1], allowRollbacks))
                {
                    this._selectedOptions[j - 1] = null;
                }
            }
        }
    }

    protected void AddSynchronizationState(List<FluxSyncState> targetList, long frame, FluxSyncState state)
    {
        // Remove first element if list is too big
        if (targetList.Count > UFE.config.networkOptions.recordingBuffer)
            targetList.RemoveAt(0);

        bool stateFound = false;
        for (int i = 0; i < targetList.Count; i++)
        {
            if (targetList[i].frame == frame)
            {
                targetList[i] = state;
                stateFound = true;
                break;
            }
        }
        if (!stateFound)
            targetList.Add(state);
    }

    private bool SynchronizationCheck(FluxSyncState expectedState, FluxSyncState receivedState, long frame)
    {
        float distanceThreshold = UFE.config.networkOptions.floatDesynchronizationThreshold;

        string expectedStateString = expectedState.ToString();
        string receivedStateString = receivedState.ToString();

        if (expectedState.frame == receivedState.frame
            && Mathf.Abs(expectedState.syncInfo.data.x - receivedState.syncInfo.data.x) <= distanceThreshold
            && Mathf.Abs(expectedState.syncInfo.data.y - receivedState.syncInfo.data.y) <= distanceThreshold
            && Mathf.Abs(expectedState.syncInfo.data.z - receivedState.syncInfo.data.z) <= distanceThreshold)
        {
            if (UFE.config.networkOptions.logSyncMsg)
            {
                string logMsg = string.Format("Synchronization Check\nFrame: {0}\nExpected State: {1}\nReceived State: {2}",
                        frame,
                        expectedStateString,
                        receivedStateString);
                Debug.Log(logMsg);
            }
            return true;
        }
        else
        {
            //---------------------------------------------------------------------------------------------------------
            // If a desynchronization has happened, stop clients and initiate playback tools.
            // Show a log message and check if we should exit from the network game.
            //---------------------------------------------------------------------------------------------------------

            // Whoever catches the desync, send the data back to the other player.
            UFE.MultiplayerAPI.SendNetworkMessage(new SynchronizationMessage(UFE.GetLocalPlayer(), frame, expectedState));

            string errorMsg = string.Format("Synchronization Lost!\nFrame: {0}\nExpected State: {1}\nReceived State: {2}",
                    frame,
                    expectedStateString,
                    receivedStateString);
            Debug.LogError(errorMsg);


            this._localSynchronizationStates.Clear();
            this._remoteSynchronizationStates.Clear();

            if (UFE.config.networkOptions.synchronizationAction == NetworkSynchronizationAction.PlaybackTool && UFE.ReplayMode != null)
            {
                if (UFE.config.networkOptions.postRollbackRecording)
                {
                    List<FluxStates> localRecordedHistory = new List<FluxStates>();
                    for (int i = (int)(_history.LastStoredFrame - _history.Count); i <= _history.LastStoredFrame; i++)
                    {
                        FluxStates hState;
                        if (_history.TryGetState(i, out hState))
                            localRecordedHistory.Add(hState);
                    }

                    UFE.ReplayMode.OverrideTrack(localRecordedHistory, 2);
                    UFE.ReplayMode.SetStartingFrame(_history.LastStoredFrame - UFE.ReplayMode.GetBufferSize(2), 2);
                }

                if (UFE.config.networkOptions.generateVariableLog)
                    CreateLogFile(frame);

                UFE.ReplayMode.SetStartingFrame(UFE.currentFrame - UFE.ReplayMode.GetBufferSize(1), 1);
                UFE.ReplayMode.enableControls = true;
                UFE.ReplayMode.enablePlayerControl = true;
                UFE.ReplayMode.enableRecording = false;
                UFE.ReplayMode.StopRecording();
                UFE.ReplayMode.Play();
                UFE.ReplayMode.Pause();
            }
            else if (UFE.config.networkOptions.synchronizationAction == NetworkSynchronizationAction.Disconnect)
            {
                UFE.MultiplayerAPI.Disconnect();
                UFE.StartConnectionLostScreen();
            }

            return false;
        }
    }

    public void CreateLogFile(long frame)
    {
        FluxStates desyncFrame;
        if (_history.TryGetState(frame, out desyncFrame))
        {
            string filePath = Application.dataPath + "/" + UFE.config.networkOptions.textFilePath;
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);
            System.IO.StreamWriter file = System.IO.File.CreateText(filePath);

            RecordVar.SaveStateTrackers(desyncFrame, new Dictionary<System.Reflection.MemberInfo, object>(), false, file);
            file.Close();

            Debug.Log("File Created: " + filePath);
        }
    }
    #endregion
}
