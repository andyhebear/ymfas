using System;
using System.Collections.Generic;
using System.Text;
using Mogre;

namespace Ymfas {
    class ChatManager {
        private GameModeEnum gameMode;
        private int playerId;
        public ChatManager(GameModeEnum gMode, int player) {
            gameMode = gMode;
            TextRenderer.AddTextBox("score", "Game Mode: " + GameModeFactory.GetName(gMode) + "\nScore: 0", 10, 700, 300, 50, ColourValue.Green, ColourValue.White);
            StatBoardEvent.FiringEvent += new GameEventFiringHandler(handleStatUpdate);
            playerId = player;
        }

        private void handleStatUpdate(GameEvent e) {
            try {
                StatBoardEvent sbe = (StatBoardEvent)e;
                if (sbe.Stat == StatBoardEnum.PositiveTime) {
                    int myScore;
                    if (sbe.ValueById.TryGetValue(playerId, out myScore)) {
                        TextRenderer.UpdateTextBox("score", "Game Mode: " + GameModeFactory.GetName(gameMode) + "\nScore: " + myScore);
                    }
                }
            }
            catch (Exception err) {
                Util.RecordException(err);
            }
        }

    }
}
