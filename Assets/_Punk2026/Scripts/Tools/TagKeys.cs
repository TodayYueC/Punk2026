namespace Punk2026.Tools
{
    /// <summary>
    /// 标签常量定义 —— 集中管理所有 Unity Tag 字符串
    /// 避免在代码中硬编码字符串，防止拼写错误导致的运行时 Bug
    /// 使用方式：CompareTag(TagKeys.Player)
    /// </summary>
    public static class TagKeys
    {
        /// <summary>玩家标签</summary>
        public const string Player = "Player";

        /// <summary>敌人标签</summary>
        public const string Enemy = "Enemy";

        /// <summary>危险区域/陷阱标签</summary>
        public const string Hazard = "Hazard";

        /// <summary>投射物标签（子弹等）</summary>
        public const string Projectile = "Projectile";
    }
}
