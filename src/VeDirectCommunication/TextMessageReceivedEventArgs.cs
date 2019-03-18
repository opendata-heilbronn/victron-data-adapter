using VeDirectCommunication.TextMode;

namespace VeDirectCommunication
{
    public class TextMessageReceivedEventArgs
    {
        public TextMessageReceivedEventArgs(VictronTextBlock data)
        {
            Data = data;
        }
        public VictronTextBlock Data { get; }
    }
}