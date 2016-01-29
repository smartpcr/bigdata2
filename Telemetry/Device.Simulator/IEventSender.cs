namespace Devices.Simulator
{
    using System.Threading.Tasks;

    public interface IEventSender
    {
        Task<bool> SendAsync(object evt);
    }
}
