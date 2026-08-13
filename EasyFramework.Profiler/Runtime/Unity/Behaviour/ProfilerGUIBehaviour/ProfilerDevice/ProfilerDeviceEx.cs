using System.Reflection;
using System.Text;
using UnityEngine;

namespace EasyFramework.Profiler
{
    public class ProfilerDeviceEx
    {
        public float AwakeTime;
        public float StartBatteryLevel;
 
        private StringBuilder sb = new StringBuilder();

        public DeviceProfilerInfo GetProfilerInfo()
        {
            var deviceInfo = new DeviceProfilerInfo();
            
            var fieldInfos = typeof(DeviceProfilerInfo).GetFields(BindingFlags.Instance | BindingFlags.Public);
            foreach (var fieldInfo in fieldInfos)
            {
                var info = typeof(SystemInfo).GetProperty(fieldInfo.Name, BindingFlags.Static | BindingFlags.Public);
                if (info != null) fieldInfo.SetValue(deviceInfo, info.GetValue(null));
            }

            deviceInfo.startBatteryLevel = StartBatteryLevel;
            deviceInfo.batteryLevel = deviceInfo.batteryLevel < 0 ? 100 : (int)(deviceInfo.batteryLevel * 100);
            deviceInfo.monitorDurationSeconds = (long)(Time.realtimeSinceStartup - AwakeTime);

            return deviceInfo;  
        }

        public string DeviceToString()
        {
            int duraSeconds = (int)(Time.realtimeSinceStartup - AwakeTime);
            int curBatteryLevel = SystemInfo.batteryLevel < 0 ? 100 : (int)(SystemInfo.batteryLevel * 100);

            sb.Clear();
            sb.AppendLine($" 硬件&软件 (运行时长: {duraSeconds.FormatTimeString()})");
            sb.AppendLine($" 操作系统: {SystemInfo.operatingSystem}");
            sb.AppendLine($" CPU: {SystemInfo.processorType}  {SystemInfo.processorFrequency}MHz");
            sb.AppendLine($" GPU: {SystemInfo.graphicsDeviceName}");
            sb.AppendLine($" 核心数: {SystemInfo.processorCount} 内存: {SystemInfo.systemMemorySize}MB  显存: {SystemInfo.graphicsMemorySize}MB");
            sb.AppendLine($" 图形版本: {SystemInfo.graphicsDeviceVersion} 着色器级别: {SystemInfo.graphicsShaderLevel}");
            sb.AppendLine($" 耗电量（当前/初始): {curBatteryLevel} / {(int)StartBatteryLevel}");
            sb.AppendLine($" 设备uid: {SystemInfo.deviceUniqueIdentifier}");
            return sb.ToString();  
        }
    }
}