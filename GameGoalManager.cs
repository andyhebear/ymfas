using System;
using System.Collections;
using System.Collections.Generic;
using Ymfas;
using Mogre;
using MogreNewt;

namespace Ymfas {
    public interface GameMode {
        
        GameModeEnum Mode { get;}

        /// <summary>
        /// Processes the ship states and sends out messages through eventMgr
        /// </summary>
        void ProcessState();
    }
    public class GameModeFactory {
        private int TAG_RATE = 20;
        private EventManager eventMgr;
        private ServerShipManager shipMgr;

        public GameModeFactory(EventManager eventManager, ServerShipManager serverShipManager) {
            eventMgr = eventManager;
            shipMgr = serverShipManager;
        }
        public GameMode CreateMode(GameModeEnum gameMode){
            switch (gameMode) {
                case GameModeEnum.Tag:
                    return new Tag(eventMgr, shipMgr, TAG_RATE);
                default:
                    return new Tag(eventMgr, shipMgr, TAG_RATE);
            }
        }
    }
    public class Tag : GameMode {
        private EventManager eventMgr;
        private ServerShipManager shipMgr;
        private int msgRate;
        private int processCtr;
        private Dictionary<StatBoardEnum, Dictionary<int, int>> playerStatsById;

        public GameModeEnum Mode { get { return GameModeEnum.Tag; } }

        public Tag(EventManager eventManager, ServerShipManager shipManager, int sendRate) {
            eventMgr = eventManager;
            shipMgr = shipManager;
            msgRate = sendRate;
            processCtr = 0;

            //init ship stats
            playerStatsById = new Dictionary<StatBoardEnum, Dictionary<int, int>>();
            Dictionary<int, int> initPrimary = new Dictionary<int, int>();
            Dictionary<int, int> initPosTime = new Dictionary<int, int>();
            Dictionary<int, int> initNegTime = new Dictionary<int, int>();
            IEnumerator shipIds = shipManager.ShipTable.Keys.GetEnumerator();
            shipIds.Reset();
            while (shipIds.MoveNext()) {
                initPrimary.Add((int)shipIds.Current, 0);
                initPosTime.Add((int)shipIds.Current, 0);
                initNegTime.Add((int)shipIds.Current, 0);
            }

            playerStatsById.Add(StatBoardEnum.PrimaryScore, initPrimary);
            playerStatsById.Add(StatBoardEnum.PositiveTime, initPosTime);
            playerStatsById.Add(StatBoardEnum.NegativeTime, initNegTime);
        }
        public void ProcessState() {
            if (processCtr % msgRate == 0) {
                //Send scoreboard updates
                IEnumerator byStat = playerStatsById.GetEnumerator();
                byStat.Reset();
                while (byStat.MoveNext()) {
                    KeyValuePair<StatBoardEnum, Dictionary<int, int>> curKV = (KeyValuePair<StatBoardEnum, Dictionary<int, int>>)byStat.Current;
                    StatBoardEvent statBoard = new StatBoardEvent(curKV.Key, curKV.Value);
                    eventMgr.SendEvent(statBoard);
                }

                //calculate ship stats

                //TO-DO
            }

            processCtr++;
        }
    }
}