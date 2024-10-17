using static Revistone.Interaction.UserInputProfile;

namespace Revistone.Apps.Tracker;

abstract class TrackerStat
{
    public string statName { get; private set; }
    public string value;

    public TrackerStat(string statName, string value = "Not Tracked")
    {
        this.statName = statName;
        this.value = value;
    }

    public abstract string IdentificationString();

    public override string ToString()
    {
        return $"{statName} - {value}";
    }
}

class TrackerInputStat : TrackerStat
{
    public InputType[] inputTypes { get; private set; }

    public TrackerInputStat(string statName, params InputType[] inputTypes) : base(statName)
    {
        this.inputTypes = inputTypes;
    }

    public TrackerInputStat(string statName, string value, params InputType[] inputTypes) : base(statName, value)
    {
        this.inputTypes = inputTypes;
    }

    public override string IdentificationString()
    {
        return $"{statName}{string.Join("", inputTypes)}";
    }
}

class TrackerDropdownStat : TrackerStat
{
    public (string option, bool success)[] options;

    public TrackerDropdownStat(string statName, params (string option, bool success)[] options) : base(statName)
    {
        this.options = options;
    }

    public TrackerDropdownStat(string statName, string value, params (string option, bool success)[] options) : base(statName, value)
    {
        this.options = options;
    }

    public override string IdentificationString()
    {
        return $"{statName}dropdown";
    }
}