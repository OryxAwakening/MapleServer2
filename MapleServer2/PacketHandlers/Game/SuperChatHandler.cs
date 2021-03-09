﻿using System.Collections.Generic;
using System.Linq;
using MaplePacketLib2.Tools;
using MapleServer2.Constants;
using MapleServer2.Packets;
using MapleServer2.Servers.Game;
using MapleServer2.Types;
using Microsoft.Extensions.Logging;

namespace MapleServer2.PacketHandlers.Game
{
    public class SuperChatHandler : GamePacketHandler
    {
        public override RecvOp OpCode => RecvOp.SUPER_WORLDCHAT;

        public SuperChatHandler(ILogger<SuperChatHandler> logger) : base(logger) { }

        private enum SuperChatMode : byte
        {
            Select = 0x0,
            Deselect = 0x1,
        }

        public override void Handle(GameSession session, PacketReader packet)
        {
            SuperChatMode mode = (SuperChatMode) packet.ReadByte();

            switch (mode)
            {
                case SuperChatMode.Select:
                    HandleSelect(session, packet);
                    break;
                case SuperChatMode.Deselect:
                    HandleDeselect(session);
                    break;
                default:
                    IPacketHandler<GameSession>.LogUnknownMode(mode);
                    break;
            }
        }

        private static void HandleSelect(GameSession session, PacketReader packet)
        {
            int item = packet.ReadInt();

            List<Item> playerInventoryItems = new(session.Player.Inventory.Items.Values);

            Item superChatItem = playerInventoryItems.FirstOrDefault(x => x.Id == item);
            if (superChatItem == null)
            {
                return;
            }

            session.Player.SuperChat = superChatItem.FunctionId;
            session.Send(SuperChatPacket.Select(session.FieldPlayer, superChatItem.FunctionId));
        }

        private static void HandleDeselect(GameSession session)
        {
            session.Send(SuperChatPacket.Deselect(session.FieldPlayer));
        }
    }
}
