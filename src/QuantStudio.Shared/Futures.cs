using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantStudio
{
    /// <summary>
    /// 期货商品
    /// </summary>
    public struct FuturesCategory
    {
        public readonly string Symbol { get; }
        public readonly string Name { get;}

        public readonly string MarketCode { get; }

        public readonly TradingTimeFrameType TimeFrameType { get; }
        public FuturesCategory(string symbol,string name,string marketCode,TradingTimeFrameType timeFrameType = TradingTimeFrameType.FuturesDayNight)
        {
            Symbol = symbol;
            Name = name;
            TimeFrameType = timeFrameType;
            MarketCode = marketCode;
        }

        #region SHFH

        // 金属

        /// <summary>
        /// 沪铜
        /// </summary>
        public static FuturesCategory SHFE_cu = new FuturesCategory("cu","沪铜", Market.SHFE, TradingTimeFrameType.FuturesDayOvernight);
        /// <summary>
        /// 沪铝
        /// </summary>
        public static FuturesCategory SHFE_al = new FuturesCategory("al", "沪铝", Market.SHFE, TradingTimeFrameType.FuturesDayOvernight);

        /// <summary>
        /// 沪锌
        /// </summary>
        public static FuturesCategory SHFE_zn = new FuturesCategory("zn", "沪锌", Market.SHFE, TradingTimeFrameType.FuturesDayOvernight);
        /// <summary>
        /// 沪铅
        /// </summary>
        public static FuturesCategory SHFE_pb = new FuturesCategory("pb", "沪铅", Market.SHFE, TradingTimeFrameType.FuturesDayOvernight);
        /// <summary>
        /// 沪镍
        /// </summary>
        public static FuturesCategory SHFE_ni = new FuturesCategory("ni", "沪镍", Market.SHFE, TradingTimeFrameType.FuturesDayOvernight);
        /// <summary>
        /// 沪锡
        /// </summary>
        public static FuturesCategory SHFE_sn = new FuturesCategory("sn", "沪锡", Market.SHFE, TradingTimeFrameType.FuturesDayOvernight);
        /// <summary>
        /// 不锈钢
        /// </summary>
        public static FuturesCategory SHFE_ss = new FuturesCategory("ss", "不锈钢", Market.SHFE, TradingTimeFrameType.FuturesDayOvernight);


        /// <summary>
        /// 沪金
        /// </summary>
        public static FuturesCategory SHFE_au = new FuturesCategory("au", "沪金", Market.SHFE, TradingTimeFrameType.FuturesDayOvernightLong);
        /// <summary>
        /// 沪银
        /// </summary>
        public static FuturesCategory SHFE_ag = new FuturesCategory("ag", "沪银", Market.SHFE, TradingTimeFrameType.FuturesDayOvernightLong);


        /// <summary>
        /// 螺纹钢
        /// </summary>
        public static FuturesCategory SHFE_rb = new FuturesCategory("rb", "螺纹钢", Market.SHFE, TradingTimeFrameType.FuturesDayNight);
        /// <summary>
        /// 线材
        /// </summary>
        public static FuturesCategory SHFE_wr = new FuturesCategory("wr", "线材", Market.SHFE, TradingTimeFrameType.FuturesDayNight);
        /// <summary>
        /// 热卷板
        /// </summary>
        public static FuturesCategory SHFE_hc = new FuturesCategory("hc", "热卷板", Market.SHFE, TradingTimeFrameType.FuturesDayNight);

        /// <summary>
        /// 原油
        /// </summary>
        public static FuturesCategory SHFE_sc = new FuturesCategory("sc", "原油", Market.SHFE, TradingTimeFrameType.FuturesDayOvernightLong);

        /// <summary>
        /// 低硫燃料油
        /// </summary>
        public static FuturesCategory SHFE_lu = new FuturesCategory("lu", "低硫燃料油", Market.SHFE,  TradingTimeFrameType.FuturesDayNight);
        /// <summary>
        /// 燃料油
        /// </summary>
        public static FuturesCategory SHFE_fu = new FuturesCategory("fu", "燃料油", Market.SHFE, TradingTimeFrameType.FuturesDayNight);
        /// <summary>
        /// 石油沥青
        /// </summary>
        public static FuturesCategory SHFE_bu = new FuturesCategory("bu", "石油沥青", Market.SHFE, TradingTimeFrameType.FuturesDayNight);
        /// <summary>
        /// 天然橡胶
        /// </summary>
        public static FuturesCategory SHFE_ru = new FuturesCategory("ru", "天然橡胶", Market.SHFE, TradingTimeFrameType.FuturesDayNight);
        /// <summary>
        /// 20号胶
        /// </summary>
        public static FuturesCategory SHFE_nr = new FuturesCategory("nr", "20号胶", Market.SHFE, TradingTimeFrameType.FuturesDayNight);
        /// <summary>
        /// 纸浆
        /// </summary>
        public static FuturesCategory SHFE_sp = new FuturesCategory("sp", "纸浆", Market.SHFE, TradingTimeFrameType.FuturesDayNight);

        #endregion

        #region DCE

        // 农产品
        
        /// <summary>
        /// 玉米
        /// </summary>
        public static FuturesCategory DCE_c = new FuturesCategory("c", "玉米", Market.DCE, TradingTimeFrameType.FuturesDayNight);

        /// <summary>
        /// 玉米淀粉
        /// </summary>
        public static FuturesCategory DCE_cs = new FuturesCategory("cs", "玉米淀粉", Market.DCE, TradingTimeFrameType.FuturesDayNight);
        /// <summary>
        /// 黄大豆1号
        /// </summary>
        public static FuturesCategory DCE_a = new FuturesCategory("a", "黄大豆1号", Market.DCE, TradingTimeFrameType.FuturesDayNight);
        /// <summary>
        /// 黄大豆2号
        /// </summary>
        public static FuturesCategory DCE_b = new FuturesCategory("b", "黄大豆2号", Market.DCE, TradingTimeFrameType.FuturesDayNight);
        /// <summary>
        /// 豆粕
        /// </summary>
        public static FuturesCategory DCE_m = new FuturesCategory("m", "豆粕", Market.DCE, TradingTimeFrameType.FuturesDayNight);
        /// <summary>
        /// 豆油
        /// </summary>
        public static FuturesCategory DCE_y = new FuturesCategory("y", "豆油", Market.DCE, TradingTimeFrameType.FuturesDayNight);
        /// <summary>
        /// 棕榈油
        /// </summary>
        public static FuturesCategory DCE_p = new FuturesCategory("p", "棕榈油", Market.DCE, TradingTimeFrameType.FuturesDayNight);
        /// <summary>
        /// 纤维板
        /// </summary>
        public static FuturesCategory DCE_fb = new FuturesCategory("fb", "纤维板", Market.DCE, TradingTimeFrameType.FuturesDayNight);
        /// <summary>
        /// 胶合板
        /// </summary>
        public static FuturesCategory DCE_bb = new FuturesCategory("bb", "胶合板", Market.DCE, TradingTimeFrameType.FuturesDayNight);
        /// <summary>
        /// 鸡蛋
        /// </summary>
        public static FuturesCategory DCE_jb = new FuturesCategory("jb", "鸡蛋", Market.DCE, TradingTimeFrameType.FuturesDayOnly);
        /// <summary>
        /// 粳米
        /// </summary>
        public static FuturesCategory DCE_rr = new FuturesCategory("rr", "粳米", Market.DCE, TradingTimeFrameType.FuturesDayOnly);
        /// <summary>
        /// 生猪
        /// </summary>
        public static FuturesCategory DCE_lh = new FuturesCategory("lh", "生猪", Market.DCE, TradingTimeFrameType.FuturesDayOnly);

        // 工业品

        /// <summary>
        /// 聚乙烯
        /// </summary>
        public static FuturesCategory DCE_l = new FuturesCategory("l", "聚乙烯", Market.DCE, TradingTimeFrameType.FuturesDayNight);
        /// <summary>
        /// 聚氯乙烯
        /// </summary>
        public static FuturesCategory DCE_v = new FuturesCategory("v", "聚氯乙烯", Market.DCE, TradingTimeFrameType.FuturesDayNight);
        /// <summary>
        /// 聚丙烯
        /// </summary>
        public static FuturesCategory DCE_pp = new FuturesCategory("pp", "聚丙烯", Market.DCE, TradingTimeFrameType.FuturesDayNight);
        /// <summary>
        /// 焦炭
        /// </summary>
        public static FuturesCategory DCE_j = new FuturesCategory("j", "焦炭", Market.DCE, TradingTimeFrameType.FuturesDayNight);
        /// <summary>
        /// 焦煤
        /// </summary>
        public static FuturesCategory DCE_jm = new FuturesCategory("jm", "焦煤", Market.DCE, TradingTimeFrameType.FuturesDayNight);
        /// <summary>
        /// 铁矿石
        /// </summary>
        public static FuturesCategory DCE_i = new FuturesCategory("i", "铁矿石", Market.DCE, TradingTimeFrameType.FuturesDayNight);
        /// <summary>
        /// 乙二醇
        /// </summary>
        public static FuturesCategory DCE_eg = new FuturesCategory("eg", "乙二醇", Market.DCE, TradingTimeFrameType.FuturesDayNight);
        /// <summary>
        /// 苯乙烯
        /// </summary>
        public static FuturesCategory DCE_eb = new FuturesCategory("eb", "苯乙烯", Market.DCE, TradingTimeFrameType.FuturesDayNight);
        /// <summary>
        /// 液化石油气
        /// </summary>
        public static FuturesCategory DCE_pg = new FuturesCategory("pg", "液化石油气", Market.DCE, TradingTimeFrameType.FuturesDayNight);

        #endregion

        #region CZCE

        /// <summary>
        /// 强麦
        /// </summary>
        public static FuturesCategory CZCE_WH = new FuturesCategory("WH", "强麦", Market.CZCE, TradingTimeFrameType.FuturesDayOnly);
        /// <summary>
        /// 普麦
        /// </summary>
        public static FuturesCategory CZCE_PM = new FuturesCategory("PM", "普麦", Market.CZCE, TradingTimeFrameType.FuturesDayOnly);
        /// <summary>
        /// 早籼稻
        /// </summary>
        public static FuturesCategory CZCE_RI = new FuturesCategory("RI", "早籼稻", Market.CZCE, TradingTimeFrameType.FuturesDayOnly);
        /// <summary>
        /// 粳稻
        /// </summary>
        public static FuturesCategory CZCE_JR = new FuturesCategory("JR", "粳稻", Market.CZCE, TradingTimeFrameType.FuturesDayOnly);
        /// <summary>
        /// 晚籼稻
        /// </summary>
        public static FuturesCategory CZCE_LR = new FuturesCategory("LR", "晚籼稻", Market.CZCE, TradingTimeFrameType.FuturesDayOnly);
        /// <summary>
        /// 棉花
        /// </summary>
        public static FuturesCategory CZCE_CF = new FuturesCategory("CF", "棉花", Market.CZCE, TradingTimeFrameType.FuturesDayNight);
        /// <summary>
        /// 白糖
        /// </summary>
        public static FuturesCategory CZCE_SR = new FuturesCategory("SR", "白糖", Market.CZCE, TradingTimeFrameType.FuturesDayNight);
        /// <summary>
        /// 菜籽油
        /// </summary>
        public static FuturesCategory CZCE_OI = new FuturesCategory("OI", "菜籽油", Market.CZCE, TradingTimeFrameType.FuturesDayNight);
        /// <summary>
        /// 油菜籽
        /// </summary>
        public static FuturesCategory CZCE_RS = new FuturesCategory("RS", "油菜籽", Market.CZCE, TradingTimeFrameType.FuturesDayNight);
        /// <summary>
        /// 菜籽粕
        /// </summary>
        public static FuturesCategory CZCE_RM = new FuturesCategory("RM", "菜籽粕", Market.CZCE, TradingTimeFrameType.FuturesDayNight);
        /// <summary>
        /// 棉纱
        /// </summary>
        public static FuturesCategory CZCE_CY = new FuturesCategory("CY", "棉纱", Market.CZCE, TradingTimeFrameType.FuturesDayNight);
        /// <summary>
        /// 苹果
        /// </summary>
        public static FuturesCategory CZCE_AP = new FuturesCategory("AP", "苹果", Market.CZCE, TradingTimeFrameType.FuturesDayOnly);
        /// <summary>
        /// 红枣
        /// </summary>
        public static FuturesCategory CZCE_CJ = new FuturesCategory("CJ", "红枣", Market.CZCE, TradingTimeFrameType.FuturesDayOnly);
        /// <summary>
        /// 花生
        /// </summary>
        public static FuturesCategory CZCE_PK = new FuturesCategory("PK", "花生", Market.CZCE, TradingTimeFrameType.FuturesDayOnly);

        // 非农产品

        /// <summary>
        /// PTA
        /// </summary>
        public static FuturesCategory CZCE_TA = new FuturesCategory("TA", "PTA", Market.CZCE, TradingTimeFrameType.FuturesDayNight);
        /// <summary>
        /// 甲醇
        /// </summary>
        public static FuturesCategory CZCE_MA = new FuturesCategory("MA", "甲醇", Market.CZCE, TradingTimeFrameType.FuturesDayNight);
        /// <summary>
        /// 玻璃
        /// </summary>
        public static FuturesCategory CZCE_FG = new FuturesCategory("FG", "玻璃", Market.CZCE, TradingTimeFrameType.FuturesDayNight);
        /// <summary>
        /// 动力煤
        /// </summary>
        public static FuturesCategory CZCE_ZC = new FuturesCategory("ZC", "动力煤", Market.CZCE, TradingTimeFrameType.FuturesDayNight);
        /// <summary>
        /// 硅铁
        /// </summary>
        public static FuturesCategory CZCE_SF = new FuturesCategory("SF", "硅铁", Market.CZCE, TradingTimeFrameType.FuturesDayOnly);
        /// <summary>
        /// 锰硅
        /// </summary>
        public static FuturesCategory CZCE_SM = new FuturesCategory("SM", "锰硅", Market.CZCE, TradingTimeFrameType.FuturesDayOnly);
        /// <summary>
        /// 尿素
        /// </summary>
        public static FuturesCategory CZCE_UA = new FuturesCategory("UA", "尿素", Market.CZCE, TradingTimeFrameType.FuturesDayOnly);
        /// <summary>
        /// 纯碱
        /// </summary>
        public static FuturesCategory CZCE_SA = new FuturesCategory("SA", "纯碱", Market.CZCE, TradingTimeFrameType.FuturesDayNight);
        /// <summary>
        /// 短纤
        /// </summary>
        public static FuturesCategory CZCE_PF = new FuturesCategory("PF", "短纤", Market.CZCE, TradingTimeFrameType.FuturesDayNight);

        #endregion
    }
}
