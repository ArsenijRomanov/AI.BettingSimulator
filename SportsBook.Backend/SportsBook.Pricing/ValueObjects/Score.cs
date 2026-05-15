namespace SportsBook.Pricing.ValueObjects;

public readonly record struct Score
{
    public int Home { get; }
    public int Away { get; }

    public int Total => Home + Away;
    public int Difference => Home - Away;

    public Score(int home, int away)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(home);
        ArgumentOutOfRangeException.ThrowIfNegative(away);
        
        Home = home;
        Away = away;
    }

    public override string ToString()
        => $"{Home}:{Away}";
}
