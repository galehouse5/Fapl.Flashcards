using System.Collections.Generic;

namespace ASF.Shared
{
    public enum PetType { Dog, Cat, SmallAndFurry, BarnYard, Bird, Horse, Pig, Rabbit, Reptile }
    public enum PetSex { Male, Female }
    public enum PetAge { Baby, Young, Adult, Senior }
    public enum PetSize { Small, Medium, Large, ExtraLarge }
    public enum PetStatus { Adoptable, Hold, Pending, AdoptedOrRemoved }

    public interface IPetDescription
    {
        PetType Type { get; }
        PetStatus Status { get; }
        string Name { get; }
        IReadOnlyCollection<string> Breeds { get; }
        PetSex Sex { get; }
        PetAge Age { get; }
        PetSize Size { get; }
        string Description { get; }
    }
}
