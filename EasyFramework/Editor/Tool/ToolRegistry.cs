/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2025/1/22
// describe:
//----------------------------------------------------------------*/

using System.Linq;

namespace EasyFramework.Editor
{
    public static class ToolRegistry
    {
        private static ITool[] _tools;
        public static ITool[] Tools => _tools ??= InitTools();

        private static ITool[] InitTools() => EasyFrameworkReflection.FindInstanceTypes<ITool>()
                .Select(type => type.FindFieldOrProperty<ITool>())
                .OrderBy(tool => tool.Order)
                .ToArray();
    }
}
