/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using UnityEditor;
using UnityEditor.Build.Reporting;

namespace EasyFramework.Editor
{
    public interface IPlayerBuilderExtension : IToolExtension
    {
        void OnBuildReport(BuildReport report);
    }

    public interface IPlayerBuilderSettings : IToolExtension
    {
        BuildPlayerOptions BuildPlayerOptions { get; }
    }

    public partial class PlayerBuilder : ToolBase<PlayerBuilder>
    {
        public override int Order => ToolOrder.PlayerBuilder;

        public IPlayerBuilderExtension[] ToolExtensions => ToolExtension<IPlayerBuilderExtension>.Instances;
        public IPlayerBuilderSettings[] ToolSettings => ToolExtension<IPlayerBuilderSettings>.Instances;

        protected override void OnSelfExecuteBefore() => PlayerBuilderUtility.PreInitPlayerSettings();
        protected override void OnSelfExecute() => PlayerBuilderUtility.BuildBySettings();

        [MenuItem("EasyFramework/Tools/PlayerBuilder - Execute", priority = ToolOrder.PlayerBuilder)]
        private static void MenuItem1() => Instance.Execute();
        
        // [MenuItem("EasyFramework/Tools/PlayerBuilder - BuildMainRes", priority = EasyFrameworkToolsSettings.PlayerBuilder + 1)]
        // public static void MenuItem2() => PlayerBuilder.Instance.BuildMainRes();
        //
        // [MenuItem("EasyFramework/Tools/PlayerBuilder - BuildPlayer", priority = EasyFrameworkToolsSettings.PlayerBuilder + 1)]
        // public static void MenuItem3() => PlayerBuilder.Instance.BuildPlayer();
        //
        // [MenuItem("EasyFramework/Tools/PlayerBuilder - BuildProject", priority = EasyFrameworkToolsSettings.PlayerBuilder + 1)]
        // public static void MenuItem4() => PlayerBuilder.Instance.BuildProject();
    }
}