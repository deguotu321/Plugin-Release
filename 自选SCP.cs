using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Interfaces;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using MEC;
using PlayerRoles;
using UnityEngine;

namespace EpsilonRealityStabilizer
{
    public class EpsilonRealityStabilizer : Plugin<Config>
    {
        public override string Author => "SCP基金会现实科学部";
        public override string Name => "艾普西龙现实稳定系统";
        public override string Prefix => "EPSILON-STABILIZER";
        public override Version Version => new Version(5, 13, 5);
        public override Version RequiredExiledVersion => new Version(8, 9, 11);

        public static EpsilonRealityStabilizer Instance;

        private CoroutineHandle _countdownCoroutine;
        private float _remainingTime;
        private bool _isStabilizerActive;
        private int _lastBroadcastSecond = -1;

        private DateTime _lastDamageNotification = DateTime.MinValue;

        private readonly Dictionary<RoomType, string> _chineseRoomNames = new Dictionary<RoomType, string>
        {
            // 轻收容
            { RoomType.LczArmory, "轻收容军械库" },
            { RoomType.LczCafe, "轻收容餐厅" },
            { RoomType.LczPlants, "温室" },
            { RoomType.LczToilets, "洗手间" },
            { RoomType.Lcz173, "SCP-173收容室" },
            { RoomType.LczClassDSpawn, "D级人员宿舍" },
            { RoomType.LczCheckpointA, "检查点A" },
            { RoomType.LczCheckpointB, "检查点B" },
            { RoomType.LczGlassBox, "玻璃房" },
            { RoomType.Lcz914, "SCP-914房间" },
            { RoomType.Lcz330, "SCP-330糖果机" },

            // 重收容
            { RoomType.Hcz049, "SCP-049收容室" },
            { RoomType.Hcz079, "SCP-079主控室" },
            { RoomType.Hcz106, "SCP-106收容室" },
            { RoomType.Hcz939, "SCP-939收容室" },
            { RoomType.HczNuke, "核弹井" },
            { RoomType.HczCrossing, "十字路口" },
            { RoomType.HczTestRoom, "测试房间" },
            { RoomType.Hcz096, "SCP-096收容室" },
            { RoomType.HczServerRoom, "服务器室" },
            { RoomType.HczArmory, "重收容军械库" },
            { RoomType.HczHid, "HID" },

            // 入口区
            { RoomType.EzVent, "通风管道" },
            { RoomType.EzGateA, "大门A" },
            { RoomType.EzGateB, "大门B" },
            { RoomType.EzCollapsedTunnel, "坍塌隧道" },
            { RoomType.EzIntercom, "对讲机室" },
            { RoomType.EzDownstairsPcs, "办公室" },
            { RoomType.EzUpstairsPcs, "上层办公室" },
            { RoomType.EzCrossing, "检查点走廊" },
            { RoomType.EzShelter, "避难所" },

            // 其他
            { RoomType.Pocket, "口袋维度" },
            { RoomType.Surface, "地表" },
            { RoomType.HczElevatorA, "重收容电梯A" },
            { RoomType.HczElevatorB, "重收容电梯B" }
        };

        public override void OnEnabled()
        {
            Instance = this;

            Exiled.Events.Handlers.Server.RoundStarted += OnRoundStarted;
            Exiled.Events.Handlers.Player.Died += OnPlayerDied;
            Exiled.Events.Handlers.Player.Hurting += OnPlayerHurting;
            Exiled.Events.Handlers.Server.RoundEnded += OnRoundEnded;

            Log.Info("艾普西龙现实稳定系统已加载 - 授权级别: OMEGA");
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted;
            Exiled.Events.Handlers.Player.Died -= OnPlayerDied;
            Exiled.Events.Handlers.Player.Hurting -= OnPlayerHurting;
            Exiled.Events.Handlers.Server.RoundEnded -= OnRoundEnded;

            if (_countdownCoroutine.IsRunning)
                Timing.KillCoroutines(_countdownCoroutine);

            Instance = null;
            base.OnDisabled();
        }

        private void OnRoundStarted()
        {
            // 随机初始时间 (160-280秒)
            _remainingTime = UnityEngine.Random.Range(Config.MinInitialDuration, Config.MaxInitialDuration);
            _isStabilizerActive = true;
            _lastBroadcastSecond = -1;

            _countdownCoroutine = Timing.RunCoroutine(CountdownCoroutine());

            Timing.CallDelayed(3f, () =>
            {
                string message =
                    $"<size=30><color=#3498DB>■ <color=#2E86C1>基金会通知</color> ■</color></size>\n" +
                    $"<size=25><color=#85C1E9>艾普西龙级现实稳定系统已激活</color></size>\n" +
                    $"<size=25><color=#F1C40F>稳定持续时间: <color=#F39C12>{_remainingTime}秒</color></color></size>\n" +
                    $"<size=25><color=#AAB7B8>所有人员注意: <color=#E5E7E9>现实异常波动期间禁止非授权活动</color></color></size>";

                ShowGlobalHint(message, 10f);
            });
        }

        private void OnRoundEnded(RoundEndedEventArgs ev)
        {
            _isStabilizerActive = false;
            if (_countdownCoroutine.IsRunning)
                Timing.KillCoroutines(_countdownCoroutine);
        }

        private void OnPlayerDied(DiedEventArgs ev)
        {
            if (!_isStabilizerActive || _remainingTime <= 0) return;

            Timing.CallDelayed(0.5f, () =>
            {
                if (!_isStabilizerActive || _remainingTime <= 0) return;
                if (ev.Player != null && !ev.Player.IsAlive)
                    StartRespawnCountdown(ev.Player);
            });
        }

        private void StartRespawnCountdown(Player player)
        {
            var randomRole = GetRandomRole();
            var spawnZone = GetZoneByRole(randomRole);
            var spawnRoom = GetRandomRoomInZone(spawnZone);
            var chineseRoomName = GetChineseRoomName(spawnRoom.Type);

            Timing.RunCoroutine(RespawnCountdownCoroutine(player, randomRole, spawnRoom, chineseRoomName));
        }

        private IEnumerator<float> RespawnCountdownCoroutine(Player player, RoleTypeId role, Room spawnRoom, string roomName)
        {
            float countdown = 5f;

            while (countdown > 0 && _isStabilizerActive && _remainingTime > 0)
            {
                if (player != null && player.IsConnected)
                {
                    string message =
                        $"<size=30><color=#27AE60>■ <color=#2ECC71>现实稳定系统</color> ■</color></size>\n" +
                        $"<size=25><color=#A9DFBF>你将在 <color=#2ECC71>{(int)countdown}秒</color> 后作为<color=#F1C40F>{GetRoleName(role)}</color>复活</color></size>\n" +
                        $"<size=25><color=#ABEBC6>位置: <color=#27AE60>{roomName}</color></color></size>";

                    player.ShowHint(message, 1.1f);
                }

                countdown -= 1f;
                yield return Timing.WaitForSeconds(1f);
            }

            if (_isStabilizerActive && _remainingTime > 0 && player != null && !player.IsAlive)
                RespawnPlayer(player, role, spawnRoom);
        }

        private void OnPlayerHurting(HurtingEventArgs ev)
        {
            if (!_isStabilizerActive || _remainingTime <= 0 || ev.Amount <= 0 || ev.Player == null)
                return;

            if ((DateTime.Now - _lastDamageNotification).TotalSeconds < 2.5f)
                return;

            _remainingTime = Mathf.Max(0, _remainingTime - 1);
            _lastDamageNotification = DateTime.Now;

            if (Config.ShowDamageNotifications)
            {
                Timing.CallDelayed(0.1f, () =>
                {
                    string message =
                        $"<size=30><color=#E74C3C>■ <color=#C0392B>系统警报</color> ■</color></size>\n" +
                        $"<size=25><color=#EC7063>现实稳定系统受到时空扰动!</color></size>\n" +
                        $"<size=25><color=#F1948A>持续时间减少: <color=#E74C3C>1秒</color> | 剩余: <color=#F39C12>{_remainingTime}秒</color></color></size>";

                    ShowGlobalHint(message, 3f);
                });
            }
        }

        private void RespawnPlayer(Player player, RoleTypeId role, Room spawnRoom)
        {
            if (player == null || !_isStabilizerActive || _remainingTime <= 0) return;

            player.Role.Set(role, Exiled.API.Enums.SpawnReason.Respawn, RoleSpawnFlags.All);
            player.Position = spawnRoom.Position + Vector3.up * 2f;
            player.Health = player.MaxHealth;

            if (Config.ShowRespawnNotifications)
            {
                Timing.CallDelayed(0.5f, () =>
                {
                    if (player != null && player.IsConnected)
                    {
                        string chineseRoomName = GetChineseRoomName(spawnRoom.Type);
                        string roleName = GetRoleName(role);

                        string message =
                            $"<size=30><color=#2ECC71>■ <color=#27AE60>人事调整通知</color> ■</color></size>\n" +
                            $"<size=25><color=#ABEBC6>你已被重新分配到: <color=#F1C40F>{roleName}</color></color></size>\n" +
                            $"<size=25><color=#A9DFBF>复活位置: <color=#27AE60>{chineseRoomName}</color></color></size>\n" +
                            $"<size=25><color=#AAB7B8>继续履行基金会职责</color></size>";

                        player.ShowHint(message, 6f);
                    }
                });
            }
        }

        private RoleTypeId GetRandomRole()
        {
            float randomValue = UnityEngine.Random.Range(0f, 100f);

            if (randomValue <= 55f) // D级人员 55%
                return RoleTypeId.ClassD;

            if (randomValue <= 80f) // 科学家 25% (55+25=80)
                return RoleTypeId.Scientist;

            return RoleTypeId.FacilityGuard; // 安保人员 20%
        }

        private string GetRoleName(RoleTypeId role)
        {
            switch (role)
            {
                case RoleTypeId.ClassD: return "D级人员";
                case RoleTypeId.Scientist: return "科学家";
                case RoleTypeId.FacilityGuard: return "安保人员";
                default: return role.ToString();
            }
        }

        private ZoneType GetZoneByRole(RoleTypeId role)
        {
            switch (role)
            {
                case RoleTypeId.FacilityGuard:
                    return ZoneType.HeavyContainment;
                default:
                    return ZoneType.LightContainment;
            }
        }

        private Room GetRandomRoomInZone(ZoneType zone)
        {
            try
            {
                var validRooms = Room.List
                    .Where(room => room.Zone == zone && room.Type != RoomType.Unknown)
                    .ToList();

                return validRooms.Count > 0
                    ? validRooms[UnityEngine.Random.Range(0, validRooms.Count)]
                    : Room.List.FirstOrDefault();
            }
            catch
            {
                return Room.List.FirstOrDefault();
            }
        }

        private string GetChineseRoomName(RoomType roomType)
        {
            return _chineseRoomNames.TryGetValue(roomType, out string name) ? name : roomType.ToString();
        }

        private IEnumerator<float> CountdownCoroutine()
        {
            while (_remainingTime > 0 && _isStabilizerActive)
            {
                int currentSecond = Mathf.FloorToInt(_remainingTime);

                // 每20秒广播一次（排除最后10秒）
                if (currentSecond % 20 == 0 && currentSecond > 10 && currentSecond != _lastBroadcastSecond)
                {
                    _lastBroadcastSecond = currentSecond;
                    ShowGlobalHint(
                        $"<size=25><color=#3498DB>现实稳定剩余: <color=#2E86C1>{currentSecond}秒</color></color></size>",
                        2f
                    );
                }
                // 最后10秒每秒广播
                else if (currentSecond <= 10)
                {
                    string color = currentSecond > 5 ? "#F1C40F" : "#E74C3C";

                    ShowGlobalHint(
                        $"<size=30><color=#F39C12>■ <color={color}>警告</color> ■</color></size>\n" +
                        $"<size=25><color=#F5B041>稳定系统即将失效: <color={color}>{currentSecond}秒</color></color></size>\n" +
                        $"<size=25><color=#F9E79F>准备应对现实重构事件</color></size>",
                        1f
                    );
                }

                yield return Timing.WaitForSeconds(1f);
                _remainingTime -= 1f;
            }

            _isStabilizerActive = false;

            Timing.CallDelayed(1f, () =>
            {
                string message =
                    $"<size=30><color=#3498DB>■ <color=#2E86C1>系统通告</color> ■</color></size>\n" +
                    $"<size=25><color=#85C1E9>艾普西龙现实稳定系统已停用</color></size>\n" +
                    $"<size=25><color=#F1C40F>现实基准线恢复正常参数</color></size>\n" +
                    $"<size=25><color=#AAB7B8>恢复标准操作流程</color></size>";

                ShowGlobalHint(message, 10f);
            });
        }

        private void ShowGlobalHint(string message, float duration)
        {
            foreach (Player player in Player.List)
                player.ShowHint(message, duration);
        }
    }

    public class Config : IConfig
    {
        [Description("是否启用现实稳定系统")]
        public bool IsEnabled { get; set; } = true;

        [Description("稳定系统初始持续时间最小值 (秒)")]
        public float MinInitialDuration { get; set; } = 190f;

        [Description("稳定系统初始持续时间最大值 (秒)")]
        public float MaxInitialDuration { get; set; } = 280f;

        [Description("是否显示时空扰动通知")]
        public bool ShowDamageNotifications { get; set; } = true;

        [Description("是否显示人事调整通知")]
        public bool ShowRespawnNotifications { get; set; } = true;

        [Description("调试模式")]
        public bool Debug { get; set; } = false;
    }
}