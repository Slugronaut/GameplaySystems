namespace Toolbox.Game
{
    /// <summary>
    /// Can be posted after all loading/pre-processing has been performed for a scene
    /// and gameplay is ready to proceed.
    /// </summary>
    public class SceneReadyForPlay : IBufferedMessage, IDeferredMessage { }
}
