using Kart.Controls;

namespace Kart.Fusion
{
    public class RoomPlayerLocal
    {
        public RoomPlayerLocal(string id, string name, KartController kartController)
        {
            playerId = id;
            username = name;
            kart = kartController;
        }
        
        public string playerId;
        public string username;
        public KartController kart;
    }
}