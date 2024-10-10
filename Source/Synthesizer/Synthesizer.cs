
namespace Synthesizer;

public class Synthesizer
{
    private readonly HashSet<Channel> channels = [];

    public void AddChannel(Channel channel)
        => channels.Add(channel);

    public Channel GetChannel(string channelName)
        => channels.First(channel => channel.Name == channelName);

    public Channel GetChannel(int channelNum)
        => channels.First(channel => channel.Name == channelNum.ToString());

    public void DeleteChannel(Channel channel)
        => channels.Remove(channel);

    public void DeleteChannel(string channelName)
        => channels.RemoveWhere(channel => channel.Name == channelName);

    public void DeleteChannel(int channelNum)
        => channels.RemoveWhere(channel => channel.Name == channelNum.ToString());

    public void StartPlayback()
    {
        throw new NotImplementedException();
    }

    public void StopPlayback()
    {
        throw new NotImplementedException();
    }

    public byte[][] Update(float delta)
    {
        throw new NotImplementedException();
    }
}