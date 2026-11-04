using System;
using System.Collections.Generic;
using EasyFramework.Editor;
using UnityEngine;

namespace EasyFramework.Tests
{
    // Discovered only while the test creates its temporary asset. Normal builds have no instance.
    public sealed class BuildLifecycleTestExtension : ScriptableObject,
        IToolEvent<AssetBuilder>, IToolEvent<PlayerBuilder>, IToolEvent<DLCBuilder>
    {
        public static readonly List<string> Calls = new();
        public static string FailurePhase;

        public int Order => int.MinValue;

        void IToolEvent<AssetBuilder>.OnExecuteBefore() => Record("Asset.Before");
        void IToolEvent<AssetBuilder>.OnExecute() => Record("Asset.Execute");
        void IToolEvent<AssetBuilder>.OnExecuteAfter() => Record("Asset.After");
        void IToolEvent<PlayerBuilder>.OnExecuteBefore() => Record("Player.Before");
        void IToolEvent<PlayerBuilder>.OnExecute() => Record("Player.Execute");
        void IToolEvent<PlayerBuilder>.OnExecuteAfter() => Record("Player.After");
        void IToolEvent<DLCBuilder>.OnExecuteBefore() => Record("DLC.Before");
        void IToolEvent<DLCBuilder>.OnExecute() => Record("DLC.Execute");
        void IToolEvent<DLCBuilder>.OnExecuteAfter() => Record("DLC.After");

        private static void Record(string phase)
        {
            Calls.Add(phase);
            if (phase == FailurePhase) throw new InvalidOperationException("Injected build failure: " + phase);
        }
    }
}
