using System.Collections.Generic;

namespace SPICA.CLI
{
    class MotionLexicon
    {
        // public static Dictionary<string, string> StandardMotion = new Dictionary<string, string>() {
        //     { "idle", "Motion_0"},
        //     { "attack-close", "Motion_6"},
        //     { "attack-range", "Motion_9"},
        //     { "hit", "Motion_13"},
        //     { "faint", "Motion_17"},
        //     { "celebrate", "Motion_10_1"},
        //     { "walk", "Motion_1_2"},
        //     { "run", "Motion_3_2"},
        // };
        public static Dictionary<string, string> StandardMotion = new Dictionary<string, string>() {
            { "idle", "FightingAction1"},
            { "attack", "FightingAction9"},
            { "attack-2", "FightingAction10"},
            { "attack-3", "FightingAction11"},
            { "attack-4", "FightingAction12"},
            { "range-attack", "FightingAction13"},
            { "range-attack-2", "FightingAction14"},
            { "range-attack-3", "FightingAction15"},
            { "range-attack-4", "FightingAction16"},
            { "hit", "FightingAction17"},
            { "lost", "FightingAction18"},
            //{ "falling-asleep", "PetAction5"},
            //{ "sleepy", "PetAction6"},
            { "sleeping", "PetAction8"},
            { "happy", "PetAction13"},
            { "happy-2", "PetAction21"},
            { "walk", "MapAction3"},
            { "run", "MapAction4"},
            // { "idle", "Motion_0"},
            // { "jump", "Motion_3"},
            // { "airborn", "Motion_4"},
            // { "fall", "Motion_5"},
            // { "attack1", "Motion_6"},
            // { "attack1_1", "Motion_7"},
            // { "attack2", "Motion_9"},
            // { "hit1_", "Motion_13"},
            // { "hit2_", "Motion_14"},
            // { "faint", "Motion_17"},
            // { "sleep1_", "Motion_5_1"},
            // { "sleep2_", "Motion_7_1"},
            // { "wakeUp_", "Motion_8_1"},
            // { "celebrate", "Motion_10_1"},
            // { "walk", "Motion_1_2"},
            // { "run", "Motion_3_2"},
        };
    }
}
