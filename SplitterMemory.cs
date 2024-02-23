using LiveSplit.Memory;
using System;
using System.Diagnostics;
namespace LiveSplit.JumpKing {
    //.load C:\Windows\Microsoft.NET\Framework\v4.0.30319\SOS.dll
    public partial class SplitterMemory {
        private static ProgramPointer SaveManager = new ProgramPointer(AutoDeref.Single, new ProgramSignature(PointerVersion.Steam, "5756534883EC60C5F877488BF148B9????????????????488B098B0948B9", 13));
        private static ProgramPointer IStatInfo = new ProgramPointer(AutoDeref.Single, new ProgramSignature(PointerVersion.Steam, "564883EC20C5F8778B414485C0744183F801755D48B8", 19));
        private static ProgramPointer Camera = new ProgramPointer(AutoDeref.None, new ProgramSignature(PointerVersion.Steam, "833D????????007D0AC705????????????????8B05", 22));
        public Process Program { get; set; }
        public bool IsHooked { get; set; } = false;
        public DateTime LastHooked;

        public SplitterMemory() {
            LastHooked = DateTime.MinValue;
        }
        public string RAMPointers() {
            return SaveManager.GetPointer(Program).ToString("X");
        }
        public string RAMPointerVersion() {
            return SaveManager.Version.ToString();
        }
        public int PlayerEntity() {
            //SaveManager.instance.m_player
            return SaveManager.Read<int>(Program, 0x0, 0x8);
        }
        public void TeleportPlayer(Screen screen, float x, float y) {
            byte[] data = new byte[16];
            byte[] temp = BitConverter.GetBytes(x);
            Array.Copy(temp, 0, data, 0, 4);
            temp = BitConverter.GetBytes(y);
            Array.Copy(temp, 0, data, 4, 4);
            temp = BitConverter.GetBytes(0.26f);
            Array.Copy(temp, 0, data, 12, 4);

            //SaveManager.instance.m_player.m_body.position.X/Y & .velocity.X/Y
            SaveManager.Write(Program, data, 0x0, 0x8, 0x18, 0x70);
            Camera.Write<int>(Program, (int)screen, 0x0, 0x0);
        }

        // SaveManager.instance.
        // m_player         008
        // m_body           018

        // Position         070
        // Velocity         078

        public Screen PlayerScreen() {
            // Camera.CurrentScreenIndex1
            return (Screen)Camera.Read<int>(Program, 0x0, 0x0);
        }
        public float PlayerX() {
            //SaveManager.instance.m_player.m_body.position.X
            return SaveManager.Read<float>(Program, 0x0, 0x8, 0x18, 0x70);
        }
        public float PlayerY() {
            //SaveManager.instance.m_player.m_body.position.Y
            return SaveManager.Read<float>(Program, 0x0, 0x8, 0x18, 0x74);
        }
        public float GameTime() {
            //AchievementManager.instance.m_all_time_stats._ticks (68 = 40 + 28)
            int allTime = IStatInfo.Read<int>(Program, 0x0, 0x68);
            //AchievementManager.instance.m_snapshot._ticks (38 = 10 + 28)
            int snapshot = IStatInfo.Read<int>(Program, 0x0, 0x38);
            return (allTime - snapshot) * 0.017f;
        }
        public int TimesWon() {
            //AchievementManager.instance.m_all_time_stats.times_won (64 = 40 + 24)
            return IStatInfo.Read<int>(Program, 0x0, 0x64);
        }
        public bool HookProcess() {
            IsHooked = Program != null && !Program.HasExited;
            if (!IsHooked && DateTime.Now > LastHooked.AddSeconds(1)) {
                LastHooked = DateTime.Now;
                Process[] processes = Process.GetProcessesByName("Jumpking");
                Program = processes != null && processes.Length > 0 ? processes[0] : null;

                if (Program != null && !Program.HasExited) {
                    MemoryReader.Update64Bit(Program);
                    IsHooked = true;
                }
            }

            return IsHooked;
        }
        public void Dispose() {
            if (Program != null) {
                Program.Dispose();
            }
        }
    }
}