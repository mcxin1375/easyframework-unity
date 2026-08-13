/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using System;
using UnityEngine;

namespace EasyFramework.Editor
{
    public class SVNExtension : IToolEvent<AssetImporter>, IToolEvent<AssetCreator>
    {
        public int Order => int.MaxValue;

        public void OnExecute()
        {
            CommitBySettings();
        }

        public void OnExecuteAfter()
        {
            CommitBySettings();
        }

        public static void Update(bool cleanup = false, bool revert = false, bool deleteUnversionedFiles = false)
        {
            var settings = SVNExtensionSettings.Instance;
            if (settings.updateDirectories?.Length > 0)
            {
                foreach (var directory in settings.updateDirectories)
                {
                    SVNCommand.Update(directory, 0, cleanup, revert, deleteUnversionedFiles, Debug.Log);
                }
            }
        }

        public static void CommitBySettings()
        {
            if (!IsActive()) return;

            var settings = SVNExtensionSettings.Instance;
            if (settings.commitDirectories?.Length > 0)
            {
                // SVNCommand.CommitAll(settings.commitDirectories[0], "SVNExtension", Debug.Log);
                SVNCommand.CommitAll(settings.commitDirectories, "SVNExtension", Debug.Log);
            }
        }

        private static bool IsActive()
        {
            var settings = SVNExtensionSettings.Instance;
            if (Application.isBatchMode)
            {
                return settings.batchModeEnabled;
            }
            else
            {
                return settings.editorEnabled;
            }
        }
    }
}