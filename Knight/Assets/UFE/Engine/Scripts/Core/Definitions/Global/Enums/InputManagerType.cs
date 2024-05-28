namespace UFE3D
{
    /// <summary>
    /// Input Manager sets which Addon and controller type the user will be using as input:
    /// <para/>Unity Input Manager: The standard Input system offered by Unity. Access it by going to Edit -> Project Settings -> Input.
    /// <para/>cInput: Uses the cInput Addon.
    /// <para/>Control Freak: Uses Control Freak Addon (Mobile/touch controls).
    /// <para/>Rewired: Uses the Rewired Addon.
    /// <para/>Native Touch Controls: Uses a native touch controls interface provided by UFE
    /// </summary>
    public enum InputManagerType
    {
        LegacyInputManager,
        cInput,
        ControlFreak,
        Rewired,
        NativeTouchControls
    }
}