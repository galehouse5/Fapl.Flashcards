namespace ASF.Shared
{
    public interface IFlashcard
    {
        string ID { get; }

        void SetDescription(IPetDescription description);
        void SetImage(string imageID);
    }
}
