namespace Aml.Simulator.ProfilesAdditions
{
    public interface IProfileGenerator
    {
        string ProfileName { get; }

        GeneratedProfile Generate(Random random, SimulatorReferenceData reference);
    }
}
