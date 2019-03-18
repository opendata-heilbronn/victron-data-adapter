using VeDirectCommunication.HexMode.HexMessages;

namespace VeDirectCommunication.HexMode
{
    internal interface IVictronHexMessageSerializer
    {
        IHexResponseMessage ParseHexMessage(byte[] messageNibbles);
        string Serialize(HexCommand command, byte[] data);
        string SerializeGetRegister(VictronRegister register);
    }
}