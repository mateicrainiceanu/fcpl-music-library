namespace Capstone.Core.Model;

/// <summary>Anything that can describe itself in one line. Tracks and albums both can.</summary>
public interface IDescribable
{
    string Describe();
}
