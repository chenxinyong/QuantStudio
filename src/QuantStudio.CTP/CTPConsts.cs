using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantStudio.CTP
{
    public class CTPConsts
    {
        /// <summary>
        /// 默认交易所上市的品种信息
        /// </summary>
        public static IReadOnlyDictionary<string, FuturesCategory> ExchangeCategories = new Dictionary<string, FuturesCategory>()
        { 
             // 金属
            { FuturesCategory.SHFE_cu.Symbol, FuturesCategory.SHFE_cu },
            { FuturesCategory.SHFE_al.Symbol, FuturesCategory.SHFE_al },
            { FuturesCategory.SHFE_zn.Symbol, FuturesCategory.SHFE_zn },
            { FuturesCategory.SHFE_pb.Symbol, FuturesCategory.SHFE_pb } ,
            { FuturesCategory.SHFE_ni.Symbol, FuturesCategory.SHFE_ni },
            { FuturesCategory.SHFE_sn.Symbol, FuturesCategory.SHFE_sn },
            { FuturesCategory.SHFE_ss.Symbol, FuturesCategory.SHFE_ss },
            { FuturesCategory.SHFE_bc.Symbol, FuturesCategory.SHFE_bc },

            // 贵金属
            { FuturesCategory.SHFE_au.Symbol, FuturesCategory.SHFE_au },
            { FuturesCategory.SHFE_ag.Symbol, FuturesCategory.SHFE_ag },
            // 黑色系  
            { FuturesCategory.SHFE_rb.Symbol, FuturesCategory.SHFE_rb },
            { FuturesCategory.SHFE_wr.Symbol, FuturesCategory.SHFE_wr },
            { FuturesCategory.SHFE_hc.Symbol, FuturesCategory.SHFE_hc },
            // 能源
            { FuturesCategory.SHFE_sc.Symbol, FuturesCategory.SHFE_sc },
            { FuturesCategory.SHFE_lu.Symbol, FuturesCategory.SHFE_lu } ,
            { FuturesCategory.SHFE_fu.Symbol, FuturesCategory.SHFE_fu } ,
            { FuturesCategory.SHFE_bu.Symbol, FuturesCategory.SHFE_bu },
            { FuturesCategory.SHFE_sp.Symbol, FuturesCategory.SHFE_sp },
            { FuturesCategory.SHFE_ru.Symbol, FuturesCategory.SHFE_ru },
            { FuturesCategory.SHFE_nr.Symbol, FuturesCategory.SHFE_nr },

            // 农产品
            { FuturesCategory.DCE_c.Symbol ,FuturesCategory.DCE_c },
            { FuturesCategory.DCE_cs.Symbol,FuturesCategory.DCE_cs },
            { FuturesCategory.DCE_a.Symbol ,FuturesCategory.DCE_a},
            { FuturesCategory.DCE_b.Symbol,FuturesCategory.DCE_b },
            { FuturesCategory.DCE_m.Symbol,FuturesCategory.DCE_m },
            { FuturesCategory.DCE_y.Symbol,FuturesCategory.DCE_y },
            { FuturesCategory.DCE_p.Symbol, FuturesCategory.DCE_p},
            { FuturesCategory.DCE_fb.Symbol,FuturesCategory.DCE_fb },
            { FuturesCategory.DCE_bb.Symbol, FuturesCategory.DCE_bb},
            { FuturesCategory.DCE_jb.Symbol,FuturesCategory.DCE_jb },
            { FuturesCategory.DCE_rr.Symbol,FuturesCategory.DCE_rr },
            { FuturesCategory.DCE_lh.Symbol, FuturesCategory.DCE_lh},

            // 工业品
            { FuturesCategory.DCE_l.Symbol, FuturesCategory.DCE_l},
            { FuturesCategory.DCE_v.Symbol, FuturesCategory.DCE_v},
            { FuturesCategory.DCE_pp.Symbol, FuturesCategory.DCE_pp},
            { FuturesCategory.DCE_j.Symbol, FuturesCategory.DCE_j},
            { FuturesCategory.DCE_jm.Symbol, FuturesCategory.DCE_jm},
            { FuturesCategory.DCE_i.Symbol ,FuturesCategory.DCE_i},
            { FuturesCategory.DCE_eg.Symbol, FuturesCategory.DCE_eg},
            { FuturesCategory.DCE_eb.Symbol, FuturesCategory.DCE_eb},
            { FuturesCategory.DCE_pg.Symbol, FuturesCategory.DCE_pg},

            // 农产品
            { FuturesCategory.CZCE_WH.Symbol,FuturesCategory.CZCE_WH },
            { FuturesCategory.CZCE_PM.Symbol, FuturesCategory.CZCE_PM },
            { FuturesCategory.CZCE_RI.Symbol, FuturesCategory.CZCE_RI },
            { FuturesCategory.CZCE_JR.Symbol, FuturesCategory.CZCE_JR },
            { FuturesCategory.CZCE_LR.Symbol ,FuturesCategory.CZCE_LR },
            { FuturesCategory.CZCE_CF.Symbol, FuturesCategory.CZCE_CF },
            { FuturesCategory.CZCE_SR.Symbol, FuturesCategory.CZCE_SR },
            { FuturesCategory.CZCE_OI.Symbol,  FuturesCategory.CZCE_OI},
            { FuturesCategory.CZCE_RS.Symbol, FuturesCategory.CZCE_RS },
            { FuturesCategory.CZCE_RM.Symbol,  FuturesCategory.CZCE_RM},
            { FuturesCategory.CZCE_CY.Symbol, FuturesCategory.CZCE_CY},
            { FuturesCategory.CZCE_AP.Symbol, FuturesCategory.CZCE_AP},
            { FuturesCategory.CZCE_CJ.Symbol, FuturesCategory.CZCE_CJ},
            { FuturesCategory.CZCE_PK.Symbol ,FuturesCategory.CZCE_PK},

            // 其他
            { FuturesCategory.CZCE_TA.Symbol, FuturesCategory.CZCE_TA},
            { FuturesCategory.CZCE_MA.Symbol, FuturesCategory.CZCE_MA},
            { FuturesCategory.CZCE_FG.Symbol, FuturesCategory.CZCE_FG},
            { FuturesCategory.CZCE_ZC.Symbol, FuturesCategory.CZCE_ZC},
            { FuturesCategory.CZCE_SF.Symbol, FuturesCategory.CZCE_SF},
            { FuturesCategory.CZCE_SM.Symbol, FuturesCategory.CZCE_SM},
            { FuturesCategory.CZCE_UA.Symbol,FuturesCategory.CZCE_UA },
            { FuturesCategory.CZCE_SA.Symbol, FuturesCategory.CZCE_SA},
            { FuturesCategory.CZCE_PF.Symbol, FuturesCategory.CZCE_PF}

        };

        /// <summary>
        /// 默认的数据文件夹
        /// </summary>
        public static class MarketDataFolder
        {
            public static string App_Data = "App_Data";
            public static string Ticks = "Ticks";
            public static string Minutes = "Minutes";
            public static string Dialy = "Dialy";
        }

        /// <summary>
        /// CTP默认的在线时间段
        /// </summary>
        public static class CTPOnlineTradingTimeFrames
        {
            public static Dictionary<DayOfWeek, IReadOnlyList<TradingTimeFrame>> Default = new Dictionary<DayOfWeek, IReadOnlyList<TradingTimeFrame>>() {
                // 周一
                { DayOfWeek.Monday,new List<TradingTimeFrame>(){ new TradingTimeFrame(new TimeOnly(0,0),new TimeOnly(3,0)),new TradingTimeFrame(new TimeOnly(8,30),new TimeOnly(15,30))} },
                // 周二
                { DayOfWeek.Tuesday,new List<TradingTimeFrame>(){ new TradingTimeFrame(new TimeOnly(0,0),new TimeOnly(3,0)),new TradingTimeFrame(new TimeOnly(8,30),new TimeOnly(15,30))} },
                // 周三
                { DayOfWeek.Wednesday,new List<TradingTimeFrame>(){ new TradingTimeFrame(new TimeOnly(0,0),new TimeOnly(3,0)),new TradingTimeFrame(new TimeOnly(8,30),new TimeOnly(15,30))} },
                // 周四
                { DayOfWeek.Thursday,new List<TradingTimeFrame>(){ new TradingTimeFrame(new TimeOnly(0,0),new TimeOnly(3,0)),new TradingTimeFrame(new TimeOnly(8,30),new TimeOnly(15,30)) ,new TradingTimeFrame(new TimeOnly(20,30),new TimeOnly(23,59,59,999)) } },
                // 周五
                { DayOfWeek.Friday,new List<TradingTimeFrame>(){ new TradingTimeFrame(new TimeOnly(0,0),new TimeOnly(3,0))
                    ,new TradingTimeFrame(new TimeOnly(8,30),new TimeOnly(15,30)) 
                    ,new TradingTimeFrame(new TimeOnly(21,0),new TimeOnly(23,59,59,999)) } },
                // 周六
                { DayOfWeek.Saturday,new List<TradingTimeFrame>(){ new TradingTimeFrame(new TimeOnly(0,0),new TimeOnly(3,0))} }
            };

            public static bool IsCTPOnlineTradingTime(DateTime dateTime)
            {
                if(Default.ContainsKey(dateTime.DayOfWeek))
                {
                    TimeOnly time = TimeOnly.FromDateTime(dateTime);
                    var list = Default[dateTime.DayOfWeek].ToList();
                    for(int i = 0; i < list.Count; i++)
                    {
                        if (time >= list[i].Begin && time < list[i].End)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// CTP默认的收盘时间段
        /// </summary>
        public static class CTPClosingTradingTimeFrames
        {
            public static Dictionary<DayOfWeek, IReadOnlyList<TradingTimeFrame>> Default = new Dictionary<DayOfWeek, IReadOnlyList<TradingTimeFrame>>() {
                // 周一
                { DayOfWeek.Monday,new List<TradingTimeFrame>(){ new TradingTimeFrame(new TimeOnly(15,15),new TimeOnly(15,45)) } },
                // 周二
                { DayOfWeek.Tuesday,new List<TradingTimeFrame>(){ new TradingTimeFrame(new TimeOnly(3,0),new TimeOnly(3,30)), new TradingTimeFrame(new TimeOnly(15,15),new TimeOnly(15,45))} },
                // 周三
                { DayOfWeek.Wednesday,new List<TradingTimeFrame>(){ new TradingTimeFrame(new TimeOnly(3,0),new TimeOnly(3,30)), new TradingTimeFrame(new TimeOnly(15,15),new TimeOnly(15,45))} },
                // 周四
                { DayOfWeek.Thursday,new List<TradingTimeFrame>(){ new TradingTimeFrame(new TimeOnly(3,0),new TimeOnly(3,30)), new TradingTimeFrame(new TimeOnly(15,15),new TimeOnly(15,45))} },
                // 周五
                { DayOfWeek.Friday ,new List<TradingTimeFrame>(){ new TradingTimeFrame(new TimeOnly(3,0),new TimeOnly(3,30)), new TradingTimeFrame(new TimeOnly(15,15),new TimeOnly(15,45))} },
                // 周六
                { DayOfWeek.Saturday,new List<TradingTimeFrame>(){ new TradingTimeFrame(new TimeOnly(3,0),new TimeOnly(3,30)) } },
            };

            public static bool IsCTPClosingTradingTradingTime(DateTime dateTime)
            {
                if (Default.ContainsKey(dateTime.DayOfWeek))
                {
                    TimeOnly time = TimeOnly.FromDateTime(dateTime);
                    var list = Default[dateTime.DayOfWeek].ToList();
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (time >= list[i].Begin && time < list[i].End)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }
    }
}
