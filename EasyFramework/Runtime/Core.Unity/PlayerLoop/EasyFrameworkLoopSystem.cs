/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2025/7/11
// describe:
//----------------------------------------------------------------*/

using System;
using UnityEngine.LowLevel;

namespace EasyFramework
{
    public enum UnityLoopTiming
    {
        EarlyUpdate,
        PreUpdate,
        Update,
        PreLateUpdate,
        PostLateUpdate,
        FixedUpdate,
    }
    
    public static class EasyFrameworkLoopSystem
    {
        struct EarlyUpdate { }
        struct PreUpdate { }
        struct Update { }
        struct PreLateUpdate { }
        struct PostLateUpdate { }
        struct FixedUpdate { }

        public static void Add(UnityLoopTiming timing, PlayerLoopSystem.UpdateFunction func)
        {
            switch (timing)
            {
                case UnityLoopTiming.EarlyUpdate:
                    AddPlayerLoop(typeof(UnityEngine.PlayerLoop.EarlyUpdate), typeof(EarlyUpdate), func);
                    break;
                case UnityLoopTiming.PreUpdate:
                    AddPlayerLoop(typeof(UnityEngine.PlayerLoop.PreUpdate), typeof(PreUpdate), func);
                    break;
                case UnityLoopTiming.Update:
                    AddPlayerLoop(typeof(UnityEngine.PlayerLoop.Update), typeof(Update), func);
                    break;
                case UnityLoopTiming.PreLateUpdate:
                    AddPlayerLoop(typeof(UnityEngine.PlayerLoop.PreLateUpdate), typeof(PreLateUpdate), func);
                    break;
                case UnityLoopTiming.PostLateUpdate:
                    AddPlayerLoop(typeof(UnityEngine.PlayerLoop.PostLateUpdate), typeof(PostLateUpdate), func);
                    break;
                case UnityLoopTiming.FixedUpdate:
                    AddPlayerLoop(typeof(UnityEngine.PlayerLoop.FixedUpdate), typeof(FixedUpdate), func);
                    break;
            }
        }
        
        private static void AddPlayerLoop(Type playerLoopType, Type systemType, PlayerLoopSystem.UpdateFunction updateFunction)
        {
            AddPlayerLoop(playerLoopType, new PlayerLoopSystem
            {
                type = systemType,
                updateDelegate = updateFunction
            });
        }
        
        private static void AddPlayerLoop(Type playerLoopType, PlayerLoopSystem addSystem)
        {
            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            for (int i = 0; i < playerLoop.subSystemList.Length; i++)
            {
                if (playerLoop.subSystemList[i].type == playerLoopType)
                {
                    var newSubSystems = new PlayerLoopSystem[playerLoop.subSystemList[i].subSystemList.Length + 1];
                    newSubSystems[0] = addSystem;
                    Array.Copy(
                        playerLoop.subSystemList[i].subSystemList, 
                        0, 
                        newSubSystems, 
                        1, 
                        playerLoop.subSystemList[i].subSystemList.Length
                    );

                    playerLoop.subSystemList[i].subSystemList = newSubSystems;
                    PlayerLoop.SetPlayerLoop(playerLoop);
                    return;
                }
            }
        }

    }
}