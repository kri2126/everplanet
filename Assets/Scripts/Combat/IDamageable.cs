namespace Everplanet.Combat
{
    /// <summary>
    /// 데미지를 받을 수 있는 모든 대상(플레이어, 몬스터)이 구현하는 인터페이스.
    /// 공격 판정을 하는 쪽은 구체 타입을 몰라도 이 인터페이스만으로 데미지를 줄 수 있다.
    /// </summary>
    public interface IDamageable
    {
        int CurrentHP { get; }
        int MaxHP { get; }
        bool IsDead { get; }

        /// <summary>공격력(atk)을 넘겨주면 내부적으로 자신의 방어력을 적용해 실 데미지를 계산한다.</summary>
        void TakeDamage(int attackerPower);
    }
}
