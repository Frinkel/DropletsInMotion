using DropletsInMotion.Domain;

public class SplitManager
{
    public List<ICommand> SplitsInAction { get; private set; } = new List<ICommand>();

    public SplitManager() { }

    public void StoreSplit(ICommand splitCommand)
    {
        SplitsInAction.Add(splitCommand);
    }

    public bool CanSplit(ICommand splitCommand)
    {
        return !SplitsInAction.Any(existingSplit => SplitEquals(existingSplit, splitCommand));
    }

    public void RemoveSplit(ICommand splitCommand)
    {
        SplitsInAction.RemoveAll(existingSplit => SplitEquals(existingSplit, splitCommand));
    }

    private bool SplitEquals(ICommand split1, ICommand split2)
    {
        if (split1 is SplitByRatio ratio1 && split2 is SplitByRatio ratio2)
        {
            return ratio1.InputName == ratio2.InputName &&
                   ratio1.OutputName1 == ratio2.OutputName1 &&
                   ratio1.OutputName2 == ratio2.OutputName2;
        }

        if (split1 is SplitByVolume volume1 && split2 is SplitByVolume volume2)
        {
            return volume1.InputName == volume2.InputName &&
                   volume1.OutputName1 == volume2.OutputName1 &&
                   volume1.OutputName2 == volume2.OutputName2;
        }

        return false;
    }
}