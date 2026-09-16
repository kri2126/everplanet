using UnityEngine;

namespace Everplanet.Data
{
    /// <summary>
    /// 행성 하나를 정의하는 데이터 에셋.
    /// Project 창에서 우클릭 -> Create -> Everplanet -> Planet Definition 으로 생성해서
    /// 에버그린 / 이타카 / 노바루나 3개를 각각 만들어 사용한다.
    /// (초기 Unity 다중 행성 계획 기준. 현재 웹 구현은 에버그린 한 무대만 다룬다.)
    /// </summary>
    [CreateAssetMenu(fileName = "NewPlanet", menuName = "Everplanet/Planet Definition")]
    public class PlanetDefinition : ScriptableObject
    {
        [Header("기본 정보")]
        public string planetName = "New Planet";
        [TextArea]
        public string description;

        [Header("씬 연결")]
        [Tooltip("Build Settings에 등록된 이 행성의 씬 이름")]
        public string sceneName;

        [Header("입장 조건")]
        [Tooltip("이 행성에 입장하기 위해 필요한 최소 레벨")]
        public int requiredLevel = 1;

        [Header("귀환 지점")]
        [Tooltip("이 행성에서 사망(리스폰) 시 돌아갈 행성. 비워두면 마이 플래닛으로 간주")]
        public PlanetDefinition respawnPlanet;
    }
}
