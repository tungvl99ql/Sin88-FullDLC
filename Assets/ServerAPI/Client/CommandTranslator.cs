using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Core.Server.Api
{


    public class CommandTranslator
    {
        public static CommandTranslator instance;
        private Dictionary<string, string> commandCodeById = new Dictionary<string, string>();
        public CommandTranslator()
        {
            addCommandCodeById();
        }

        public static CommandTranslator getInstance()
        {
            if (instance == null)
            {
                instance = new CommandTranslator();
            }

            return instance;
        }

        //add code - id vao dictionary
        void addCommandCodeById()
        {
            commandCodeById.Add("300", "PONG");
            commandCodeById.Add("301", "PING");
            commandCodeById.Add("302", "LOGIN");
            commandCodeById.Add("303", "ALERT");
            commandCodeById.Add("304", "RIBBON_MESSAGE");
            commandCodeById.Add("305", "FEEDBACK");
            commandCodeById.Add("306", "RELOAD");
            commandCodeById.Add("307", "RELOAD_APP");
            commandCodeById.Add("308", "NAVIGATE");
            commandCodeById.Add("309", "ACHM_ACHIEVED");
            commandCodeById.Add("310", "PM.UNREAD");
            commandCodeById.Add("311", "BROADCAST");
            commandCodeById.Add("312", "INVITE");
            commandCodeById.Add("313", "GET_CLIENT_MODE");
            commandCodeById.Add("314", "SET_CLIENT_MODE");
            commandCodeById.Add("315", "CONFIG");
            commandCodeById.Add("317", "TRANSFER");

            commandCodeById.Add("324", "CHANGE_PASSWORD");
            commandCodeById.Add("325", "UPDATE_PROFILE");
            commandCodeById.Add("329", "REPORT_ABUSE");

            commandCodeById.Add("331", "CHAT.SEND");
            commandCodeById.Add("332", "CHAT.SUBS");
            commandCodeById.Add("333", "CHAT.LOAD");
            commandCodeById.Add("334", "GET_REMAIN_DURATION");
            commandCodeById.Add("335", "CHAT.MSG");
            commandCodeById.Add("337", "HONOUR");
            commandCodeById.Add("352", "GET_SC_RECHARGE_DATA");
            commandCodeById.Add("354", "GET_SMS_RECHARGE_DATA");
            commandCodeById.Add("355", "RECHARGE_BY_GIFT_CODE");
            commandCodeById.Add("356", "GET_GOOGLE_RECHARGE_DATA");
            commandCodeById.Add("358", "GET_IOS_RECHARGE_DATA");
            //dau 4
            commandCodeById.Add("401", "ENTER_PLACE");
            commandCodeById.Add("402", "ENTER_CHILD_PLACE");
            commandCodeById.Add("403", "ENTER_PARENT_PLACE");
            commandCodeById.Add("404", "ENTER_SIBLING_PLACE");
            commandCodeById.Add("405", "CREATE_RULE");
            commandCodeById.Add("408", "QUICK_PLAY");

            commandCodeById.Add("410", "KICK_PLAYER");
            commandCodeById.Add("411", "LIST_ZONE_TABLE");
            commandCodeById.Add("412", "LIST_ZONE_ROOM");
            commandCodeById.Add("413", "LIST_BET_AMT");
            commandCodeById.Add("414", "GET_TABLE_DATA");
            commandCodeById.Add("415", "TABLE_IN_ROOM_CHANGED");
            commandCodeById.Add("416", "SLOT_IN_TABLE_CHANGED");
            commandCodeById.Add("417", "START_MATCH");
            commandCodeById.Add("418", "GAMEOVER");
            commandCodeById.Add("419", "ENTER_STATE");
            commandCodeById.Add("420", "SET_TURN");
            commandCodeById.Add("421", "SET_PLAYER_STATUS");
            commandCodeById.Add("422", "SET_PLAYER_POINT");
            commandCodeById.Add("423", "SET_PLAYER_ATTR");
            commandCodeById.Add("425", "SPIN_LUCKY_WHEEL");
            commandCodeById.Add("426", "GET_REMAIN_SPIN");
            commandCodeById.Add("429", "BUY_ITEM");
            commandCodeById.Add("430", "GET_CURRENT_PATH");
            commandCodeById.Add("431", "BALANCE_CHANGED");
            commandCodeById.Add("432", "OWNER_CHANGED");
            commandCodeById.Add("433", "GET_TABLE_DATA_EX");

            //dau 5
            commandCodeById.Add("501", "BET");
            commandCodeById.Add("502", "PLAY");
            commandCodeById.Add("518", "HIGHLIGHT");
            commandCodeById.Add("521", "TAKE_CARD");
            commandCodeById.Add("522", "SHOW_PLAYER_CARD");
            commandCodeById.Add("523", "CLEAR_CARDS");
            commandCodeById.Add("524", "SET_CARDS");
            commandCodeById.Add("525", "SELECT_CARDS");
            commandCodeById.Add("528", "SEND_CARD");
            commandCodeById.Add("529", "MOVE");
            commandCodeById.Add("530", "CHANGE_PIECE");
            commandCodeById.Add("531", "SET_REMAIN_TURN");
            commandCodeById.Add("533", "ASK_DRAW");
            commandCodeById.Add("534", "SURRENDER");
            commandCodeById.Add("537", "HIT");
            commandCodeById.Add("538", "STAY");
            commandCodeById.Add("539", "FIRE_CARD");
            commandCodeById.Add("540", "PASS_TURN");
            commandCodeById.Add("541", "SORT_CARD");
            commandCodeById.Add("542", "TAKE");
            commandCodeById.Add("543", "EAT");
            commandCodeById.Add("544", "REMOVE");
            commandCodeById.Add("545", "DROP_BAND");
            commandCodeById.Add("547", "DROP_AVAILABLE_BAND");
            commandCodeById.Add("549", "SUBMIT");
            commandCodeById.Add("553", "IGNORE");
            commandCodeById.Add("554", "GRAB");
            commandCodeById.Add("555", "DROP");
            commandCodeById.Add("556", "RAISE");
            commandCodeById.Add("557", "CHECK");
            commandCodeById.Add("558", "DEAL");
            commandCodeById.Add("559", "CALL");
            commandCodeById.Add("560", "FOLD");
            commandCodeById.Add("561", "OPEN");
            commandCodeById.Add("562", "DIVIDE_CHIP");
            commandCodeById.Add("563", "SPREAD");
            commandCodeById.Add("571", "UNBET");
            commandCodeById.Add("572", "SIDE_BET");
            commandCodeById.Add("573", "SELL_GATE");
            commandCodeById.Add("574", "BUY_GATE");

            //dau 6
            commandCodeById.Add("601", "LOGIN_EX");
            commandCodeById.Add("602", "TOP_RICH_PLAYER");
            commandCodeById.Add("604", "PLAYER_PROFILE");
            commandCodeById.Add("605", "LIST_ZONE_PLAYER");
            commandCodeById.Add("606", "NOTIFY_ALL");
            commandCodeById.Add("607", "LOGIN_NEW");
            commandCodeById.Add("608", "PHONE.ACTIVE");

            //dau 7
            commandCodeById.Add("701", "PM.LIST");
            commandCodeById.Add("702", "PM.CREATE");
            commandCodeById.Add("703", "PM.DELETE");
            commandCodeById.Add("711", "PLAYER.LIST");
            commandCodeById.Add("721", "FRIEND.LIST");
            commandCodeById.Add("722", "FRIEND.CREATE");
            commandCodeById.Add("723", "FRIEND.DELETE");
            commandCodeById.Add("724", "LIST_MATCH");
            commandCodeById.Add("725", "GET_MATCH_DATA");
            commandCodeById.Add("744", "PM.SUBS");
            //dau 8
            commandCodeById.Add("801", "GET_CASH_SHOP_DATA");
            commandCodeById.Add("802", "BUY_CASH_SHOP_ITEM");
            commandCodeById.Add("812", "REGISTER_ACCOUNT_EX");
            commandCodeById.Add("821", "CASH_OUT.GET_DATA");
            commandCodeById.Add("822", "CASH_OUT.EXCHANGE_ITEM");
            commandCodeById.Add("823", "CASH_OUT.GET_POLICY");
            commandCodeById.Add("824", "CASH_OUT.GET_PLAYER_VIP_POINT");
            commandCodeById.Add("825", "CASH_OUT.GET_HISTORY");
            commandCodeById.Add("826", "CASH_OUT.GET_HISTORY_ALL");
            commandCodeById.Add("827", "CASH_OUT.CANCEL");



            commandCodeById.Add("831", "ROULETTE.GET_DATA");
            commandCodeById.Add("841", "PAYMENT.LIST");
            commandCodeById.Add("842", "PAYMENT.SMS_PLUS");
            commandCodeById.Add("843", "PAYMENT.SMS_PLUS_EX");
            commandCodeById.Add("844", "PAYMENT.LIST_EX");

            commandCodeById.Add("851", "RECHARGE_BY_SC_EX");
            commandCodeById.Add("861", "UTIL.STORE_REVIEW_BYPASS");
            commandCodeById.Add("862", "UTIL.GET_HEADLINES");
            commandCodeById.Add("863", "UTIL.TOP_XOC_DIA_PLAYER");
            commandCodeById.Add("865", "UTIL.TOP_GAME_PLAYER");
            commandCodeById.Add("866", "UTIL.STORE_REVIEW_BYPASS_EX");
            commandCodeById.Add("871", "XENG.GET_INFO");
            commandCodeById.Add("872", "XENG.GET_GUIDE");
            commandCodeById.Add("873", "XENG.GET_HISTORY");
            commandCodeById.Add("874", "XENG.START");
            commandCodeById.Add("875", "XENG.END");

            commandCodeById.Add("881", "VUACO.TOP_GAME_RECORD");

            commandCodeById.Add("891", "SOCIAL.VALIDATE_FB_SHARING");
            commandCodeById.Add("892", "SOCIAL.COMPLETE_FB_SHARING");


            //Dau 9
            commandCodeById.Add("901", "TAIXIU.GET_INFO");
            commandCodeById.Add("902", "TAIXIU.ENTER");
            commandCodeById.Add("903", "TAIXIU.EXIT");
            commandCodeById.Add("904", "TAIXIU.PREPARE");
            commandCodeById.Add("905", "TAIXIU.START");
            commandCodeById.Add("906", "TAIXIU.SELL_GATE");
            commandCodeById.Add("907", "TAIXIU.GAMEOVER");
            commandCodeById.Add("908", "TAIXIU.BET");
            commandCodeById.Add("909", "TAIXIU.UPDATE_POT");
            commandCodeById.Add("910", "TAIXIU.REFUND");
            commandCodeById.Add("911", "TAIXIU.DIVIDE_CHIP");
            commandCodeById.Add("912", "TAIXIU.SHOW_RESULT");
            commandCodeById.Add("913", "TAIXIU.TIME_CHANGED");
            commandCodeById.Add("914", "TAIXIU.REPORT");
            commandCodeById.Add("915", "TAIXIU.MATCH_INFO");

            commandCodeById.Add("921", "EGGY.GET_INFO");
            commandCodeById.Add("922", "EGGY.POT_CHANGED");
            commandCodeById.Add("923", "EGGY.START");
            commandCodeById.Add("924", "EGGY.GET_HISTORY");

            commandCodeById.Add("931", "SLOT_MACHINE.GET_INFO");
            commandCodeById.Add("932", "SLOT_MACHINE.START");
            commandCodeById.Add("933", "SLOT_MACHINE.GET_HISTORY");
            commandCodeById.Add("934", "SLOT_MACHINE.POT_CHANGED");
            commandCodeById.Add("935", "SLOT_MACHINE.GET_INFO_DETAIL");
            commandCodeById.Add("936", "SLOT_MACHINE.GLORY_BOARD");
            commandCodeById.Add("937", "SLOT_MACHINE.GET_POT_ALL");
            commandCodeById.Add("940", "SLOT_GOLD_RUSH.GET_INFO");
            commandCodeById.Add("941", "SLOT_GOLD_RUSH.START");
            commandCodeById.Add("942", "SLOT_GOLD_RUSH.GET_HISTORY");

            commandCodeById.Add("943", "SLOT_GOLD_RUSH.GET_INFO_DETAIL");

            commandCodeById.Add("950", "GOLD_RUSH.START");
            commandCodeById.Add("951", "RECHARGE_BY_IAP_IOS");
            commandCodeById.Add("952", "RECHARGE_BY_IAP_ANDROID");
            commandCodeById.Add("953", "RECHARGE_BY_IAP_IOS_LIST");
            commandCodeById.Add("954", "RECHARGE_BY_IAP_ANDROID_LIST");

            commandCodeById.Add("960", "MATCH.HISTORY");
            commandCodeById.Add("961", "EVENT.LIST");
            commandCodeById.Add("962", "EVENT.DETAIL");
            commandCodeById.Add("970", "MINIPOKER.GET_INFO");
            commandCodeById.Add("971", "MINIPOKER.START");

            commandCodeById.Add("981", "CAOTHAP.START");
            commandCodeById.Add("982", "CAOTHAP.END_GAME");
            commandCodeById.Add("980", "CAOTHAP.GET_INFO");
            commandCodeById.Add("990", "MINIGAME.GET_POT_ALL");
            commandCodeById.Add("991", "MINIGAME.POT_CHANGED");
            commandCodeById.Add("993", "ACTIVE_BUTTON.SHOW");
            commandCodeById.Add("994", "AGENCY_UPDATE");
            commandCodeById.Add("995", "AGENCY_PASSWORD_LV2");

            commandCodeById.Add("996", "AGENCY.GET_LIST");

            commandCodeById.Add("997", "CLIENT.INFO");

            commandCodeById.Add("998", "ONESIGNAL.GET_INFO");
            commandCodeById.Add("999", "FACEBOOK.GET_INFO");

            commandCodeById.Add("1001", "XENGFULL.START");
            commandCodeById.Add("1002", "XENGFULL.TAIXIU_START");
            commandCodeById.Add("1010", "MINIGAME.LUCKY_STATUS");
            commandCodeById.Add("1011", "MINIGAME.LUCKY_START");

            commandCodeById.Add("1020", "LOGIN_NOW");
            commandCodeById.Add("1021", "LOGIN_NOW.CAPTCHA");
            commandCodeById.Add("1022", "LOGIN_CAPTCHA.SUCCESS");

            commandCodeById.Add("1030", "KEOHOLO.GET_INFO");
            commandCodeById.Add("1031", "KEOHOLO.START");
            commandCodeById.Add("1032", "KEOHOLO.HISTORY");
            commandCodeById.Add("1033", "KEOHOLO.GLORY_BOARD");
            // code for 3x3
            commandCodeById.Add("1050", "PIGS.GET_INFO");
            commandCodeById.Add("1051", "PIGS.START");
            commandCodeById.Add("1052", "PIGS.GET_HISTORY");
            commandCodeById.Add("1053", "PIGS.GLORY_BOARD");

            commandCodeById.Add("1054", "PIGS.COMPLETED");


            commandCodeById.Add("1060", "BAUCUA.GET_INFO");
            commandCodeById.Add("1061", "BAUCUA.ENTER");
            commandCodeById.Add("1062", "BAUCUA.EXIT");
            commandCodeById.Add("1063", "BAUCUA.BET");
            commandCodeById.Add("1064", "BAUCUA.REPORT");
            commandCodeById.Add("1065", "BAUCUA.PREPARE");
            commandCodeById.Add("1066", "BAUCUA.START");
            commandCodeById.Add("1067", "BAUCUA.COMPLETED");
            commandCodeById.Add("1068", "BAUCUA.SHOW_RESULT");
            commandCodeById.Add("1069", "BAUCUA.DIVIDE_CHIP");
            commandCodeById.Add("1070", "BAUCUA.GAMEOVER");
            commandCodeById.Add("1071", "BAUCUA.UPDATE_POT");

            commandCodeById.Add("1080", "GOBLINS.GET_INFO");
            commandCodeById.Add("1081", "GOBLINS.START");
            commandCodeById.Add("1082", "GOBLINS.GET_HISTORY");
            commandCodeById.Add("1083", "GOBLINS.GET_INFO_DETAIL");
            commandCodeById.Add("1084", "GOBLINS.GLORY_BOARD");
            commandCodeById.Add("1085", "GOBLINS.RUSH");

            commandCodeById.Add("1101", "XBOX.GET_INFO");
            commandCodeById.Add("1102", "XBOX.START");

            commandCodeById.Add("1105", "DROPBALL.GET_INFO");
            commandCodeById.Add("1106", "DROPBALL.START");
            //Slot One line
            commandCodeById.Add("1090", "GUMMY.GET_INFO");
            commandCodeById.Add("1091", "GUMMY.START");
            commandCodeById.Add("1092", "GUMMY.GET_HISTORY");
            commandCodeById.Add("1093", "GUMMY.GLORY_BOARD");

            commandCodeById.Add("1111", "EGYPT.GET_INFO");
            commandCodeById.Add("1112", "EGYPT.START");
            commandCodeById.Add("1113", "EGYPT.GET_HISTORY");
            commandCodeById.Add("1114", "EGYPT.GET_INFO_DETAIL");
            commandCodeById.Add("1115", "EGYPT.GLORY_BOARD");

            commandCodeById.Add("1104", "REDBLACK.START");

            commandCodeById.Add("1116", "69C.GET_INFO");
            commandCodeById.Add("1117", "69C.START");
            commandCodeById.Add("1118", "69C.GET_HISTORY");
            commandCodeById.Add("1119", "69C.GET_INFO_DETAIL");
            commandCodeById.Add("1120", "69C.GLORY_BOARD");
            commandCodeById.Add("1121", "XPOT.INFO");
            commandCodeById.Add("1122", "IP_LOOKUP.URL");
            commandCodeById.Add("1123", "LOST_PWD.URL");
            commandCodeById.Add("1124", "APP.NEW_VERSION");
            commandCodeById.Add("1125", "APP.MAINTAIN");
            commandCodeById.Add("1126", "ASSET_DATA.LOAD");


            // exit when not enough money
            commandCodeById.Add("893", "GAME.EXIT");
            commandCodeById.Add("992", "BONUS_BUTTON.SHOW");

            commandCodeById.Add("1107", "DROPBALL.SHOWUP");
            commandCodeById.Add("1108", "REDBLACK.SHOWUP");
            commandCodeById.Add("1109", "XBOX.SHOWUP");
            commandCodeById.Add("360", "GET_SC_RECHARGE_DATA_EX");
        }

        private Dictionary<string, string> commandIdByCode = null;

        //lấy commandcode theo id 
        public string getCommandCode(string id)
        {
            return commandCodeById[id];
        }

        //lấy commnad id theo code
        public string getCommandId(string code)
        {
            if (commandIdByCode == null)
            {
                commandIdByCode = new Dictionary<string, string>();
                foreach (string key in commandCodeById.Keys)
                {
                    commandIdByCode.Add(commandCodeById[key], key);
                }
            }
            return commandIdByCode[code];
        }


    }

}