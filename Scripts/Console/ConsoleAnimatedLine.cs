using Revistone.Apps;
using Revistone.Functions;
using Revistone.Management;

namespace Revistone.Console;

/// <summary>
/// The configuration for dynamically updating a ConsoleLine.
/// </summary>
public class ConsoleAnimatedLine
{
    public bool enabled;

    public Action<ConsoleLine, ConsoleAnimatedLine, int> update; //called on update
    public object metaInfo;

    public int initTick; //tick updated
    public int tickMod; //ticks per update

    // --- CONSTRUCTORS ---

    /// <summary> The configuration for dynamically updating a ConsoleLine, every tickMod ticks. </summary>
    public ConsoleAnimatedLine(Action<ConsoleLine, ConsoleAnimatedLine, int> update, object metaInfo, int tickMod = 5, bool enabled = false)
    {
        this.enabled = enabled;
        this.update = update;
        this.initTick = Manager.currentTick - 1;
        this.metaInfo = metaInfo;
        this.tickMod = tickMod;
    }

    /// <summary> The configuration for dynamically updating a ConsoleLine, every tickMod ticks. </summary>
    public ConsoleAnimatedLine(Action<ConsoleLine, ConsoleAnimatedLine, int> update, int tickMod = 5, bool enabled = false) : this(update, new object(), tickMod, enabled) { }
    /// <summary> The configuration for dynamically updating a ConsoleLine, every tickMod ticks. </summary>
    public ConsoleAnimatedLine(ConsoleAnimatedLine animatedLine) : this(animatedLine.update, animatedLine.metaInfo, animatedLine.tickMod, animatedLine.enabled) { }
    /// <summary> The configuration for dynamically updating a ConsoleLine, every tickMod ticks. </summary>
    public ConsoleAnimatedLine() : this(Nothing, "", 5, false) { }

    /// <summary> The configuration for dynamically updating a ConsoleLine. </summary>
    public static ConsoleAnimatedLine AppTheme => new ConsoleAnimatedLine(UpdateAppTheme, AppRegistry.activeApp.colourScheme.speed, true);
    /// <summary> The configuration for dynamically updating a ConsoleLine. </summary>
    public static ConsoleAnimatedLine None => new ConsoleAnimatedLine(Nothing, "", 5, false);

    /// <summary> Update configuration for dynamically updating a ConsoleLine, every tickMod ticks. </summary>
    public void Update(Action<ConsoleLine, ConsoleAnimatedLine, int> update, object metaInfo, int tickMod = 5, bool enabled = false)
    {
        lock (Management.Manager.renderLockObject)
        {
            this.enabled = enabled;
            this.update = update;
            this.initTick = Manager.currentTick - 1;
            this.metaInfo = metaInfo;
            this.tickMod = tickMod;
        }
    }

    /// <summary> Update configuration for dynamically updating a ConsoleLine, every tickMod ticks. </summary>
    public void Update(ConsoleAnimatedLine dynamicUpdate) { Update(dynamicUpdate.update, dynamicUpdate.metaInfo, dynamicUpdate.tickMod, dynamicUpdate.enabled); }
    /// <summary> Update configuration for dynamically updating a ConsoleLine, every tickMod ticks. </summary>
    public void Update() { Update(Nothing, "", 5, false); }

    // --- PREMADE UPDATE TYPES ---

    /// <summary> Does nothing... </summary>
    public static void Nothing(ConsoleLine lineInfo, ConsoleAnimatedLine animationInfo, int tickNum) { }

    /// <summary> Shift colour by given shift (within animationMetaInfo). </summary>
    public static void ShiftColour(ConsoleLine lineInfo, ConsoleAnimatedLine animationInfo, int tickNum)
    {
        int shift = 1;
        if (animationInfo.metaInfo as int? != null) shift = (int)animationInfo.metaInfo;
        lineInfo.Update(lineInfo.lineColour.Shift(shift));
    }

    /// <summary> Updates text colours, via switching cyan and dark cyan. </summary>
    public static void UpdateAppTheme(ConsoleLine lineInfo, ConsoleAnimatedLine animationInfo, int tickNum)
    {
        (ConsoleColor oldColour, ConsoleColor newColour)[] colourPairs;
        colourPairs = Enumerable.Range(0, AppRegistry.activeApp.colourScheme.secondaryColour.Length).Select(i => (AppRegistry.activeApp.colourScheme.secondaryColour[i], AppRegistry.activeApp.colourScheme.secondaryColour.Flip()[i])).ToArray();
        lineInfo.Update(ColourFunctions.Replace(lineInfo.lineColour, colourPairs));
    }
}
