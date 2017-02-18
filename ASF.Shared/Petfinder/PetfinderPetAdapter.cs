using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace ASF.Shared.Petfinder
{
    public class PetfinderPetAdapter : IPet
    {
        private petfinderPetRecord dto;

        public PetfinderPetAdapter(petfinderPetRecord dto)
        {
            this.dto = dto;
        }

        public PetAge Age
            => dto.age == petAgeType.Adult ? PetAge.Adult
            : dto.age == petAgeType.Baby ? PetAge.Baby
            : dto.age == petAgeType.Senior ? PetAge.Senior
            : PetAge.Young;

        public IReadOnlyCollection<string> Breeds
            => dto.breeds.breed
            .Select(b =>
            {
                XmlEnumAttribute attribute = typeof(petfinderBreedType)
                    .GetMember(b.ToString()).Single()
                    .GetCustomAttributes(typeof(XmlEnumAttribute), false)
                    .Cast<XmlEnumAttribute>()
                    .SingleOrDefault();
                return attribute?.Name ?? b.ToString();
            })
            .ToArray();

        public string Description
            => dto.description;

        public string ID
            => dto.shelterPetId;

        public IReadOnlyCollection<string> ImageUrls
            => dto.media.photos
            .Where(p => p.size == petPhotoTypeSize.x)
            .Select(p => p.Value)
            .ToArray();

        public DateTime LastUpdateTimestamp
            => dto.lastUpdate;

        public string Name
            => dto.name;

        public PetSex Sex
            => dto.sex == petGenderType.F ? PetSex.Female : PetSex.Male;

        public PetSize Size
            => dto.size == petSizeType.L ? PetSize.Large
            : dto.size == petSizeType.M ? PetSize.Medium
            : dto.size == petSizeType.S ? PetSize.Small
            : PetSize.ExtraLarge;

        public PetStatus Status
            => dto.status == petStatusType.A ? PetStatus.Adoptable
            : dto.status == petStatusType.H ? PetStatus.Hold
            : dto.status == petStatusType.P ? PetStatus.Pending
            : PetStatus.AdoptedOrRemoved;

        public PetType Type
            => dto.animal == animalType.BarnYard ? PetType.BarnYard
            : dto.animal == animalType.Bird ? PetType.Bird
            : dto.animal == animalType.Cat ? PetType.Cat
            : dto.animal == animalType.Dog ? PetType.Dog
            : dto.animal == animalType.Horse ? PetType.Horse
            : dto.animal == animalType.Pig ? PetType.Pig
            : dto.animal == animalType.Rabbit ? PetType.Rabbit
            : dto.animal == animalType.Reptile ? PetType.Reptile
            : PetType.SmallAndFurry;
    }
}
