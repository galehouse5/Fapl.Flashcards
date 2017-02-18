namespace ASF.Shared.Petfinder
{
    public class PetfinderPetPhoto
    {
        public enum PhotoWidth { _50, _60, _95, _300, _500 }

        public int ID { get; set; }
        public PhotoWidth Width { get; set; }
        public string Url { get; set; }

        public static PetfinderPetPhoto Create(dynamic dto)
            => new PetfinderPetPhoto
            {

            };
    }
}
