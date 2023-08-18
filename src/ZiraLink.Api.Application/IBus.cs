namespace ZiraLink.Api.Application
{
    public interface IBus
    {
        void Publish(string message);
    }
}
