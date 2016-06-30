public enum EventID
{
    Null,

    #region 系统
    ConfigDataReady,
    PreSceneChange,
    CameraAnimation,

    /// <summary>
    /// 和服务器对时成功
    /// </summary>
    ServerTimeReady,

    /// <summary>
    /// 整点报时
    /// </summary>
    HourlyChime,

    /// <summary>
    /// SocketMessage的请求和回调
    /// 参数true: req; false: rsp
    /// </summary>
    SocketMessage,

    /// <summary>
    /// 所有系统的OnEnterWorld都调用了
    /// </summary>
    EnterWorldFinish,
    #endregion

    #region 村庄
    /// <summary>
    /// 村庄升级
    /// </summary>
    VillageUpgrade,

    /// <summary>
    /// 更新村庄繁华度（没有参数）
    /// </summary>
    VillageExp,

    /// <summary>
    /// 更新玩家体力（没有参数）
    /// </summary>
    UpdateVit,

    /// <summary>
    /// 建筑物开始建造.参数：buildingInfo
    /// </summary>
    BuildingBeginBuild,

    /// <summary>
    /// 建筑物取消建造.参数：buildingInfo
    /// </summary>
    buildingCancelBuild,

    /// <summary>
    /// 建筑物建造完成.参数：buildingInfo
    /// </summary>
    BuildingFinishBuild,

    /// <summary>
    /// 建筑物开始升级.参数：buildingInfo
    /// </summary>
    BuildingBeginUpgrade,

    /// <summary>
    /// 建筑物取消升级.参数：buildingInfo
    /// </summary>
    buildingCancelUpgrade,

    /// <summary>
    /// 建筑物升级完成.参数：buildingInfo
    /// </summary>
    BuildingFinishUpgrade,

    /// <summary>
    /// 建筑物状态改变.参数：buildingInfo
    /// </summary>
    BuildingStateChange,
    #endregion

    #region 战斗
    /// <summary>
    /// 战斗里面的怪物死亡//id
    /// </summary>
    BattleEnemyDie,
    /// <summary>
    /// 英雄&怪物死亡
    /// </summary>
    HeroDie,//hero

    EnemyGroupDie,//int groupId

    /// <summary>
    /// 部件改变 MapPartsBasic 部件
    /// </summary>
    PartsStateAlter,

    /// <summary>
    /// 一个战斗区域战斗结束的时候：int areaId:区域id  bool isWin :true is win
    /// </summary>
    BattleAreaOver,

    /// 一个战斗区域战斗开始的时候：int areaId:区域id
    /// </summary>
    BattleAreaStart,

    /// <summary>
    /// 战斗里面的状态改变,BattleStateType
    /// </summary>
    BattleStateAlter,

    /// <summary>
    /// 英雄攻击前（Hero）
    /// </summary>
    HeroAttackiPer,

    /// <summary>
    /// 英雄受伤时（hero,int injuredValue）
    /// </summary>
    HeroInjured,
    /// <summary>
    /// 战斗暂停状态改变
    /// </summary>
    BattlePauseStateAlter,

    #endregion

    #region 副本
    /// <summary>
    /// 章节解锁ID
    /// </summary>
    DungeonUnlock,

    /// <summary>
    /// 副本完成度增加 dungeonInfo
    /// </summary>
    CompletenessIncrease,

    /// <summary>
    /// 副本事件完成 int eventid
    /// </summary>
    MapEventFinish,

    /// <summary>
    /// 副本通关
    /// </summary>
    DungeonFinish,
    #endregion

    #region 英雄

    /// <summary>
    /// 立刻刷个英雄出来（没有参数）
    /// </summary>
    ImmediatelyRefreshHero,

    /// <summary>
    /// 英雄定居状态改变(参数 heroInfo)
    /// </summary>
    HeroSettleState,
    /// <summary>
    /// 英雄信息改变(参数 heroInfo)
    /// </summary>
    HeroInfoChange,

    /// <summary>
    /// 村庄英雄进入村庄（参数英雄 ID）
    /// </summary>
    HeroBehaviorVisit,
    /// <summary>
    /// 村庄英雄离开村庄（参数英雄 ID）
    /// </summary>
    HeroBehaviorLeave,

    /// <summary>
    /// 英雄状态改变；heroInfo
    /// </summary>
    HeroActivityState,

    /// <summary>
    /// 检测英雄技能解锁 （没有参数）
    /// </summary>
    HeroSkillDeblocking,
    #endregion

    #region 技能

    /// <summary>
    /// 技能合成成功（参数一个：SkillInfo）
    /// </summary>
    SkillCombineUpdate,

    /// <summary>
    /// 技能更换刷新（参数一个：SkillBasic 新的技能）
    /// </summary>
    SkillChangeUpdate,

    /// <summary>
    /// 刷新技能点显示（没有参数）
    /// </summary>
    UpdateSKillPoint,
    #endregion

    #region 背包
    /// <summary>
    /// 更新背包数据（参数：有3个int bool int 第一个是道具ID， 第二个true是增加false是减少 第三个是对应的数量）
    /// </summary>
    UpdateBagItemEvent,

    /// <summary>
    /// 更新背包建筑数据数据（参数：有3个int bool int 第一个是道具建筑ID， 第二个是true增加还是false减少 第三个是对应的数量）
    /// </summary>
    UpdateBagBuildingItemEvent,

    /// <summary>
    /// 道具使用了.参数1：道具ID；参数2：使用个数
    /// </summary>
    ItemUsed,
    #endregion

    #region 每日事件
    /// <summary>
    /// 事件刷新 （没有参数）
    /// </summary>
    RefreshDailyTask,
    /// <summary>
    /// 事件状态改变（参数一个： DailyTaskInfo）
    /// </summary>
    DailyTaskStateChange,
    /// <summary>
    /// 排序（参数一个： DailyTaskWidget）
    /// </summary>
    DailyTaskWidgetSort,
    #endregion



    #region 剧情
    StoryStart,//id
    StoryOver,//id
    #endregion
    #region 装备
    /// <summary>
    /// 铁匠铺更新:建造、升级、进阶完成
    /// </summary>
    ForgeShopUpdated,
    #endregion

    #region 好友
    /// <summary>
    /// 更新好友widget（参数一个：FriendInfo）
    /// </summary>
    UpdateFriendWidget,

    /// <summary>
    /// 更新好友在线状体（参数 string角色ID）
    /// </summary>
    OnLineStateChange,
    /// <summary>
    /// 更新好友随机事件（参数 string好友ID）
    /// </summary>
    UpdateFriendRandomEvent,
    /// <summary>
    /// 更新好友的拜访英雄（参数 string好友ID）
    /// </summary>
    UpdateFriendVisitHero,
    #endregion

    /// <summary>
    /// 更新玩家游戏币和付费币（没有参数）
    /// </summary>
    UpdateCoinAndMoney,
    /// <summary>
    /// 关闭选择窗口（没有参数）
    /// </summary>
    HideSelectNotice,

    #region 公会
    /// <summary>
    /// 创建了一个公会
    /// </summary>
    GuildCreated,

    /// <summary>
    /// 有人加入公会. value: MemberInfo
    /// </summary>
    GuildJoined,

    /// <summary>
    /// 公会解散
    /// </summary>
    GuildDismissed,

    /// <summary>
    /// 收到了加入请求
    /// </summary>
    GuildReceiveJoinReq,

    /// <summary>
    /// 有人退出了公会
    /// </summary>
    GuildQuitted,

    /// <summary>
    /// 有人被任命
    /// </summary>
    GuildNominated,

    /// <summary>
    /// 有人被禅让
    /// </summary>
    GuildTransfered,

    /// <summary>
    /// 有人被踢出
    /// </summary>
    GuildKickedOut,

    /// <summary>
    /// 修改公会信息
    /// </summary>
    GuildModify,

    /// <summary>
    /// 公会任务
    /// </summary>
    GuildQuest,
    #endregion
    #region 竞技场
    /// <summary>
    /// 竞技场排名改变
    /// </summary>
    ArenaRank,
    #endregion
}